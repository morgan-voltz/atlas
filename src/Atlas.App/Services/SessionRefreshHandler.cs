using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Atlas.App.Models;

namespace Atlas.App.Services;

/// <summary>
/// Garde de session (ADR-010) : sur un <c>401</c>, tente un refresh silencieux (<c>POST /auth/refresh</c>,
/// qui s'appuie sur le cookie HttpOnly <c>atlas_refresh</c> / secure storage), range le nouvel access token
/// en mémoire, puis rejoue la requête une fois. Le refresh passe par un client dédié (« refresh ») sans
/// ce handler ni le Bearer, pour éviter toute récursion.
/// </summary>
internal sealed class SessionRefreshHandler(IHttpClientFactory httpClientFactory, ITokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // On ne tente le refresh que sur 401, jamais sur les endpoints d'auth eux-mêmes.
        if (response.StatusCode != HttpStatusCode.Unauthorized
            || request.RequestUri?.AbsolutePath.Contains("/auth/", StringComparison.Ordinal) == true)
        {
            return response;
        }

        if (!await TryRefreshAsync(cancellationToken).ConfigureAwait(false))
        {
            return response;
        }

        // Rejoue une fois — seulement si la requête n'a pas de corps déjà consommé (GET/DELETE).
        if (request.Content is not null)
        {
            return response;
        }

        response.Dispose();
        return await base.SendAsync(Clone(request), cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        try
        {
            HttpClient refreshClient = httpClientFactory.CreateClient("refresh");
            using HttpResponseMessage refresh = await refreshClient
                .PostAsync("auth/refresh", content: null, ct)
                .ConfigureAwait(false);

            if (!refresh.IsSuccessStatusCode)
            {
                tokenStore.Clear();
                return false;
            }

            AccessTokenResponse? token = await refresh.Content
                .ReadFromJsonAsync(AtlasJsonContext.Default.AccessTokenResponse, ct)
                .ConfigureAwait(false);

            if (token is null || string.IsNullOrEmpty(token.AccessToken))
            {
                return false;
            }

            tokenStore.SetAccessToken(token.AccessToken);
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    private static HttpRequestMessage Clone(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
