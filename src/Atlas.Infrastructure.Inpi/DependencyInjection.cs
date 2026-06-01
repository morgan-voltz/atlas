using Atlas.Domain.Companies;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Infrastructure.Inpi.Authentication;
using Atlas.Infrastructure.Inpi.Common;
using Atlas.Infrastructure.Inpi.Pi;
using Atlas.Infrastructure.Inpi.Rne;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Inpi;

public static class DependencyInjection
{
    public static IServiceCollection AddInpiInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<InpiOptions>(configuration.GetSection(InpiOptions.SectionName));
        services.AddMemoryCache();

        // Résilience (audit Lot 2, ADR-007) : retry exponentiel + circuit breaker + timeout par tentative
        // sur tous les appels INPI, pour absorber les hoquets transitoires et éviter le fail-storm si l'INPI
        // est indisponible. Le retry standard ne cible que les erreurs transitoires (5xx/408/429/timeout).
        services.AddHttpClient<IInpiAuthenticationProvider, InpiAuthenticationProvider>(ConfigureRneClient)
            .AddStandardResilienceHandler().Configure(ConfigureResilience);
        services.AddHttpClient<ICompanyDataProvider, RneCompanyProvider>(ConfigureRneClient)
            .AddStandardResilienceHandler().Configure(ConfigureResilience);

        services
            .AddHttpClient<IIntellectualPropertyProvider, InpiPiTrademarkProvider>(ConfigurePiClient)
            .ConfigurePrimaryHttpMessageHandler(static () => new HttpClientHandler
            {
                // Multi-tenant : on gère les cookies (access_token / XSRF) manuellement, pas de container partagé.
                UseCookies = false,
                AllowAutoRedirect = false,
            })
            .AddStandardResilienceHandler().Configure(ConfigureResilience);

        return services;
    }

    private static void ConfigureRneClient(IServiceProvider provider, HttpClient client)
    {
        InpiOptions options = provider.GetRequiredService<IOptions<InpiOptions>>().Value;
        client.BaseAddress = new Uri(options.RneBaseUrl);
        // Le timeout est désormais géré par le pipeline de résilience (par tentative + total).
        client.Timeout = Timeout.InfiniteTimeSpan;
    }

    private static void ConfigurePiClient(IServiceProvider provider, HttpClient client)
    {
        InpiOptions options = provider.GetRequiredService<IOptions<InpiOptions>>().Value;
        client.BaseAddress = new Uri(options.PiBaseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options, IServiceProvider provider)
    {
        int seconds = provider.GetRequiredService<IOptions<InpiOptions>>().Value.TimeoutSeconds;
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(seconds);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(seconds * 4);
        // Contrainte du handler standard : SamplingDuration >= 2 × AttemptTimeout.
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(seconds * 2);
    }
}
