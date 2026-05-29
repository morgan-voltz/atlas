using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Atlas.Maui.Models;
using Atlas.Shared.Result;

namespace Atlas.Maui.Services;

internal sealed class AtlasApiClient(HttpClient httpClient, ITokenStore tokenStore) : IAtlasApiClient
{
    public async Task<bool> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        using HttpResponseMessage response =
            await httpClient.PostAsJsonAsync("auth/login", new { email, password }, ct);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        AccessTokenResponse? tokens = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(ct);
        if (tokens is null)
        {
            return false;
        }

        await tokenStore.SetAccessTokenAsync(tokens.AccessToken);
        return true;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsync("auth/logout", content: null, ct);
        }
        catch (HttpRequestException)
        {
            // Déconnexion locale même si l'appel réseau échoue.
        }

        tokenStore.Clear();
    }

    public async Task<bool> IsAuthenticatedAsync() =>
        !string.IsNullOrEmpty(await tokenStore.GetAccessTokenAsync());

    public Task<CompanyResponse?> GetCompanyBySirenAsync(string siren, CancellationToken ct = default) =>
        GetAsync<CompanyResponse>($"companies/{Uri.EscapeDataString(siren)}", ct);

    public Task<PagedResult<CompanySummaryResponse>?> SearchCompaniesAsync(
        string name,
        int page,
        int pageSize,
        CancellationToken ct = default) =>
        GetAsync<PagedResult<CompanySummaryResponse>>(
            $"companies?name={Uri.EscapeDataString(name)}&page={page}&pageSize={pageSize}", ct);

    public async Task<IReadOnlyList<SearchHistoryEntryResponse>> GetSearchHistoryAsync(CancellationToken ct = default) =>
        await GetAsync<List<SearchHistoryEntryResponse>>("search-history", ct) ?? [];

    public Task<AccessibilityPreferencesResponse?> GetAccessibilityPreferencesAsync(CancellationToken ct = default) =>
        GetAsync<AccessibilityPreferencesResponse>("user/preferences/accessibility", ct);

    public async Task<bool> UpdateAccessibilityPreferencesAsync(
        AccessibilityPreferencesResponse preferences,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        using HttpResponseMessage response = await SendWithAuthAsync(
            () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Put, "user/preferences/accessibility")
                {
                    Content = JsonContent.Create(preferences),
                };
                return request;
            },
            ct);

        return response.IsSuccessStatusCode;
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        using HttpResponseMessage response =
            await SendWithAuthAsync(() => new HttpRequestMessage(HttpMethod.Get, url), ct);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<T>(ct)
            : default;
    }

    private async Task<HttpResponseMessage> SendWithAuthAsync(Func<HttpRequestMessage> requestFactory, CancellationToken ct)
    {
        HttpResponseMessage response = await SendOnceAsync(requestFactory(), ct);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        response.Dispose();

        // Le refresh token est porté par le cookie httpOnly (conservé par le CookieContainer du handler).
        if (await TryRefreshAsync(ct))
        {
            return await SendOnceAsync(requestFactory(), ct);
        }

        return await SendOnceAsync(requestFactory(), ct);
    }

    private async Task<HttpResponseMessage> SendOnceAsync(HttpRequestMessage request, CancellationToken ct)
    {
        string? token = await tokenStore.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await httpClient.SendAsync(request, ct);
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        using HttpResponseMessage response = await httpClient.PostAsync("auth/refresh", content: null, ct);
        if (!response.IsSuccessStatusCode)
        {
            tokenStore.Clear();
            return false;
        }

        AccessTokenResponse? tokens = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(ct);
        if (tokens is null)
        {
            return false;
        }

        await tokenStore.SetAccessTokenAsync(tokens.AccessToken);
        return true;
    }
}
