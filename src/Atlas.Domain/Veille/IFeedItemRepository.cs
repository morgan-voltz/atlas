using Atlas.Domain.Users;

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

    /// <summary>Items pas encore rattachés à un cluster (F-045), les plus anciens d'abord, plafonnés à <paramref name="max"/>.</summary>
    Task<IReadOnlyList<FeedItem>> GetUnclusteredAsync(int max, CancellationToken ct = default);

    /// <summary>
    /// Items <see cref="FeedItem.FetchedAt"/> strictement &gt; <paramref name="since"/>,
    /// plafonnés à <paramref name="max"/>, plus anciens d'abord. Sert à l'évaluation incrémentale
    /// des règles de surveillance (F-046).
    /// </summary>
    Task<IReadOnlyList<FeedItem>> ListFetchedSinceAsync(DateTimeOffset since, int max, CancellationToken ct = default);

    /// <summary>
    /// Timeline d'un utilisateur (F-044), en pagination keyset : items des sources abonnées, filtrés, avec
    /// l'état de lecture/favori/archivage. Renvoie au plus <paramref name="limit"/> entrées ordonnées
    /// <c>(PublishedAt DESC, Id DESC)</c> et situées strictement après <paramref name="cursor"/>
    /// (<c>null</c> = première page).
    /// </summary>
    Task<IReadOnlyList<TimelineEntry>> GetTimelineAsync(
        UserId userId,
        TimelineFilter filter,
        TimelineCursor? cursor,
        int limit,
        CancellationToken ct = default);
}
