using Atlas.Domain.Companies;
using Atlas.Domain.Inpi;
using Atlas.Infrastructure.Inpi.Authentication;
using Atlas.Infrastructure.Inpi.Common;
using Atlas.Infrastructure.Inpi.Rne;
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
        services.AddMemoryCache();

        services.AddHttpClient<IInpiAuthenticationProvider, InpiAuthenticationProvider>(ConfigureRneClient);
        services.AddHttpClient<ICompanyDataProvider, RneCompanyProvider>(ConfigureRneClient);

        return services;
    }

    private static void ConfigureRneClient(IServiceProvider provider, HttpClient client)
    {
        InpiOptions options = provider.GetRequiredService<IOptions<InpiOptions>>().Value;
        client.BaseAddress = new Uri(options.RneBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    }
}
