using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.App.Services;

/// <summary>
/// Composition root du client (U4.1) : un <see cref="IServiceProvider"/> minimal (DI + HttpClientFactory
/// standard .NET), sans la pile Uno.Extensions, pour garder la navigation maison. La pile reste un pur
/// consommateur de l'API (ADR-002).
/// </summary>
public static class AppServices
{
    // Adresse de l'API en développement (harness docs/13, profil https). On vise HTTPS car le refresh
    // token est un cookie `Secure` (non transmis sur http). À externaliser en configuration par tête
    // (WASM via origin, natif via appsettings) dans une tranche ultérieure.
    private const string DevApiBaseAddress = "https://localhost:7201/";

    private static IServiceProvider? _provider;

    /// <summary>Fournisseur de services applicatif (construit une fois au démarrage de l'app).</summary>
    public static IServiceProvider Provider =>
        _provider ?? throw new InvalidOperationException("AppServices.Initialize() doit être appelé au démarrage.");

    public static void Initialize()
    {
        var services = new ServiceCollection();

        // CookieContainer partagé : conserve le refresh cookie HttpOnly (atlas_refresh) côté desktop,
        // pour que le client « refresh » puisse rejouer /auth/refresh. Sur WebAssembly, c'est le
        // navigateur qui gère le cookie (handler primaire non personnalisé).
        var cookies = new CookieContainer();

        services.AddSingleton<ITokenStore, InMemoryTokenStore>();
        services.AddTransient<AuthHeaderHandler>();
        services.AddTransient<SessionRefreshHandler>();

        // Client dédié au refresh : partage le CookieContainer, SANS Bearer ni handler de refresh
        // (évite toute récursion).
        IHttpClientBuilder refresh = services.AddHttpClient("refresh", c => c.BaseAddress = new Uri(DevApiBaseAddress));

        // Client principal : refresh (outer) → Bearer (inner) → handler primaire.
        IHttpClientBuilder main = services
            .AddHttpClient<AtlasApiClient>(c => c.BaseAddress = new Uri(DevApiBaseAddress))
            .AddHttpMessageHandler<SessionRefreshHandler>()
            .AddHttpMessageHandler<AuthHeaderHandler>();

        if (!OperatingSystem.IsBrowser())
        {
            refresh.ConfigurePrimaryHttpMessageHandler(() => CreateDesktopHandler(cookies));
            main.ConfigurePrimaryHttpMessageHandler(() => CreateDesktopHandler(cookies));
        }

        _provider = services.BuildServiceProvider();
    }

    // Handler primaire des têtes natives : partage le CookieContainer (refresh cookie Secure).
    // En DEBUG, accepte le certificat auto-signé du harness dev (https://localhost:7201) — JAMAIS en Release.
    private static HttpClientHandler CreateDesktopHandler(CookieContainer cookies)
    {
        var handler = new HttpClientHandler { CookieContainer = cookies, UseCookies = true };
#if DEBUG
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#endif
        return handler;
    }

    /// <summary>Raccourci de résolution.</summary>
    public static T Get<T>() where T : notnull => Provider.GetRequiredService<T>();
}
