using Atlas.Domain.Inpi;
using Atlas.Infrastructure.Inpi.Authentication;
using Atlas.Infrastructure.Inpi.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Inpi;

public static class DependencyInjection
{
    public static IServiceCollection AddInpiInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<InpiOptions>(configuration.GetSection(InpiOptions.SectionName));

        services.AddHttpClient<IInpiAuthenticationProvider, InpiAuthenticationProvider>((provider, client) =>
        {
            InpiOptions options = provider.GetRequiredService<IOptions<InpiOptions>>().Value;
            client.BaseAddress = new Uri(options.RneBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;
    }
}
