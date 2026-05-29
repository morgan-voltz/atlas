using Atlas.Domain.Common;
using Atlas.Domain.Downloads;
using Atlas.Domain.Favorites;
using Atlas.Domain.Inpi;
using Atlas.Domain.Notifications;
using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence;

public sealed class AtlasDbContext(DbContextOptions<AtlasDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<TwoFactorRecoveryCode> TwoFactorRecoveryCodes => Set<TwoFactorRecoveryCode>();

    public DbSet<InpiCredentials> InpiCredentials => Set<InpiCredentials>();

    public DbSet<SearchHistoryEntry> SearchHistory => Set<SearchHistoryEntry>();

    public DbSet<FeedSource> FeedSources => Set<FeedSource>();

    public DbSet<FeedItem> FeedItems => Set<FeedItem>();

    public DbSet<FeedItemCluster> FeedItemClusters => Set<FeedItemCluster>();

    public DbSet<VeilleSubscription> VeilleSubscriptions => Set<VeilleSubscription>();

    public DbSet<FeedItemUserState> FeedItemUserStates => Set<FeedItemUserState>();

    public DbSet<VeillePack> VeillePacks => Set<VeillePack>();

    public DbSet<VeillePackEnrollment> VeillePackEnrollments => Set<VeillePackEnrollment>();

    public DbSet<CompanyFavorite> CompanyFavorites => Set<CompanyFavorite>();

    public DbSet<CompanyFavoriteSnapshot> CompanyFavoriteSnapshots => Set<CompanyFavoriteSnapshot>();

    public DbSet<DeviceRegistration> DeviceRegistrations => Set<DeviceRegistration>();

    public DbSet<FeedItemFavoriteMatch> FeedItemFavoriteMatches => Set<FeedItemFavoriteMatch>();

    public DbSet<FavoriteEvent> FavoriteEvents => Set<FavoriteEvent>();

    public DbSet<TrademarkFavorite> TrademarkFavorites => Set<TrademarkFavorite>();

    public DbSet<PatentFavorite> PatentFavorites => Set<PatentFavorite>();

    public DbSet<BulkDownloadJob> BulkDownloadJobs => Set<BulkDownloadJob>();

    public DbSet<FeedRule> FeedRules => Set<FeedRule>();

    public DbSet<VeillePackLike> VeillePackLikes => Set<VeillePackLike>();

    public DbSet<VeillePackReport> VeillePackReports => Set<VeillePackReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtlasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
