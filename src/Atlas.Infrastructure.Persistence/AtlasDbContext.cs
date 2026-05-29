using Atlas.Domain.Common;
using Atlas.Domain.Downloads;
using Atlas.Domain.Favorites;
using Atlas.Domain.Inpi;
using Atlas.Domain.Notifications;
using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Atlas.Infrastructure.Persistence;

public sealed class AtlasDbContext(
    DbContextOptions<AtlasDbContext> options,
    IPublisher? domainEventPublisher = null)
    : DbContext(options), IUnitOfWork
{
    private readonly IPublisher? _domainEventPublisher = domainEventPublisher;

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

    /// <summary>
    /// Sémantique « after-commit » : les événements domaine sont collectés depuis les entités tracked,
    /// nettoyés, puis publiés via <see cref="IPublisher"/> après le <c>SaveChangesAsync</c> EF Core réussi.
    /// Si la transaction échoue, aucun événement n'est publié — pas de fuite vers les handlers.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<IDomainEvent> events = CollectAndClearDomainEvents();

        int affected = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (events.Count > 0 && _domainEventPublisher is not null)
        {
            foreach (IDomainEvent domainEvent in events)
            {
                await _domainEventPublisher.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
            }
        }

        return affected;
    }

    private List<IDomainEvent> CollectAndClearDomainEvents()
    {
        List<IDomainEvent> events = [];

        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IHasDomainEvents holder || holder.DomainEvents.Count == 0)
            {
                continue;
            }

            events.AddRange(holder.DomainEvents);
            holder.ClearDomainEvents();
        }

        return events;
    }
}
