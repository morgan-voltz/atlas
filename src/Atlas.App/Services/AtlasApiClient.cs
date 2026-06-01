using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Atlas.App.Models;
using Atlas.Domain.Companies;
using Atlas.Shared.Result;

namespace Atlas.App.Services;

/// <summary>État d'une tentative de connexion (doc 12 §10 ; ADR-010).</summary>
public enum LoginStatus
{
    /// <summary>Connecté : l'access token est en mémoire (le refresh vit dans le cookie/secure storage).</summary>
    Authenticated,

    /// <summary>2FA requise : un défi TOTP doit être résolu (challenge token hors URL).</summary>
    TwoFactorRequired,
}

/// <summary>Résultat d'une connexion réussie ou en attente de 2FA.</summary>
public sealed record LoginResult(LoginStatus Status, string? ChallengeToken);

/// <summary>
/// Unique point d'entrée du client Uno vers le backend (ADR-002 : « client pur de l'API »). Ne
/// référence que <c>Atlas.Domain</c> + <c>Atlas.Shared</c> (verrouillé par Atlas.Architecture.Tests).
/// L'access token est posé sur chaque requête par <see cref="AuthHeaderHandler"/>.
/// </summary>
public sealed class AtlasApiClient(HttpClient httpClient, ITokenStore tokenStore)
{
    /// <summary>
    /// Connecte l'utilisateur (<c>POST /auth/login</c>). En cas de succès, range l'access token en
    /// mémoire ; le refresh token rotatif est posé en cookie HttpOnly par l'API (jamais vu ici).
    /// </summary>
    public async Task<Result<LoginResult>> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient
                .PostAsJsonAsync("auth/login", new LoginRequest(email, password), AtlasJsonContext.Default.LoginRequest, ct)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return Result<LoginResult>.Fail(ApiErrors.Unreachable());
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
        {
            return Result<LoginResult>.Fail(ApiErrors.InvalidCredentials());
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result<LoginResult>.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
        }

