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
/// Accès aux marques via l'API INPI PI (recherche, notice, image). ⚠️ Best-effort : l'authentification PI
/// (login → cookies <c>access_token</c> + <c>XSRF-TOKEN</c>) et les contrats des endpoints
/// (<c>/marques/search</c>, <c>/marques/notice/{id}</c>, <c>/marques/image/{id}</c>) sont *supposés* et
/// doivent être validés contre l'API réelle. Les identifiants ne sont jamais loggés. Multi-tenant : les
/// cookies sont gérés manuellement (handler configuré avec UseCookies=false).
/// </summary>
internal sealed class InpiPiTrademarkProvider(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptions<InpiOptions> options) : IIntellectualPropertyProvider
{
    private const string SearchPath = "services/apidiffusion/api/marques/search";
    private const string NoticePath = "services/apidiffusion/api/marques/notice/";
    private const string ImagePath = "services/apidiffusion/api/marques/image/";

    private static readonly TimeSpan SessionMargin = TimeSpan.FromMinutes(1);

    public Task<Result<PagedResult<TrademarkSummary>>> SearchTrademarksAsync(
        TrademarkSearchQuery query,
        InpiAccessCredentials credentials,
        CancellationToken ct = default) =>
        ExecuteAsync(credentials, (session, innerCt) => TrySearchAsync(query, session, innerCt), ct);

    public Task<Result<TrademarkDetail>> GetTrademarkAsync(
        DepositNumber depositNumber,
        InpiAccessCredentials credentials,
        CancellationToken ct = default) =>
        ExecuteAsync(credentials, (session, innerCt) => TryGetNoticeAsync(depositNumber, session, innerCt), ct);

    public Task<Result<TrademarkImage>> GetTrademarkImageAsync(
        DepositNumber depositNumber,
        InpiAccessCredentials credentials,
        CancellationToken ct = default) =>
        ExecuteAsync(credentials, (session, innerCt) => TryGetImageAsync(depositNumber, session, innerCt), ct);

    private async Task<Result<T>> ExecuteAsync<T>(
        InpiAccessCredentials credentials,
        Func<PiSession, CancellationToken, Task<(Result<T> Result, bool Unauthorized)>> attempt,
        CancellationToken ct)
    {
        Result<PiSession> session = await GetSessionAsync(credentials, forceRefresh: false, ct);
        if (session.IsFailure)
        {
            return Result<T>.Fail(session.Error!);
        }

        (Result<T> result, bool unauthorized) = await attempt(session.Value!, ct);
        if (!unauthorized)
        {
            return result;
        }

        Result<PiSession> refreshed = await GetSessionAsync(credentials, forceRefresh: true, ct);
        if (refreshed.IsFailure)
        {
            return Result<T>.Fail(refreshed.Error!);
        }

        (result, _) = await attempt(refreshed.Value!, ct);
        return result;
    }

    private async Task<(Result<PagedResult<TrademarkSummary>> Result, bool Unauthorized)> TrySearchAsync(
        TrademarkSearchQuery query,
        PiSession session,
        CancellationToken ct)
    {
        using HttpRequestMessage request = Authenticated(
            HttpMethod.Post, SearchPath, session, JsonContent.Create(new PiSearchRequest(query.Term, query.Page, query.PageSize)));

        (HttpResponseMessage? response, bool unauthorized, Error? transportError) = await SendAsync(request, ct);
        if (response is null)
        {
            return (Result<PagedResult<TrademarkSummary>>.Fail(transportError!), unauthorized);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return (Result<PagedResult<TrademarkSummary>>.Fail(InpiErrors.Unavailable), false);
            }

            PiTrademarkSearchResponse? body = await response.Content.ReadFromJsonAsync<PiTrademarkSearchResponse>(ct);
            List<TrademarkSummary> items = (body?.Results ?? []).Select(PiTrademarkMapper.Map).ToList();
            long total = body?.Total ?? items.Count;
            var page = new PagedResult<TrademarkSummary>(items, query.Page, query.PageSize, total);
            return (Result<PagedResult<TrademarkSummary>>.Ok(page), false);
        }
    }

    private async Task<(Result<TrademarkDetail> Result, bool Unauthorized)> TryGetNoticeAsync(
        DepositNumber depositNumber,
        PiSession session,
        CancellationToken ct)
    {
        using HttpRequestMessage request = Authenticated(HttpMethod.Get, NoticePath + depositNumber.Value, session);

        (HttpResponseMessage? response, bool unauthorized, Error? transportError) = await SendAsync(request, ct);
        if (response is null)
        {
            return (Result<TrademarkDetail>.Fail(transportError!), unauthorized);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (Result<TrademarkDetail>.Fail(TrademarkErrors.NotFound(depositNumber)), false);
            }

            if (!response.IsSuccessStatusCode)
            {
                return (Result<TrademarkDetail>.Fail(InpiErrors.Unavailable), false);
            }

            PiTrademarkNotice? body = await response.Content.ReadFromJsonAsync<PiTrademarkNotice>(ct);
            return body is null
                ? (Result<TrademarkDetail>.Fail(InpiErrors.Unavailable), false)
                : (Result<TrademarkDetail>.Ok(PiTrademarkMapper.MapDetail(body, depositNumber)), false);
        }
    }

    private async Task<(Result<TrademarkImage> Result, bool Unauthorized)> TryGetImageAsync(
        DepositNumber depositNumber,
        PiSession session,
        CancellationToken ct)
    {
        using HttpRequestMessage request = Authenticated(HttpMethod.Get, ImagePath + depositNumber.Value, session);

        (HttpResponseMessage? response, bool unauthorized, Error? transportError) = await SendAsync(request, ct);
        if (response is null)
        {
            return (Result<TrademarkImage>.Fail(transportError!), unauthorized);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (Result<TrademarkImage>.Fail(TrademarkErrors.ImageNotFound), false);
            }

            if (!response.IsSuccessStatusCode)
            {
                return (Result<TrademarkImage>.Fail(InpiErrors.Unavailable), false);
            }

            byte[] content = await response.Content.ReadAsByteArrayAsync(ct);
            string contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            return (Result<TrademarkImage>.Ok(new TrademarkImage(content, contentType)), false);
        }
    }

    private async Task<(HttpResponseMessage? Response, bool Unauthorized, Error? TransportError)> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct)
    {
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

    private static HttpRequestMessage Authenticated(
        HttpMethod method,
        string url,
        PiSession session,
        HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.TryAddWithoutValidation("Cookie", $"access_token={session.AccessToken}");
        request.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", session.XsrfToken);
        return request;
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
