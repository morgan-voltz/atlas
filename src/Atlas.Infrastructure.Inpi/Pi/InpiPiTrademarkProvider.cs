using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Infrastructure.Inpi.Common;
using Atlas.Infrastructure.Inpi.Pi.DTOs;
using Atlas.Shared.Result;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Inpi.Pi;

/// <summary>
/// Recherche de marques via l'API INPI PI. ⚠️ Best-effort : l'authentification PI (login → cookies
/// <c>access_token</c> + <c>XSRF-TOKEN</c>) et le contrat de <c>POST /services/apidiffusion/api/marques/search</c>
/// sont *supposés* et doivent être validés contre l'API réelle. Les identifiants ne sont jamais loggés.
/// Multi-tenant : les cookies sont gérés manuellement (handler configuré avec UseCookies=false).
/// </summary>
internal sealed class InpiPiTrademarkProvider(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptions<InpiOptions> options) : IIntellectualPropertyProvider
{
    private static readonly TimeSpan SessionMargin = TimeSpan.FromMinutes(1);

    public async Task<Result<PagedResult<TrademarkSummary>>> SearchTrademarksAsync(
        TrademarkSearchQuery query,
        InpiAccessCredentials credentials,
        CancellationToken ct = default)
    {
        Result<PiSession> session = await GetSessionAsync(credentials, forceRefresh: false, ct);
        if (session.IsFailure)
        {
            return Result<PagedResult<TrademarkSummary>>.Fail(session.Error!);
        }

        (Result<PagedResult<TrademarkSummary>> result, bool unauthorized) =
            await TrySearchAsync(query, session.Value!, ct);
        if (!unauthorized)
        {
            return result;
        }

        Result<PiSession> refreshed = await GetSessionAsync(credentials, forceRefresh: true, ct);
        if (refreshed.IsFailure)
        {
            return Result<PagedResult<TrademarkSummary>>.Fail(refreshed.Error!);
        }

        (result, _) = await TrySearchAsync(query, refreshed.Value!, ct);
        return result;
    }

    private async Task<(Result<PagedResult<TrademarkSummary>> Result, bool Unauthorized)> TrySearchAsync(
        TrademarkSearchQuery query,
        PiSession session,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "services/apidiffusion/api/marques/search")
        {
            Content = JsonContent.Create(new PiSearchRequest(query.Term, query.Page, query.PageSize)),
        };
        request.Headers.TryAddWithoutValidation("Cookie", $"access_token={session.AccessToken}");
        request.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", session.XsrfToken);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            return (Failure(InpiErrors.Unavailable), false);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return (Failure(InpiErrors.Unavailable), false);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return (Failure(InpiErrors.Unavailable), true);
            }

            if (!response.IsSuccessStatusCode)
            {
                return (Failure(InpiErrors.Unavailable), false);
            }

            PiTrademarkSearchResponse? body = await response.Content.ReadFromJsonAsync<PiTrademarkSearchResponse>(ct);
            List<TrademarkSummary> items = (body?.Results ?? []).Select(PiTrademarkMapper.Map).ToList();
            long total = body?.Total ?? items.Count;
            var page = new PagedResult<TrademarkSummary>(items, query.Page, query.PageSize, total);
            return (Result<PagedResult<TrademarkSummary>>.Ok(page), false);
        }

        static Result<PagedResult<TrademarkSummary>> Failure(Error error) =>
            Result<PagedResult<TrademarkSummary>>.Fail(error);
    }

    private async Task<Result<PiSession>> GetSessionAsync(
        InpiAccessCredentials credentials,
        bool forceRefresh,
        CancellationToken ct)
    {
        string cacheKey = "inpi:pi:" + credentials.Username;
        if (!forceRefresh && cache.TryGetValue(cacheKey, out PiSession? cached) && cached is not null)
        {
            return Result<PiSession>.Ok(cached);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "auth/login")
        {
            Content = JsonContent.Create(new PiLoginRequest(credentials.Username, credentials.Password)),
        };

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            return Result<PiSession>.Fail(InpiErrors.Unavailable);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return Result<PiSession>.Fail(InpiErrors.Unavailable);
        }

        using (response)
        {
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.BadRequest)
            {
                return Result<PiSession>.Fail(InpiErrors.InvalidCredentials);
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result<PiSession>.Fail(InpiErrors.Unavailable);
            }

            List<string> setCookies = response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values)
                ? values.ToList()
                : [];

            string? accessToken = ExtractCookie(setCookies, "access_token");
            if (accessToken is null)
            {
                return Result<PiSession>.Fail(InpiErrors.Unavailable);
            }

            string xsrf = ExtractCookie(setCookies, "XSRF-TOKEN") ?? string.Empty;
            var session = new PiSession(accessToken, xsrf, DateTimeOffset.UtcNow.Add(options.Value.TokenLifetimeFallback));

            TimeSpan ttl = session.ExpiresAt - DateTimeOffset.UtcNow - SessionMargin;
            if (ttl > TimeSpan.Zero)
            {
                cache.Set(cacheKey, session, ttl);
            }

            return Result<PiSession>.Ok(session);
        }
    }

    private static string? ExtractCookie(IEnumerable<string> setCookies, string name)
    {
        string prefix = name + "=";
        foreach (string cookie in setCookies)
        {
            string trimmed = cookie.TrimStart();
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                int start = prefix.Length;
                int end = trimmed.IndexOf(';', start);
                return end < 0 ? trimmed[start..] : trimmed[start..end];
            }
        }

        return null;
    }

    private sealed record PiSession(string AccessToken, string XsrfToken, DateTimeOffset ExpiresAt);

    private sealed record PiLoginRequest(
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("password")] string Password);

    private sealed record PiSearchRequest(
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("page")] int Page,
        [property: JsonPropertyName("size")] int Size);
}
