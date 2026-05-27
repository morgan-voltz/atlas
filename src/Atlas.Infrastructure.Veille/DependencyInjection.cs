using Atlas.Domain.Veille;
using Atlas.Infrastructure.Veille.Deduplication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Veille;

public static class DependencyInjection
{
    public static IServiceCollection AddVeilleInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<VeilleOptions>(configuration.GetSection(VeilleOptions.SectionName));

        services.AddHttpClient<IExternalContentSource, RssFeedProvider>(ConfigureClient);

        services.AddSingleton<IFeedSubscriptionPolicy, FeedSubscriptionPolicy>();
        services.AddSingleton<IDeduplicationPolicy, DeduplicationPolicy>();

        return services;
    }

    private static void ConfigureClient(IServiceProvider provider, HttpClient client)
    {
        VeilleOptions options = provider.GetRequiredService<IOptions<VeilleOptions>>().Value;
        client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
    }
}
