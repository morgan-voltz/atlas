using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Atlas.Domain.Companies;
using Atlas.Domain.Inpi;
using Atlas.Infrastructure.Inpi.Rne.DTOs;
using Atlas.Shared.Result;
using Microsoft.Extensions.Caching.Memory;

namespace Atlas.Infrastructure.Inpi.Rne;

/// <summary>
/// Lecture entreprise via le RNE (fiche par SIREN, recherche par dénomination). Le token Bearer est
/// mis en cache (par compte INPI) jusqu'à peu avant son expiration ; en cas de 401, on ré-authentifie une fois.
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

            RneCompanyResponse? body = await response.Content.ReadFromJsonAsync<RneCompanyResponse>(ct);
            return body is null
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

            List<RneCompanyResponse>? items = await response.Content.ReadFromJsonAsync<List<RneCompanyResponse>>(ct);
            List<CompanySummary> summaries = (items ?? [])
                .Select(RneCompanyMapper.MapSummary)
                .OfType<CompanySummary>()
                .ToList();

            var page = new PagedResult<CompanySummary>(summaries, query.Page, query.PageSize, summaries.Count);
            return (Ok(page), false);
        }

        static Result<PagedResult<CompanySummary>> Ok(PagedResult<CompanySummary> value) =>
            Result<PagedResult<CompanySummary>>.Ok(value);
    }

    private static PagedResult<CompanySummary> EmptyPage(CompanySearchQuery query) =>
        new([], query.Page, query.PageSize, 0);

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
