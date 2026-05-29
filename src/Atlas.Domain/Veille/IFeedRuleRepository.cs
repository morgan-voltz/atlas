using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

/// <summary>
/// Port d'accès aux règles de surveillance personnalisées (F-046).
/// </summary>
public interface IFeedRuleRepository
{
    Task<FeedRule?> GetByIdAsync(FeedRuleId id, CancellationToken ct = default);

    Task<IReadOnlyList<FeedRule>> ListByUserAsync(UserId userId, CancellationToken ct = default);

    /// <summary>Toutes les règles actives, tous utilisateurs confondus (job d'évaluation post-polling).</summary>
    Task<IReadOnlyList<FeedRule>> ListActiveAsync(CancellationToken ct = default);

    Task AddAsync(FeedRule rule, CancellationToken ct = default);

    Task RemoveAsync(FeedRule rule, CancellationToken ct = default);
}
