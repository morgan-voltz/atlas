using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class VeilleSubscriptionRepository(AtlasDbContext dbContext) : IVeilleSubscriptionRepository
{
    public async Task<bool> ExistsAsync(UserId userId, FeedSourceId sourceId, CancellationToken ct = default) =>
        await dbContext.VeilleSubscriptions
            .AnyAsync(subscription => subscription.UserId == userId && subscription.SourceId == sourceId, ct);

    public async Task<int> CountByUserAsync(UserId userId, CancellationToken ct = default) =>
        await dbContext.VeilleSubscriptions.CountAsync(subscription => subscription.UserId == userId, ct);

    public async Task<VeilleSubscription?> GetAsync(UserId userId, VeilleSubscriptionId id, CancellationToken ct = default) =>
        await dbContext.VeilleSubscriptions
            .FirstOrDefaultAsync(subscription => subscription.UserId == userId && subscription.Id == id, ct);

    public async Task<IReadOnlyList<VeilleSubscription>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await dbContext.VeilleSubscriptions
            .Where(subscription => subscription.UserId == userId)
            .OrderByDescending(subscription => subscription.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(VeilleSubscription subscription, CancellationToken ct = default) =>
        await dbContext.VeilleSubscriptions.AddAsync(subscription, ct);

    public void Remove(VeilleSubscription subscription) => dbContext.VeilleSubscriptions.Remove(subscription);
}
