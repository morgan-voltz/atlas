using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.App.Services;

/// <summary>
/// Ajoute l'en-tête <c>Authorization: Bearer {access token}</c> à chaque requête sortante, depuis
/// l'<see cref="ITokenStore"/> (token en mémoire). Le refresh token n'est jamais manipulé ici
/// (cookie HttpOnly côté WASM / secure storage natif).
/// </summary>
internal sealed class AuthHeaderHandler(ITokenStore tokenStore) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenStore.GetAccessToken() is { Length: > 0 } token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
