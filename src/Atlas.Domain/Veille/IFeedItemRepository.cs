using Atlas.Domain.Users;
using Atlas.Shared.Result;

namespace Atlas.Domain.Veille;

public interface IFeedItemRepository
{
    /// <summary>Parmi les hashes fournis, renvoie ceux déjà présents pour cette source (déduplication).</summary>
    Task<IReadOnlyCollection<string>> GetExistingHashesAsync(
        FeedSourceId sourceId,
        IReadOnlyCollection<string> hashes,
        CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<FeedItem> items, CancellationToken ct = default);

    Task<IReadOnlyList<FeedItem>> GetRecentAsync(int page, int pageSize, CancellationToken ct = default);

    Task<long> CountAsync(CancellationToken ct = default);

    Task<bool> ExistsAsync(FeedItemId id, CancellationToken ct = default);

    /// <summary>
    /// Timeline d'un utilisateur (F-044) : items des sources auxquelles il est abonné, filtrés et paginés,
    /// avec l'état de lecture/favori/archivage de l'utilisateur.
    /// </summary>
    Task<PagedResult<TimelineEntry>> GetTimelineAsync(
        UserId userId,
        TimelineFilter filter,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
