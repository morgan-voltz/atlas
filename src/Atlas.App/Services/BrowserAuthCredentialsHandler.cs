using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.App.Services;

/// <summary>
/// Sur la tête WebAssembly, le cookie HttpOnly de refresh (<c>atlas_refresh</c>) n'est posé (réponse
/// de <c>/auth/login</c> ou <c>/auth/2fa/verify</c>) et renvoyé (<c>/auth/refresh</c>, <c>/auth/logout</c>)
/// par le navigateur que si la requête <c>fetch</c> utilise <c>credentials: "include"</c> (cross-origin
/// WASM→API). Ce handler pose cette option sur les appels <c>/auth/*</c>. Réplique
/// <c>SetBrowserRequestCredentials(Include)</c> du client Blazor sans dépendance Blazor : il écrit
/// l'option de fetch lue par le runtime via la clé <c>WebAssemblyFetchOptions</c>. No-op hors navigateur
/// (têtes natives : le cookie est géré par le <c>CookieContainer</c>) et hors <c>/auth/*</c>
/// (les appels data s'authentifient par le bearer en mémoire, sans cookie).
/// </summary>
internal sealed class BrowserAuthCredentialsHandler : DelegatingHandler
{
    private static readonly HttpRequestOptionsKey<IDictionary<string, object>> FetchOptionsKey = new("WebAssemblyFetchOptions");

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsBrowser()
            && request.RequestUri?.AbsolutePath.Contains("/auth/", StringComparison.Ordinal) == true)
        {
            IncludeCredentials(request);
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static void IncludeCredentials(HttpRequestMessage request)
    {
        if (!request.Options.TryGetValue(FetchOptionsKey, out IDictionary<string, object>? options) || options is null)
        {
            options = new Dictionary<string, object>();
            request.Options.Set(FetchOptionsKey, options);
        }

        // Mode credentials du fetch navigateur : joint/accepte les cookies cross-origin (le CORS de l'API
        // autorise déjà les credentials avec une origine explicite, cf. appsettings.Development.json).
        options["credentials"] = "include";
    }
}
