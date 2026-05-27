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
/// Lecture entreprise via le RNE (<c>GET /companies/{siren}</c>). Le token Bearer est mis en cache
/// (par compte INPI) jusqu'à peu avant son expiration ; en cas de 401, on ré-authentifie une fois.
/// </summary>
internal sealed class RneCompanyProvider(
    HttpClient httpClient,
    IInpiAuthenticationProvider authenticationProvider,
    IMemoryCache cache) : ICompanyDataProvider
{
    private static readonly TimeSpan TokenExpiryMargin = TimeSpan.FromMinutes(1);

    public async Task<Result<UniteLegale>> GetBySirenAsync(
        Siren siren,
        InpiAccessCredentials credentials,
        CancellationToken ct = default)
    {
        Result<string> token = await GetTokenAsync(credentials, forceRefresh: false, ct);
        if (token.IsFailure)
        {
            return Result<UniteLegale>.Fail(token.Error!);
        }

        (Result<UniteLegale> result, bool unauthorized) = await TryGetAsync(siren, token.Value!, ct);
        if (!unauthorized)
        {
            return result;
        }

        // Token probablement expiré côté RNE : on force une ré-authentification puis on retente une fois.
        Result<string> refreshed = await GetTokenAsync(credentials, forceRefresh: true, ct);
        if (refreshed.IsFailure)
        {
            return Result<UniteLegale>.Fail(refreshed.Error!);
        }

        (result, _) = await TryGetAsync(siren, refreshed.Value!, ct);
        return result;
    }

    private async Task<(Result<UniteLegale> Result, bool Unauthorized)> TryGetAsync(
        Siren siren,
        string token,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"companies/{siren.Value}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            return (Result<UniteLegale>.Fail(InpiErrors.Unavailable), false);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return (Result<UniteLegale>.Fail(InpiErrors.Unavailable), false);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return (Result<UniteLegale>.Fail(InpiErrors.Unavailable), true);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (Result<UniteLegale>.Fail(CompanyErrors.NotFound(siren)), false);
            }

            if (!response.IsSuccessStatusCode)
            {
                return (Result<UniteLegale>.Fail(InpiErrors.Unavailable), false);
            }

            RneCompanyResponse? body = await response.Content.ReadFromJsonAsync<RneCompanyResponse>(ct);
            if (body is null)
            {
                return (Result<UniteLegale>.Fail(InpiErrors.Unavailable), false);
            }

            return (Result<UniteLegale>.Ok(RneCompanyMapper.Map(siren, body)), false);
        }
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
