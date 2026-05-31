using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    // F-015 : couverture brevets via le même backend PI (auth XSRF + cookies identique).
    private const string PatentNoticePath = "services/apidiffusion/api/brevets/notice/";
    private const string PatentSearchPath = "services/apidiffusion/api/brevets/search";

    // Collections par défaut conformes à docs/INPI/APIDiffusionV2.json (TrademarkQuery /
    // PatentQuery, champ `collections`). Marques : FR = françaises, EU = Union européenne (EUIPO),
    // WO = internationales (OMPI). Brevets : FR, EP (Office européen des brevets), WO (PCT/OMPI),
    // CCP (certificats complémentaires de protection). Des collections hors spec font remonter une
    // 500 SolR côté INPI (validé en réel le 2026-05-30 : ["FMARK","CTMARK","TMINT"] → 500).
    private static readonly string[] DefaultTrademarkCollections = ["FR", "EU", "WO"];
    private static readonly string[] DefaultPatentCollections = ["FR", "EP", "WO", "CCP"];

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

    public Task<Result<PatentDetail>> GetPatentByPublicationNumberAsync(
        PublicationNumber publicationNumber,
        InpiAccessCredentials credentials,
        CancellationToken ct = default) =>
        ExecuteAsync(credentials, (session, innerCt) => TryGetPatentAsync(publicationNumber, session, innerCt), ct);

    public Task<Result<PagedResult<PatentSummary>>> SearchPatentsAsync(
        PatentSearchQuery query,
        InpiAccessCredentials credentials,
        CancellationToken ct = default) =>
        ExecuteAsync(credentials, (session, innerCt) => TrySearchPatentsAsync(query, session, innerCt), ct);

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
        // Lot 13 — contrat v2 : position 0-based + size + collections explicite + query SolR.
        int position = Math.Max(0, (query.Page - 1) * query.PageSize);
        var payload = new TrademarkQueryRequest(
            Collections: DefaultTrademarkCollections,
            Query: BuildTrademarkSolrQuery(query.Term),
            Position: position,
            Size: query.PageSize);
        using HttpRequestMessage request = Authenticated(
            HttpMethod.Post, SearchPath, session, JsonContent.Create(payload));

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

            // L'INPI renvoie 204 No Content quand la recherche n'a aucun résultat : c'est un
            // résultat valide (page vide), pas une erreur. Le traiter avant ReadFromJsonAsync,
            // qui lèverait sinon une JsonException sur le corps vide.
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return (Result<PagedResult<TrademarkSummary>>.Ok(
                    new PagedResult<TrademarkSummary>([], query.Page, query.PageSize, 0)), false);
            }

            // Lot 13 — l'INPI peut renvoyer 200 avec un body vide / non-JSON quand le moteur
            // SolR a un soubresaut. On capture la JsonException et on retourne unavailable
            // au lieu de leaker une stack trace dans la réponse API.
            PiTrademarkSearchResponse? body;
            try
            {
                body = await response.Content.ReadFromJsonAsync<PiTrademarkSearchResponse>(ct);
            }
            catch (JsonException)
            {
                return (Result<PagedResult<TrademarkSummary>>.Fail(InpiErrors.Unavailable), false);
            }
            var items = (body?.Results ?? []).Select(PiTrademarkMapper.Map).ToList();
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

    // ── F-016 : recherche brevets multi-critères ────────────────────────────────────

    private async Task<(Result<PagedResult<PatentSummary>> Result, bool Unauthorized)> TrySearchPatentsAsync(
        PatentSearchQuery query,
        PiSession session,
        CancellationToken ct)
    {
        // Lot 13 — contrat v2 : position 0-based + size + collections explicite + query SolR
        // multi-critères (TIT / DEPOSANT / INV) joints par AND.
        int position = Math.Max(0, (query.Page - 1) * query.PageSize);
        var payload = new PatentQueryRequest(
            Collections: DefaultPatentCollections,
            Query: BuildPatentSolrQuery(query.Title, query.Applicant, query.Inventor),
            Position: position,
            Size: query.PageSize);
        using HttpRequestMessage request = Authenticated(
            HttpMethod.Post, PatentSearchPath, session, JsonContent.Create(payload));

        (HttpResponseMessage? response, bool unauthorized, Error? transportError) = await SendAsync(request, ct);
        if (response is null)
        {
            return (Result<PagedResult<PatentSummary>>.Fail(transportError!), unauthorized);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return (Result<PagedResult<PatentSummary>>.Fail(InpiErrors.Unavailable), false);
            }

            // 204 No Content = recherche sans résultat (cf. trademark search) : page vide, pas 502.
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return (Result<PagedResult<PatentSummary>>.Ok(
                    new PagedResult<PatentSummary>([], query.Page, query.PageSize, 0)), false);
            }

            // Lot 13 — robustesse face à body vide / non-JSON (cf. trademark search).
            PiPatentSearchResponse? body;
            try
            {
                body = await response.Content.ReadFromJsonAsync<PiPatentSearchResponse>(ct);
            }
            catch (JsonException)
            {
                return (Result<PagedResult<PatentSummary>>.Fail(InpiErrors.Unavailable), false);
            }
            var items = (body?.Results ?? []).Select(PiPatentMapper.MapSummary).ToList();
            long total = body?.Total ?? items.Count;
            var page = new PagedResult<PatentSummary>(items, query.Page, query.PageSize, total);
            return (Result<PagedResult<PatentSummary>>.Ok(page), false);
        }
    }

    // ── F-015 : brevet par numéro ───────────────────────────────────────────────────

    private async Task<(Result<PatentDetail> Result, bool Unauthorized)> TryGetPatentAsync(
        PublicationNumber publicationNumber,
        PiSession session,
        CancellationToken ct)
    {
        using HttpRequestMessage request = Authenticated(
            HttpMethod.Get, PatentNoticePath + Uri.EscapeDataString(publicationNumber.Value), session);

        (HttpResponseMessage? response, bool unauthorized, Error? transportError) = await SendAsync(request, ct);
        if (response is null)
        {
            return (Result<PatentDetail>.Fail(transportError!), unauthorized);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (Result<PatentDetail>.Fail(PatentErrors.NotFound(publicationNumber)), false);
            }

            if (!response.IsSuccessStatusCode)
            {
                return (Result<PatentDetail>.Fail(InpiErrors.Unavailable), false);
            }

            PiPatentNotice? body = await response.Content.ReadFromJsonAsync<PiPatentNotice>(ct);
            return body is null
                ? (Result<PatentDetail>.Fail(InpiErrors.Unavailable), false)
                : (Result<PatentDetail>.Ok(PiPatentMapper.Map(body, publicationNumber)), false);
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

        // Lot 11 — durcissement INPI : l'auth PI exige désormais un primer CSRF.
        // (1) POST auth/login sans body avec header `X-CSRF-TOKEN: Fetch` → réponse 403
        //     attendue avec un cookie `XSRF-TOKEN=<guid>` à réutiliser.
        // (2) Vrai POST auth/login avec body JSON + header `X-XSRF-TOKEN: <guid>` + le
        //     cookie XSRF-TOKEN renvoyé en `Cookie:` (le serveur compare les deux,
        //     pattern double-submit cookie de Spring Security).
        // Cf. erreur INPI : « Invalid CSRF Token 'null' was found on the request
        // parameter '_csrf' or header 'X-XSRF-TOKEN'. ».
        Result<string> primerResult = await FetchCsrfTokenAsync(ct);
        if (primerResult.IsFailure)
        {
            return Result<PiSession>.Fail(primerResult.Error!);
        }
        string initialXsrf = primerResult.Value!;

        using var request = new HttpRequestMessage(HttpMethod.Post, "auth/login")
        {
            Content = JsonContent.Create(new PiLoginRequest(credentials.Username, credentials.Password)),
        };
        request.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", initialXsrf);
        request.Headers.TryAddWithoutValidation("Cookie", $"XSRF-TOKEN={initialXsrf}");

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

            // La doc technique API PI (§3.8, exemples §4.x/§5.x) exige que TOUS les appels
            // `apidiffusion` portent aussi le cookie `session_token=<refresh_token>` en plus de
            // `access_token`. Le login pose `refresh_token` en cookie ; on le capture ici.
            string refreshToken = ExtractCookie(setCookies, "refresh_token") ?? string.Empty;

            // Le serveur renvoie un nouveau XSRF-TOKEN post-login (rotation) ; on prend ça
            // pour les requêtes suivantes, sinon on retombe sur le token primer.
            string xsrf = ExtractCookie(setCookies, "XSRF-TOKEN") ?? initialXsrf;
            var session = new PiSession(accessToken, refreshToken, xsrf, DateTimeOffset.UtcNow.Add(options.Value.TokenLifetimeFallback));

            TimeSpan ttl = session.ExpiresAt - DateTimeOffset.UtcNow - SessionMargin;
            if (ttl > TimeSpan.Zero)
            {
                cache.Set(cacheKey, session, ttl);
            }

            return Result<PiSession>.Ok(session);
        }
    }

    /// <summary>
    /// Lot 11 — Primer CSRF de l'auth PI. <c>POST auth/login</c> avec <c>X-CSRF-TOKEN: Fetch</c>
    /// et sans body : le serveur renvoie 403 (attendu) en posant un cookie <c>XSRF-TOKEN=&lt;guid&gt;</c>
    /// que l'on extrait et réutilise pour la vraie requête de login. Sans ce primer, le serveur
    /// rejette systématiquement avec « Invalid CSRF Token 'null' ».
    /// </summary>
    private async Task<Result<string>> FetchCsrfTokenAsync(CancellationToken ct)
    {
        using var primer = new HttpRequestMessage(HttpMethod.Post, "auth/login");
        primer.Headers.TryAddWithoutValidation("X-CSRF-TOKEN", "Fetch");

        HttpResponseMessage primerResponse;
        try
        {
            primerResponse = await httpClient.SendAsync(primer, ct);
        }
        catch (HttpRequestException)
        {
            return Result<string>.Fail(InpiErrors.Unavailable);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return Result<string>.Fail(InpiErrors.Unavailable);
        }

        using (primerResponse)
        {
            // Le primer répond 403 par design (pas de credentials, juste pose le cookie).
            // On tolère aussi 200 / 401 — seule l'absence du cookie XSRF est bloquante.
            List<string> setCookies = primerResponse.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values)
                ? values.ToList()
                : [];

            string? xsrf = ExtractCookie(setCookies, "XSRF-TOKEN");
            return xsrf is null
                ? Result<string>.Fail(InpiErrors.Unavailable)
                : Result<string>.Ok(xsrf);
        }
    }

    private static HttpRequestMessage Authenticated(
        HttpMethod method,
        string url,
        PiSession session,
        HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        // Doc technique API PI (§3.8, exemples §4.x/§5.x) : les appels `apidiffusion` portent
        // TROIS cookies — `access_token`, `session_token=<refresh_token>` ET `XSRF-TOKEN` —
        // plus le header `X-XSRF-TOKEN` (double-submit Spring Security). L'absence de
        // `session_token` faisait échouer `search` (405/erreur de session) malgré une auth OK.
        string cookie = $"access_token={session.AccessToken}; XSRF-TOKEN={session.XsrfToken}";
        if (!string.IsNullOrEmpty(session.RefreshToken))
        {
            cookie += $"; session_token={session.RefreshToken}";
        }
        request.Headers.TryAddWithoutValidation("Cookie", cookie);
        request.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", session.XsrfToken);
        // Doc §4.4.5 / §5.4.5 : `x-forwarded-for` est « indispensable pour la gestion des quotas
        // utilisateurs ». On l'envoie systématiquement sur les appels de diffusion.
        request.Headers.TryAddWithoutValidation("x-forwarded-for", "127.0.0.1");
        // Réponse par défaut en XML ; on impose Accept JSON pour rester homogène côté mapping.
        request.Headers.TryAddWithoutValidation("Accept", "application/json");
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

    private sealed record PiSession(string AccessToken, string RefreshToken, string XsrfToken, DateTimeOffset ExpiresAt);

    private sealed record PiLoginRequest(
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("password")] string Password);

    /// <summary>
    /// Lot 13 — Conforme à <c>TrademarkQuery</c> de la spec INPI v2 (cf.
    /// <c>docs/INPI/APIDiffusionV2.json</c>) : pagination par <c>position</c> / <c>size</c>,
    /// <c>collections</c> obligatoire (sans quoi la 500 « SolR no body » remonte), <c>query</c>
    /// au format SolR INPI (<c>[Mark=...]</c>).
    /// </summary>
    private sealed record TrademarkQueryRequest(
        [property: JsonPropertyName("collections")] string[] Collections,
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("position")] int Position,
        [property: JsonPropertyName("size")] int Size);

    /// <summary>
    /// Lot 13 — Conforme à <c>PatentQuery</c> de la spec INPI v2. Identique en shape à
    /// <see cref="TrademarkQueryRequest"/> ; les champs SolR (<c>TIT</c>, <c>DEPOSANT</c>,
    /// <c>INV</c>) et les collections (<c>FR</c> / <c>EP</c> / <c>WO</c> / <c>CCP</c>) sont
    /// différents.
    /// </summary>
    private sealed record PatentQueryRequest(
        [property: JsonPropertyName("collections")] string[] Collections,
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("position")] int Position,
        [property: JsonPropertyName("size")] int Size);

    /// <summary>
    /// Conversion d'un terme libre (saisi par l'utilisateur) en clause SolR INPI pour la
    /// recherche marques sur le champ <c>Mark</c>. Échappe les caractères SolR spéciaux
    /// (<c>[ ] : \</c>) qui sinon casseraient le parseur (cf. spec § « 500 SolR corrompue »).
    /// </summary>
    private static string BuildTrademarkSolrQuery(string term) =>
        $"[Mark={EscapeSolrValue(term)}]";

    /// <summary>
    /// Conversion d'une requête multi-critères brevet en SolR INPI avec opérateur AND.
    /// Tous critères vides → fallback <c>[TIT=*]</c> (liste exhaustive paginée), pour ne
    /// pas envoyer une requête vide qui serait 500.
    /// </summary>
    private static string BuildPatentSolrQuery(string? title, string? applicant, string? inventor)
    {
        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(title))
        {
            parts.Add($"[TIT={EscapeSolrValue(title)}]");
        }
        if (!string.IsNullOrWhiteSpace(applicant))
        {
            parts.Add($"[DEPOSANT={EscapeSolrValue(applicant)}]");
        }
        if (!string.IsNullOrWhiteSpace(inventor))
        {
            parts.Add($"[INV={EscapeSolrValue(inventor)}]");
        }
        return parts.Count == 0 ? "[TIT=*]" : string.Join(" AND ", parts);
    }

    /// <summary>
    /// Échappement minimal des caractères réservés SolR INPI. La syntaxe INPI utilise des
    /// crochets <c>[CHAMP=valeur]</c>, donc on doit au moins échapper <c>[</c>, <c>]</c>,
    /// <c>:</c> et <c>\</c> pour que la valeur reste interne aux crochets.
    /// </summary>
    private static string EscapeSolrValue(string raw) => raw
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("[", "\\[", StringComparison.Ordinal)
        .Replace("]", "\\]", StringComparison.Ordinal)
        .Replace(":", "\\:", StringComparison.Ordinal);
}
