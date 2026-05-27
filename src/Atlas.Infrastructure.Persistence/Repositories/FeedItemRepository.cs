using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
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

    public async Task<bool> ExistsAsync(FeedItemId id, CancellationToken ct = default) =>
        await dbContext.FeedItems.AnyAsync(item => item.Id == id, ct);

    public async Task<PagedResult<TimelineEntry>> GetTimelineAsync(
        UserId userId,
        TimelineFilter filter,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        // Items des sources abonnées par l'utilisateur, LEFT JOIN sur son état (lu/favori/archivé).
        var query =
            from item in dbContext.FeedItems
            where dbContext.VeilleSubscriptions.Any(sub => sub.UserId == userId && sub.SourceId == item.SourceId)
            join state in dbContext.FeedItemUserStates.Where(s => s.UserId == userId)
                on item.Id equals state.FeedItemId into stateGroup
            from state in stateGroup.DefaultIfEmpty()
            select new { item, state };

        if (filter.SourceId is { } sourceId)
        {
            query = query.Where(row => row.item.SourceId == sourceId);
        }

        if (filter.PublishedAfter is { } after)
        {
            query = query.Where(row => row.item.PublishedAt >= after);
        }

        if (filter.PublishedBefore is { } before)
        {
            query = query.Where(row => row.item.PublishedAt <= before);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            string pattern = $"%{filter.Keyword}%";
            query = query.Where(row =>
                EF.Functions.ILike(row.item.Title, pattern)
                || (row.item.Summary != null && EF.Functions.ILike(row.item.Summary, pattern)));
        }

        if (filter.UnreadOnly)
        {
            query = query.Where(row => row.state == null || !row.state.IsRead);
        }

        if (filter.FavoritesOnly)
        {
            query = query.Where(row => row.state != null && row.state.IsFavorite);
        }

        if (!filter.IncludeArchived)
        {
            query = query.Where(row => row.state == null || !row.state.IsArchived);
        }

        long total = await query.LongCountAsync(ct);

        var rows = await query
            .OrderByDescending(row => row.item.PublishedAt)
            .ThenByDescending(row => row.item.FetchedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        IReadOnlyList<TimelineEntry> entries = rows
            .Select(row => new TimelineEntry(
                row.item,
                row.state != null && row.state.IsRead,
                row.state != null && row.state.IsFavorite,
                row.state != null && row.state.IsArchived))
            .ToList();

        return new PagedResult<TimelineEntry>(entries, page, pageSize, total);
    }
}
