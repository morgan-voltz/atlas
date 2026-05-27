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
}
