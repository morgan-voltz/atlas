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

    public async Task<IReadOnlyList<FeedItem>> ListFetchedSinceAsync(
        DateTimeOffset since,
        int max,
        CancellationToken ct = default) =>
        await dbContext.FeedItems
            .Where(item => item.FetchedAt > since)
            .OrderBy(item => item.FetchedAt)
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
            .Select(row => new { row.item, row.state })
            .ToListAsync(ct);

        // SourceCount (badge « N sources rapportent ») calculé séparément pour les seuls items affichés
        // (audit E5b). Dans la projection paginée, la sous-requête corrélée était évaluée par PostgreSQL pour
        // TOUTES les lignes ordonnées avant l'OFFSET (≈1000 à la page 50 → ~1,8 s) au lieu des 20 retournées.
        // Ici l'ensemble externe est restreint aux items affichés et clusterisés → au plus pageSize sous-requêtes.
        // On réutilise les seules constructions que EF traduit ici : Contains sur FeedItemId (non-nullable, cf.
        // les mentions) et l'égalité ClusterId == ClusterId (nullable, comme la requête d'origine).
        var clusteredDisplayedIds = rows
            .Where(row => row.item.ClusterId is not null)
            .Select(row => row.item.Id)
            .ToList();

        Dictionary<FeedItemId, int> sourceCountByItem = clusteredDisplayedIds.Count == 0
            ? []
            : await dbContext.FeedItems
                .Where(representative => clusteredDisplayedIds.Contains(representative.Id))
                .Select(representative => new
                {
                    representative.Id,
                    Count = dbContext.FeedItems
                        .Where(sibling => sibling.ClusterId == representative.ClusterId)
                        .Select(sibling => sibling.SourceId)
                        .Distinct()
                        .Count(),
                })
                .ToDictionaryAsync(row => row.Id, row => row.Count, ct);

        // F-047 : charge en une requête les mentions de favoris pour les items affichés.
        var displayedIds = rows.Select(r => r.item.Id).ToList();
        var mentionRows = await dbContext.FeedItemFavoriteMatches
            .Where(m => m.UserId == userId && displayedIds.Contains(m.FeedItemId))
            .Select(m => new { ItemId = m.FeedItemId.Value, m.Siren, m.MatchedName })
            .ToListAsync(ct);

        // Regroupement en mémoire (audit Lot 3, E5a : remplace un ContinueWith qui bloquait sur .Result).
        var mentionsByItem = mentionRows
            .GroupBy(x => x.ItemId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new FavoriteMention(x.Siren.Value, x.MatchedName)).ToList());

        IReadOnlyList<TimelineEntry> entries = rows
            .Select(row => new TimelineEntry(
                row.item,
                row.state != null && row.state.IsRead,
                row.state != null && row.state.IsFavorite,
                row.state != null && row.state.IsArchived,
                row.item.ClusterId is null
                    ? 1
                    : sourceCountByItem.GetValueOrDefault(row.item.Id, 1),
                mentionsByItem.TryGetValue(row.item.Id.Value, out List<FavoriteMention>? mentions)
                    ? mentions
                    : []))
            .ToList();

        return new PagedResult<TimelineEntry>(entries, page, pageSize, total);
    }
}
