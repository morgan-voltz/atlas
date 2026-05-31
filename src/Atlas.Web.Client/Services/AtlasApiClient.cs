using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Atlas.Shared.Result;
using Atlas.Web.Client.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Atlas.Web.Client.Services;

/// <summary>
/// Implémentation HttpClient typé. Miroir du client MAUI, à un détail WASM près : en navigateur il n'y a
/// pas de <c>CookieContainer</c> — le cookie HttpOnly <c>atlas_refresh</c> n'est joint que si la requête
/// active <see cref="BrowserRequestCredentials.Include"/> (et que le CORS de l'API autorise les credentials).
/// On l'active donc sur les appels <c>/auth/*</c>. Les appels data s'authentifient par le bearer en mémoire.
/// </summary>
internal sealed class AtlasApiClient(HttpClient httpClient, ITokenStore tokenStore) : IAtlasApiClient
{
    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "auth/login")
            {
                Content = JsonContent.Create(new LoginRequest(email, password)),
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            response = await httpClient.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            return new LoginResult(LoginStatus.Unavailable);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return new LoginResult(MapLoginError(await ReadProblemCodeAsync(response, ct)));
            }

            LoginResponse? body = await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
            if (body is null)
            {
                return new LoginResult(LoginStatus.Unavailable);
            }

            if (body.TwoFactorRequired == true)
            {
                return new LoginResult(LoginStatus.TwoFactorRequired, body.ChallengeToken);
            }

            if (string.IsNullOrEmpty(body.AccessToken))
            {
                return new LoginResult(LoginStatus.Unavailable);
            }

            tokenStore.SetAccessToken(body.AccessToken);
            return new LoginResult(LoginStatus.Success);
        }
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            using HttpResponseMessage response = await httpClient.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            // Déconnexion locale même si l'appel réseau échoue.
        }
        finally
        {
            tokenStore.Clear();
        }
    }

    public Task<ApiResult<PagedResult<CompanySummaryResponse>>> SearchCompaniesAsync(
        string name,
        int page,
        int pageSize,
        CancellationToken ct = default) =>
        GetAsync<PagedResult<CompanySummaryResponse>>(
            $"companies?name={Uri.EscapeDataString(name)}&page={page}&pageSize={pageSize}", ct);

    public Task<ApiResult<CompanyResponse>> GetCompanyBySirenAsync(string siren, CancellationToken ct = default) =>
        GetAsync<CompanyResponse>($"companies/{Uri.EscapeDataString(siren)}", ct);

    private async Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken ct)
    {
        try
        {
            using HttpResponseMessage response =
                await SendWithAuthAsync(() => new HttpRequestMessage(HttpMethod.Get, url), ct);

            if (response.IsSuccessStatusCode)
            {
                T? value = await response.Content.ReadFromJsonAsync<T>(ct);
                return value is null
                    ? ApiResult<T>.Fail("empty_response")
                    : ApiResult<T>.Ok(value);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return ApiResult<T>.Fail("unauthorized");
            }

            return ApiResult<T>.Fail(await ReadProblemCodeAsync(response, ct) ?? "unexpected");
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Fail("network");
        }
    }

    private async Task<HttpResponseMessage> SendWithAuthAsync(Func<HttpRequestMessage> requestFactory, CancellationToken ct)
    {
        HttpResponseMessage response = await SendOnceAsync(requestFactory(), ct);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        response.Dispose();

        // Le refresh token est porté par le cookie HttpOnly, joint grâce à credentials=include.
        await TryRefreshAsync(ct);
        return await SendOnceAsync(requestFactory(), ct);
    }

    private async Task<HttpResponseMessage> SendOnceAsync(HttpRequestMessage request, CancellationToken ct)
    {
        string? token = tokenStore.GetAccessToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await httpClient.SendAsync(request, ct);
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "auth/refresh");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            using HttpResponseMessage response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                tokenStore.Clear();
                return false;
            }

            LoginResponse? body = await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
            if (body is null || string.IsNullOrEmpty(body.AccessToken))
            {
                tokenStore.Clear();
                return false;
            }

            tokenStore.SetAccessToken(body.AccessToken);
            return true;
        }
        catch (HttpRequestException)
        {
            tokenStore.Clear();
            return false;
        }
    }

    private static async Task<string?> ReadProblemCodeAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            ApiProblem? problem = await response.Content.ReadFromJsonAsync<ApiProblem>(ct);
            return problem?.Code;
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private static LoginStatus MapLoginError(string? code) => code switch
    {
        "users.invalid_credentials" => LoginStatus.InvalidCredentials,
        "users.email_not_verified" => LoginStatus.EmailNotVerified,
        "users.account_locked" => LoginStatus.AccountLocked,
        _ => LoginStatus.Unavailable,
    };
}
