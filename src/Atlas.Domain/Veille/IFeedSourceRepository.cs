namespace Atlas.Domain.Veille;

public interface IFeedSourceRepository
{
    Task<IReadOnlyList<FeedSource>> GetActiveAsync(CancellationToken ct = default);

    Task<bool> ExistsByUrlAsync(string url, CancellationToken ct = default);

    Task<FeedSource?> GetByUrlAsync(string url, CancellationToken ct = default);

    Task<IReadOnlyList<FeedSource>> GetByIdsAsync(IReadOnlyCollection<FeedSourceId> ids, CancellationToken ct = default);

    Task AddAsync(FeedSource source, CancellationToken ct = default);

    void Update(FeedSource source);
}
