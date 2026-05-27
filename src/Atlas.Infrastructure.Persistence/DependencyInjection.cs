using Atlas.Domain.Common;
using Atlas.Domain.Inpi;
using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Atlas");

        services.AddDbContext<AtlasDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AtlasDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITwoFactorRecoveryCodeRepository, TwoFactorRecoveryCodeRepository>();
        services.AddScoped<IInpiCredentialsRepository, InpiCredentialsRepository>();
        services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();
        services.AddScoped<IFeedSourceRepository, FeedSourceRepository>();
        services.AddScoped<IFeedItemRepository, FeedItemRepository>();

        return services;
    }
}
