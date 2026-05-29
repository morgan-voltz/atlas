using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Atlas.Domain.Companies;
using Atlas.Domain.Companies.Attachments;
using Atlas.Domain.Inpi;
using Atlas.Shared.Result;
using Microsoft.Extensions.Caching.Memory;

namespace Atlas.Infrastructure.Inpi.Rne;

/// <summary>
/// Lecture entreprise via le RNE (fiche par SIREN, recherche par dénomination, actes & bilans).
/// Le token Bearer est mis en cache (par compte INPI) jusqu'à peu avant son expiration ; en cas
/// de 401, on ré-authentifie une fois.
/// </summary>
internal sealed class RneCompanyProvider(
    HttpClient httpClient,
    IInpiAuthenticationProvider authenticationProvider,
    IMemoryCache cache) : ICompanyDataProvider
{
    private static readonly TimeSpan TokenExpiryMargin = TimeSpan.FromMinutes(1);

    public Task<Result<UniteLegale>> GetBySirenAsync(
        Siren siren,
        InpiAccessCredentials credentials,
        CancellationToken ct = default) =>
        ExecuteAsync(credentials, (token, token2Ct) => TryGetBySirenAsync(siren, token, token2Ct), ct);

    public Task<Result<PagedResult<CompanySummary>>> SearchByNameAsync(
        CompanySearchQuery query,
        InpiAccessCredentials credentials,
        CancellationToken ct = default) =>
        ExecuteAsync(credentials, (token, innerCt) => TrySearchByNameAsync(query, token, innerCt), ct);

    public Task<Result<IReadOnlyList<CompanyAttachment>>> GetAttachmentsAsync(
        Siren siren,
        InpiAccessCredentials credentials,
        CancellationToken ct = default) =>
        ExecuteAsync(credentials, (token, innerCt) => TryGetAttachmentsAsync(siren, token, innerCt), ct);

    public Task<Result<AttachmentContent>> DownloadAttachmentAsync(
        Siren siren,
        string attachmentId,
        InpiAccessCredentials credentials,
        CancellationToken ct = default) =>
        ExecuteAsync(credentials, (token, innerCt) => TryDownloadAttachmentAsync(siren, attachmentId, token, innerCt), ct);

    /// <summary>Obtient un token (caché), exécute la tentative, et réessaie une fois après ré-auth sur 401.</summary>
    private async Task<Result<T>> ExecuteAsync<T>(
        InpiAccessCredentials credentials,
        Func<string, CancellationToken, Task<(Result<T> Result, bool Unauthorized)>> attempt,
        CancellationToken ct)
    {
        Result<string> token = await GetTokenAsync(credentials, forceRefresh: false, ct);
        if (token.IsFailure)
        {
            return Result<T>.Fail(token.Error!);
        }

        (Result<T> result, bool unauthorized) = await attempt(token.Value!, ct);
        if (!unauthorized)
        {
            return result;
        }

        Result<string> refreshed = await GetTokenAsync(credentials, forceRefresh: true, ct);
        if (refreshed.IsFailure)
        {
            return Result<T>.Fail(refreshed.Error!);
        }

        (result, _) = await attempt(refreshed.Value!, ct);
        return result;
    }

    private async Task<(Result<UniteLegale> Result, bool Unauthorized)> TryGetBySirenAsync(
        Siren siren,
        string token,
        CancellationToken ct)
    {
        (HttpResponseMessage? response, bool unauthorized, Error? transportError) =
            await SendAsync(HttpMethod.Get, $"companies/{siren.Value}", token, ct);

        if (response is null)
        {
            return (Result<UniteLegale>.Fail(transportError!), unauthorized);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (Result<UniteLegale>.Fail(CompanyErrors.NotFound(siren)), false);
            }

            if (!response.IsSuccessStatusCode)
            {
                return (Result<UniteLegale>.Fail(InpiErrors.Unavailable), false);
            }

            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            return body.ValueKind != JsonValueKind.Object
                ? (Result<UniteLegale>.Fail(InpiErrors.Unavailable), false)
                : (Result<UniteLegale>.Ok(RneCompanyMapper.Map(siren, body)), false);
        }
    }

    private async Task<(Result<PagedResult<CompanySummary>> Result, bool Unauthorized)> TrySearchByNameAsync(
        CompanySearchQuery query,
        string token,
        CancellationToken ct)
    {
        string url = $"companies?companyName={Uri.EscapeDataString(query.Term)}&page={query.Page}&pageSize={query.PageSize}";

        (HttpResponseMessage? response, bool unauthorized, Error? transportError) =
            await SendAsync(HttpMethod.Get, url, token, ct);

        if (response is null)
        {
            return (Result<PagedResult<CompanySummary>>.Fail(transportError!), unauthorized);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (Ok(EmptyPage(query)), false);
            }

            if (!response.IsSuccessStatusCode)
            {
                return (Result<PagedResult<CompanySummary>>.Fail(InpiErrors.Unavailable), false);
            }

            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            var summaries = new List<CompanySummary>();
            if (body.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in body.EnumerateArray())
                {
                    if (RneCompanyMapper.MapSummary(item) is { } summary)
                    {
                        summaries.Add(summary);
                    }
                }
            }

            var page = new PagedResult<CompanySummary>(summaries, query.Page, query.PageSize, summaries.Count);
            return (Ok(page), false);
        }

        static Result<PagedResult<CompanySummary>> Ok(PagedResult<CompanySummary> value) =>
            Result<PagedResult<CompanySummary>>.Ok(value);
    }

    private static PagedResult<CompanySummary> EmptyPage(CompanySearchQuery query) =>
        new([], query.Page, query.PageSize, 0);

    // ── F-013 : actes & bilans ──────────────────────────────────────────────────────

    private async Task<(Result<IReadOnlyList<CompanyAttachment>> Result, bool Unauthorized)> TryGetAttachmentsAsync(
        Siren siren,
        string token,
        CancellationToken ct)
    {
        (HttpResponseMessage? response, bool unauthorized, Error? transportError) =
            await SendAsync(HttpMethod.Get, $"companies/{siren.Value}/attachments", token, ct);

        if (response is null)
        {
            return (Result<IReadOnlyList<CompanyAttachment>>.Fail(transportError!), unauthorized);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (Result<IReadOnlyList<CompanyAttachment>>.Ok([]), false);
            }

            if (!response.IsSuccessStatusCode)
            {
                return (Result<IReadOnlyList<CompanyAttachment>>.Fail(InpiErrors.Unavailable), false);
            }

            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            return (Result<IReadOnlyList<CompanyAttachment>>.Ok(MapAttachments(body)), false);
        }
    }

    /// <summary>
    /// Mapping défensif : l'INPI peut renvoyer soit un tableau direct, soit un objet avec
    /// les sous-collections <c>actes</c> / <c>comptesAnnuels</c>. On accepte les deux formes.
    /// </summary>
    private static List<CompanyAttachment> MapAttachments(JsonElement body)
    {
        var result = new List<CompanyAttachment>();

        if (body.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in body.EnumerateArray())
            {
                if (MapSingleAttachment(item, AttachmentType.Other) is { } attachment)
                {
                    result.Add(attachment);
                }
            }
            return result;
        }

        if (body.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        AppendCategory(body, "actes", AttachmentType.Acte, result);
        AppendCategory(body, "comptesAnnuels", AttachmentType.Bilan, result);
        AppendCategory(body, "bilans", AttachmentType.Bilan, result);

        return result;
    }

    private static void AppendCategory(JsonElement body, string field, AttachmentType type, List<CompanyAttachment> sink)
    {
        if (!body.TryGetProperty(field, out JsonElement collection) || collection.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (JsonElement item in collection.EnumerateArray())
        {
            if (MapSingleAttachment(item, type) is { } attachment)
            {
                sink.Add(attachment);
            }
        }
    }

    private static CompanyAttachment? MapSingleAttachment(JsonElement element, AttachmentType defaultType)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        string? id = ReadString(element, "id") ?? ReadString(element, "numeroDocument");
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        string name = ReadString(element, "nom")
            ?? ReadString(element, "libelle")
            ?? ReadString(element, "typeRdd")
            ?? "Document";

        DateOnly? depositedAt = TryReadDate(element, "dateDepot")
            ?? TryReadDate(element, "dateCloture")
            ?? TryReadDate(element, "dateImmatriculation");

        long? sizeBytes = element.TryGetProperty("taille", out JsonElement sizeProp) && sizeProp.TryGetInt64(out long size)
            ? size
            : null;

        bool confidential = element.TryGetProperty("confidentialite", out JsonElement confProp)
            && confProp.ValueKind == JsonValueKind.True;

        AttachmentType type = defaultType;
        if (ReadString(element, "type") is { } rawType)
        {
            type = ClassifyType(rawType, defaultType);
        }

        return new CompanyAttachment(id!.Trim(), type, name.Trim(), depositedAt, sizeBytes, confidential);
    }

    private static AttachmentType ClassifyType(string rawType, AttachmentType fallback)
    {
        string normalized = rawType.ToLowerInvariant();
        if (normalized.Contains("bilan", StringComparison.Ordinal) || normalized.Contains("compte", StringComparison.Ordinal))
        {
            return AttachmentType.Bilan;
        }
        if (normalized.Contains("acte", StringComparison.Ordinal) || normalized.Contains("statut", StringComparison.Ordinal))
        {
            return AttachmentType.Acte;
        }
        return fallback;
    }

    private static string? ReadString(JsonElement element, string field) =>
        element.TryGetProperty(field, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static DateOnly? TryReadDate(JsonElement element, string field)
    {
        string? raw = ReadString(element, field);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }
        return DateOnly.TryParse(raw, CultureInfo.InvariantCulture, out DateOnly value) ? value : null;
    }

    private async Task<(Result<AttachmentContent> Result, bool Unauthorized)> TryDownloadAttachmentAsync(
        Siren siren,
        string attachmentId,
        string token,
        CancellationToken ct)
    {
        (HttpResponseMessage? response, bool unauthorized, Error? transportError) =
            await SendAsync(HttpMethod.Get, $"companies/{siren.Value}/attachments/{Uri.EscapeDataString(attachmentId)}/download", token, ct);

        if (response is null)
        {
            return (Result<AttachmentContent>.Fail(transportError!), unauthorized);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            response.Dispose();
            return (Result<AttachmentContent>.Fail(AttachmentErrors.NotFound), false);
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            response.Dispose();
            return (Result<AttachmentContent>.Fail(AttachmentErrors.Confidential), false);
        }

        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            return (Result<AttachmentContent>.Fail(InpiErrors.Unavailable), false);
        }

        // Le caller est responsable de disposer le Stream (et donc indirectement la response).
        Stream stream = await response.Content.ReadAsStreamAsync(ct);
        string contentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";
        string fileName = ExtractFileName(response, siren, attachmentId);

        return (Result<AttachmentContent>.Ok(new AttachmentContent(stream, contentType, fileName)), false);
    }

    private static string ExtractFileName(HttpResponseMessage response, Siren siren, string attachmentId)
    {
        string? fromHeader = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"');
        return string.IsNullOrWhiteSpace(fromHeader)
            ? $"{siren.Value}_{attachmentId}.pdf"
            : fromHeader;
    }

    /// <summary>Envoie une requête authentifiée. Retourne (réponse, unauthorized, erreur transport) — un seul est significatif.</summary>
    private async Task<(HttpResponseMessage? Response, bool Unauthorized, Error? TransportError)> SendAsync(
        HttpMethod method,
        string relativeUrl,
        string token,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, relativeUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            return (null, false, InpiErrors.Unavailable);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return (null, false, InpiErrors.Unavailable);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            return (null, true, InpiErrors.Unavailable);
        }

        return (response, false, null);
    }

    private async Task<Result<string>> GetTokenAsync(
        InpiAccessCredentials credentials,
        bool forceRefresh,
        CancellationToken ct)
    {
        string cacheKey = "inpi:token:" + credentials.Username;

        if (!forceRefresh && cache.TryGetValue(cacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
        {
            return Result<string>.Ok(cached);
        }

        Result<InpiSession> session =
            await authenticationProvider.AuthenticateAsync(credentials.Username, credentials.Password, ct);
        if (session.IsFailure)
        {
            return Result<string>.Fail(session.Error!);
        }

        InpiSession value = session.Value!;
        TimeSpan ttl = value.ExpiresAt - DateTimeOffset.UtcNow - TokenExpiryMargin;
        if (ttl > TimeSpan.Zero)
        {
            cache.Set(cacheKey, value.AccessToken, ttl);
        }

        return Result<string>.Ok(value.AccessToken);
    }
}
