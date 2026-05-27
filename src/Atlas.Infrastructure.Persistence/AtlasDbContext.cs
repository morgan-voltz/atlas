using Atlas.Domain.Common;
using Atlas.Domain.Inpi;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtlasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