        using JsonDocument doc = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false));
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("twoFactorRequired", out JsonElement twoFa) && twoFa.GetBoolean())
        {
            string? challenge = root.TryGetProperty("challengeToken", out JsonElement c) ? c.GetString() : null;
            return Result<LoginResult>.Ok(new LoginResult(LoginStatus.TwoFactorRequired, challenge));
        }

        if (root.TryGetProperty("accessToken", out JsonElement at) && at.GetString() is { Length: > 0 } accessToken)
        {
            tokenStore.SetAccessToken(accessToken);
            return Result<LoginResult>.Ok(new LoginResult(LoginStatus.Authenticated, null));
        }

        return Result<LoginResult>.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
    }

    /// <summary>
    /// Valide le défi 2FA (<c>POST /auth/2fa/verify</c>) : le challenge token (issu du login, gardé en
    /// mémoire) + le code TOTP/secours. En cas de succès, range l'access token (refresh en cookie).
    /// </summary>
    public async Task<Result> VerifyTwoFactorAsync(string challengeToken, string code, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient
                .PostAsJsonAsync("auth/2fa/verify", new VerifyTwoFactorRequest(challengeToken, code), AtlasJsonContext.Default.VerifyTwoFactorRequest, ct)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(ApiErrors.Unreachable());
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail(ApiErrors.InvalidTwoFactorCode());
        }

        AccessTokenResponse? token = await response.Content
            .ReadFromJsonAsync(AtlasJsonContext.Default.AccessTokenResponse, ct)
            .ConfigureAwait(false);

        if (token is null || string.IsNullOrEmpty(token.AccessToken))
        {
            return Result.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
        }

        tokenStore.SetAccessToken(token.AccessToken);
        return Result.Ok();
    }

    /// <summary>Termine la session locale (l'access token en mémoire). Le serveur révoque le refresh via /auth/logout.</summary>
    public void ClearSession() => tokenStore.Clear();

    /// <summary>
    /// Recherche d'entreprises par dénomination (<c>GET /companies?name=</c>, requiert l'auth — le Bearer
    /// est posé par <see cref="AuthHeaderHandler"/>). Retourne la page d'items (pagination affinée plus tard).
    /// </summary>
    public async Task<Result<IReadOnlyList<CompanySummaryResponse>>> SearchCompaniesAsync(
        string name, CancellationToken ct = default)
    {
        string url = $"companies?name={Uri.EscapeDataString(name)}&page=1&pageSize=20";

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(url, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return Result<IReadOnlyList<CompanySummaryResponse>>.Fail(ApiErrors.Unreachable());
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return Result<IReadOnlyList<CompanySummaryResponse>>.Fail(ApiErrors.SessionExpired());
        }

        if (!response.IsSuccessStatusCode)
        {
            // L'API renvoie un ProblemDetails avec un `code` métier ; on surface l'état dégradé
            // honnête « connectez INPI » (doc 12 §10) plutôt qu'un « HTTP 409 » opaque.
            string problem = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            ApiError error = problem.Contains("inpi.not_connected", StringComparison.Ordinal)
                ? ApiErrors.InpiNotConnected()
                : ApiErrors.RequestFailed((int)response.StatusCode);
            return Result<IReadOnlyList<CompanySummaryResponse>>.Fail(error);
        }

        PagedResult<CompanySummaryResponse>? paged = await response.Content
            .ReadFromJsonAsync(AtlasJsonContext.Default.PagedResultCompanySummaryResponse, ct)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<CompanySummaryResponse>>.Ok(
            paged?.Items ?? System.Array.Empty<CompanySummaryResponse>());
    }

    /// <summary>Fil Accueil : mouvements des entités suivies (<c>GET /feed/timeline?mentionsFavoritesOnly=true</c>, F-044).</summary>
    public Task<Result<IReadOnlyList<TimelineItemResponse>>> GetAccueilFeedAsync(CancellationToken ct = default) =>
        GetTimelineAsync("feed/timeline?mentionsFavoritesOnly=true&page=1&pageSize=30", ct);

    /// <summary>Fil Veille : flux éditorial des sources (<c>GET /feed/timeline?editorialOnly=true</c>, F-047).</summary>
    public Task<Result<IReadOnlyList<TimelineItemResponse>>> GetVeilleFeedAsync(CancellationToken ct = default) =>
        GetTimelineAsync("feed/timeline?editorialOnly=true&page=1&pageSize=30", ct);

    private async Task<Result<IReadOnlyList<TimelineItemResponse>>> GetTimelineAsync(string url, CancellationToken ct)
    {
        try
        {
            PagedResult<TimelineItemResponse>? paged = await httpClient
                .GetFromJsonAsync(url, AtlasJsonContext.Default.PagedResultTimelineItemResponse, ct)
                .ConfigureAwait(false);
            return Result<IReadOnlyList<TimelineItemResponse>>.Ok(paged?.Items ?? Array.Empty<TimelineItemResponse>());
        }
        catch (HttpRequestException)
        {
            return Result<IReadOnlyList<TimelineItemResponse>>.Fail(ApiErrors.Unreachable());
        }
    }

    /// <summary>Marque un item de veille comme lu (<c>PATCH /feed/items/{id}/state</c>).</summary>
    public async Task<Result> MarkFeedItemReadAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient
                .PatchAsJsonAsync($"feed/items/{id}/state", new SetFeedItemStateRequest(true, null, null), AtlasJsonContext.Default.SetFeedItemStateRequest, ct)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode ? Result.Ok() : Result.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
        }
        catch (HttpRequestException)
        {
            return Result.Fail(ApiErrors.Unreachable());
        }
    }

    /// <summary>Entreprises suivies (<c>GET /favorites/companies</c>, F-017). Base locale — pas d'INPI requis.</summary>
    public async Task<Result<IReadOnlyList<CompanyFavoriteResponse>>> GetFavoriteCompaniesAsync(CancellationToken ct = default)
    {
        try
        {
            IReadOnlyList<CompanyFavoriteResponse>? favorites = await httpClient
                .GetFromJsonAsync("favorites/companies", AtlasJsonContext.Default.IReadOnlyListCompanyFavoriteResponse, ct)
                .ConfigureAwait(false);
            return Result<IReadOnlyList<CompanyFavoriteResponse>>.Ok(favorites ?? Array.Empty<CompanyFavoriteResponse>());
        }
        catch (HttpRequestException)
        {
            return Result<IReadOnlyList<CompanyFavoriteResponse>>.Fail(ApiErrors.Unreachable());
        }
    }

    /// <summary>Suit une entreprise (<c>POST /favorites/companies</c>).</summary>
    public async Task<Result> AddFavoriteCompanyAsync(string siren, string? name, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient
                .PostAsJsonAsync("favorites/companies", new AddCompanyFavoriteRequest(siren, name), AtlasJsonContext.Default.AddCompanyFavoriteRequest, ct)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode ? Result.Ok() : Result.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
        }
        catch (HttpRequestException)
        {
            return Result.Fail(ApiErrors.Unreachable());
        }
    }

    /// <summary>Ne suit plus une entreprise (<c>DELETE /favorites/companies/{siren}</c>).</summary>
    public async Task<Result> RemoveFavoriteCompanyAsync(string siren, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient
                .DeleteAsync($"favorites/companies/{Uri.EscapeDataString(siren)}", ct)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode ? Result.Ok() : Result.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
        }
        catch (HttpRequestException)
        {
            return Result.Fail(ApiErrors.Unreachable());
        }
    }

    /// <summary>Statut de la connexion INPI (<c>GET /inpi/connection</c>, F-003) — sans aucun secret.</summary>
    public async Task<Result<InpiConnectionStatusResponse>> GetInpiStatusAsync(CancellationToken ct = default)
    {
        try
        {
            InpiConnectionStatusResponse? status = await httpClient
                .GetFromJsonAsync("inpi/connection", AtlasJsonContext.Default.InpiConnectionStatusResponse, ct)
                .ConfigureAwait(false);
            return status is null
                ? Result<InpiConnectionStatusResponse>.Fail(ApiErrors.RequestFailed(204))
                : Result<InpiConnectionStatusResponse>.Ok(status);
        }
        catch (HttpRequestException)
        {
            return Result<InpiConnectionStatusResponse>.Fail(ApiErrors.Unreachable());
        }
    }

    /// <summary>
    /// Connecte un compte INPI (<c>POST /inpi/connection</c>, F-003). Les identifiants ne sont ni
    /// persistés ni journalisés côté client ; l'API les chiffre au repos (CLAUDE.md / ADR-004).
    /// </summary>
    public async Task<Result> ConnectInpiAsync(string username, string password, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient
                .PostAsJsonAsync("inpi/connection", new ConnectInpiRequest(username, password), AtlasJsonContext.Default.ConnectInpiRequest, ct)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(ApiErrors.Unreachable());
        }

        if (response.IsSuccessStatusCode)
        {
            return Result.Ok();
        }

        string problem = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (problem.Contains("inpi.api_access_not_allowed", StringComparison.Ordinal))
        {
            return Result.Fail(ApiErrors.InpiAccessNotAllowed());
        }

        return Result.Fail(ApiErrors.InpiConnectionFailed());
    }

    /// <summary>Déconnecte le compte INPI (<c>DELETE /inpi/connection</c>).</summary>
    public async Task<Result> DisconnectInpiAsync(CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient
                .DeleteAsync("inpi/connection", ct)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode
                ? Result.Ok()
                : Result.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
        }
        catch (HttpRequestException)
        {
            return Result.Fail(ApiErrors.Unreachable());
        }
    }

    /// <summary>
    /// Fiche d'une entreprise par SIREN (<c>GET /companies/{siren}</c>, F-004). Le <see cref="Siren"/>
    /// (value object validé côté domaine) est la seule entrée acceptée — pas de <c>string</c> nu.
    /// </summary>
    public async Task<Result<CompanyResponse>> GetCompanyAsync(Siren siren, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"companies/{siren.Value}", ct).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return Result<CompanyResponse>.Fail(ApiErrors.Unreachable());
        }

        if (response.IsSuccessStatusCode)
        {
            CompanyResponse? company = await response.Content
                .ReadFromJsonAsync(AtlasJsonContext.Default.CompanyResponse, ct)
                .ConfigureAwait(false);
            return company is null
                ? Result<CompanyResponse>.Fail(ApiErrors.RequestFailed((int)response.StatusCode))
                : Result<CompanyResponse>.Ok(company);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return Result<CompanyResponse>.Fail(ApiErrors.SessionExpired());
        }

        // ProblemDetails métier : on surface l'état honnête adéquat (doc 12 §10/§14).
        string problem = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        ApiError error =
            problem.Contains("inpi.not_connected", StringComparison.Ordinal) ? ApiErrors.InpiNotConnected() :
            problem.Contains("companies.not_found", StringComparison.Ordinal) ? ApiErrors.CompanyNotFound(siren.Value) :
            ApiErrors.RequestFailed((int)response.StatusCode);
        return Result<CompanyResponse>.Fail(error);
    }
}
