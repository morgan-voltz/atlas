using Atlas.Domain.Bodacc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Bodacc;

public static class DependencyInjection
{
    public static IServiceCollection AddBodaccInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BodaccOptions>(configuration.GetSection(BodaccOptions.SectionName));

        // Résilience (audit Lot 2) : retry transitoire + circuit breaker + timeout par tentative.
        services.AddHttpClient<IBodaccProvider, OpendatasoftBodaccProvider>((provider, client) =>
        {
            BodaccOptions options = provider.GetRequiredService<IOptions<BodaccOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = Timeout.InfiniteTimeSpan; // géré par le pipeline de résilience
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Atlas-Backend/1.0");
        })
            .AddStandardResilienceHandler().Configure(ConfigureResilience);

        return services;
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options, IServiceProvider provider)
    {
        int seconds = provider.GetRequiredService<IOptions<BodaccOptions>>().Value.TimeoutSeconds;
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(seconds);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(seconds * 4);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(seconds * 2);
    }
}
