using Atlas.Domain.Bodacc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Bodacc;

public static class DependencyInjection
{
    public static IServiceCollection AddBodaccInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BodaccOptions>(configuration.GetSection(BodaccOptions.SectionName));

        services.AddHttpClient<IBodaccProvider, OpendatasoftBodaccProvider>((provider, client) =>
        {
            BodaccOptions options = provider.GetRequiredService<IOptions<BodaccOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Atlas-Backend/1.0");
        });

        return services;
    }
}
