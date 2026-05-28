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

    public async Task<IReadOnlyList<FeedItem>> GetUnclusteredAsync(int max, CancellationToken ct = default) =>
        await dbContext.FeedItems
            .Where(item => item.ClusterId == null)
            .OrderBy(item => item.PublishedAt)
            .ThenBy(item => item.FetchedAt)
            .Take(max)
            .ToListAsync(ct);

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

        // F-047 : restriction aux items mentionnant au moins un favori du user.
        if (filter.MentionsFavoritesOnly)
        {
            query = query.Where(row => dbContext.FeedItemFavoriteMatches
                .Any(m => m.UserId == userId && m.FeedItemId == row.item.Id));
        }

        // Collapse de déduplication (F-045) : on n'affiche qu'un représentant par cluster, l'item le plus récent
        // PARMI les sources auxquelles l'utilisateur est abonné (un cluster peut contenir des items de sources
        // non suivies). Départage déterministe sur (PublishedAt, FetchedAt). Les items standalone passent toujours.
        query = query.Where(row =>
            row.item.ClusterId == null
            || !dbContext.FeedItems.Any(sib =>
                sib.ClusterId == row.item.ClusterId
                && sib.Id != row.item.Id
                && dbContext.VeilleSubscriptions.Any(s => s.UserId == userId && s.SourceId == sib.SourceId)
                && (sib.PublishedAt > row.item.PublishedAt
                    || (sib.PublishedAt == row.item.PublishedAt && sib.FetchedAt > row.item.FetchedAt))));

        long total = await query.LongCountAsync(ct);

        var rows = await query
            .OrderByDescending(row => row.item.PublishedAt)
            .ThenByDescending(row => row.item.FetchedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new
            {
                row.item,
                row.state,
                SourceCount = row.item.ClusterId == null
                    ? 1
                    : dbContext.FeedItems
                        .Where(f => f.ClusterId == row.item.ClusterId)
                        .Select(f => f.SourceId)
                        .Distinct()
                        .Count(),
            })
            .ToListAsync(ct);

        // F-047 : charge en une requête les mentions de favoris pour les items affichés.
        var displayedIds = rows.Select(r => r.item.Id).ToList();
        var mentionsByItem = await dbContext.FeedItemFavoriteMatches
            .Where(m => m.UserId == userId && displayedIds.Contains(m.FeedItemId))
            .Select(m => new { ItemId = m.FeedItemId.Value, m.Siren, m.MatchedName })
            .ToListAsync(ct)
            .ContinueWith(t => t.Result
                .GroupBy(x => x.ItemId)
                .ToDictionary(g => g.Key, g => g
                    .Select(x => new FavoriteMention(x.Siren.Value, x.MatchedName))
                    .ToList()), ct);

        IReadOnlyList<TimelineEntry> entries = rows
            .Select(row => new TimelineEntry(
                row.item,
                row.state != null && row.state.IsRead,
                row.state != null && row.state.IsFavorite,
                row.state != null && row.state.IsArchived,
                row.SourceCount,
                mentionsByItem.TryGetValue(row.item.Id.Value, out List<FavoriteMention>? mentions)
                    ? mentions
                    : []))
            .ToList();

        return new PagedResult<TimelineEntry>(entries, page, pageSize, total);
    }
}
