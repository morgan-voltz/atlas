using System;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.App.Services;

/// <summary>
/// Composition root du client (U4.1) : un <see cref="IServiceProvider"/> minimal (DI + HttpClientFactory
/// standard .NET), sans la pile Uno.Extensions, pour garder la navigation maison. La pile reste un pur
/// consommateur de l'API (ADR-002).
/// </summary>
public static class AppServices
{
    // Adresse de l'API en développement (harness docs/13 : API sur :5023). À externaliser en
    // configuration par tête (WASM via origin, natif via appsettings) dans une tranche ultérieure.
    private const string DevApiBaseAddress = "http://localhost:5023/";

    private static IServiceProvider? _provider;

    /// <summary>Fournisseur de services applicatif (construit une fois au démarrage de l'app).</summary>
    public static IServiceProvider Provider =>
        _provider ?? throw new InvalidOperationException("AppServices.Initialize() doit être appelé au démarrage.");

    public static void Initialize()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ITokenStore, InMemoryTokenStore>();
        services.AddTransient<AuthHeaderHandler>();

        services.AddHttpClient<AtlasApiClient>(client =>
            {
                client.BaseAddress = new Uri(DevApiBaseAddress);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

        _provider = services.BuildServiceProvider();
    }

    /// <summary>Raccourci de résolution.</summary>
    public static T Get<T>() where T : notnull => Provider.GetRequiredService<T>();
}
