using Atlas.Domain.Common;
using Atlas.Domain.Downloads;
using Atlas.Domain.Favorites;
using Atlas.Domain.Inpi;
using Atlas.Domain.Notifications;
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
        services.AddScoped<IFeedItemClusterRepository, FeedItemClusterRepository>();
        services.AddScoped<IVeilleSubscriptionRepository, VeilleSubscriptionRepository>();
        services.AddScoped<IFeedItemUserStateRepository, FeedItemUserStateRepository>();
        services.AddScoped<IVeillePackRepository, VeillePackRepository>();
        services.AddScoped<IVeillePackEnrollmentRepository, VeillePackEnrollmentRepository>();
        services.AddScoped<ICompanyFavoriteRepository, CompanyFavoriteRepository>();
        services.AddScoped<ICompanyFavoriteSnapshotRepository, CompanyFavoriteSnapshotRepository>();
        services.AddScoped<IDeviceRegistrationRepository, DeviceRegistrationRepository>();
        services.AddScoped<IFeedItemFavoriteMatchRepository, FeedItemFavoriteMatchRepository>();
        services.AddScoped<IFavoriteEventRepository, FavoriteEventRepository>();
        services.AddScoped<ITrademarkFavoriteRepository, TrademarkFavoriteRepository>();
        services.AddScoped<IPatentFavoriteRepository, PatentFavoriteRepository>();
        services.AddScoped<IBulkDownloadJobRepository, BulkDownloadJobRepository>();
        services.AddScoped<IFeedRuleRepository, FeedRuleRepository>();
        services.AddScoped<IVeillePackLikeRepository, VeillePackLikeRepository>();
        services.AddScoped<IVeillePackReportRepository, VeillePackReportRepository>();

        return services;
    }
}
