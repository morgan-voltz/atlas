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
            return Result<IReadOnlyList<CompanySummaryResponse>>.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
        }

        PagedResult<CompanySummaryResponse>? paged = await response.Content
            .ReadFromJsonAsync(AtlasJsonContext.Default.PagedResultCompanySummaryResponse, ct)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<CompanySummaryResponse>>.Ok(
            paged?.Items ?? System.Array.Empty<CompanySummaryResponse>());
    }

    /// <summary>
    /// Récupère la fiche brute d'une entreprise par SIREN (value object validé côté domaine).
    /// Le mapping vers un DTO client arrive avec l'écran Fiche (U4 suite).
    /// </summary>
    public async Task<Result<string>> GetCompanyRawAsync(Siren siren, CancellationToken ct = default)
    {
        using HttpResponseMessage response = await httpClient
            .GetAsync($"companies/{siren.Value}", ct)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return Result<string>.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
        }

        string body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return Result<string>.Ok(body);
    }
}
