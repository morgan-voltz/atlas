using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#if __WASM__
using Uno.WebAssembly.Net.Http;
#endif

namespace Atlas.App.Services;

/// <summary>
/// Sur la tête WebAssembly, le cookie HttpOnly de refresh (<c>atlas_refresh</c>) n'est posé (réponse
/// de <c>/auth/login</c> ou <c>/auth/2fa/verify</c>) et renvoyé (<c>/auth/refresh</c>, <c>/auth/logout</c>)
/// par le navigateur que si la requête <c>fetch</c> utilise <c>credentials: "include"</c> (cross-origin
/// WASM→API). Le BCL ne permet pas de régler les options de <c>fetch</c> hors Blazor ; Uno l'expose via
/// <c>SetBrowserRequestOption</c> (package <c>Uno.Wasm.HttpRequestMessageExtensions</c>, namespace
/// <c>Uno.WebAssembly.Net.Http</c>). Ce handler pose l'option sur les appels <c>/auth/*</c>. No-op hors
/// navigateur (têtes natives : cookie géré par le <c>CookieContainer</c>) et hors <c>/auth/*</c>
/// (les appels data s'authentifient par le bearer en mémoire, sans cookie).
/// </summary>
internal sealed class BrowserAuthCredentialsHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
#if __WASM__
        if (request.RequestUri?.AbsolutePath.Contains("/auth/", System.StringComparison.Ordinal) == true)
        {
            // Mode credentials du fetch navigateur : joint/accepte les cookies cross-origin (le CORS de
            // l'API autorise déjà les credentials avec une origine explicite, cf. appsettings.Development.json).
            request.SetBrowserRequestOption("credentials", "include");
        }
#endif
        return base.SendAsync(request, cancellationToken);
    }
}
