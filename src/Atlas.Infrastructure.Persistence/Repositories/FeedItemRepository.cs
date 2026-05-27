using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class FeedItemRepository(AtlasDbContext dbContext) : IFeedItemRepository
{
    public async Task<IReadOnlyCollection<string>> GetExistingHashesAsync(
        FeedSourceId sourceId,
        IReadOnlyCollection<string> hashes,
        CancellationToken ct = default) =>
        await dbContext.FeedItems
            .Where(item => item.SourceId == sourceId && hashes.Contains(item.ContentHash))
            .Select(item => item.ContentHash)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<FeedItem> items, CancellationToken ct = default) =>
        await dbContext.FeedItems.AddRangeAsync(items, ct);

    public async Task<IReadOnlyList<FeedItem>> GetRecentAsync(int page, int pageSize, CancellationToken ct = default) =>
        await dbContext.FeedItems
            .OrderByDescending(item => item.PublishedAt)
            .ThenByDescending(item => item.FetchedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<long> CountAsync(CancellationToken ct = default) =>
        await dbContext.FeedItems.LongCountAsync(ct);
}
