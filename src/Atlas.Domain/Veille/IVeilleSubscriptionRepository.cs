using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

public interface IVeilleSubscriptionRepository
{
    Task<bool> ExistsAsync(UserId userId, FeedSourceId sourceId, CancellationToken ct = default);

    Task<int> CountByUserAsync(UserId userId, CancellationToken ct = default);

    Task<VeilleSubscription?> GetAsync(UserId userId, VeilleSubscriptionId id, CancellationToken ct = default);

    Task<IReadOnlyList<VeilleSubscription>> GetByUserAsync(UserId userId, CancellationToken ct = default);

    Task AddAsync(VeilleSubscription subscription, CancellationToken ct = default);

    void Remove(VeilleSubscription subscription);
}
