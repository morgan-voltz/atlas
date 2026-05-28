using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class FeedItemFavoriteMatchRepository(AtlasDbContext dbContext)
    : IFeedItemFavoriteMatchRepository
{
    public async Task<IReadOnlyList<FeedItem>> GetCandidatesForUserAsync(
        UserId userId,
        int lookbackDays,
        CancellationToken ct = default)
    {
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-lookbackDays);
        return await dbContext.FeedItems
            .Where(item => item.PublishedAt >= cutoff)
            .Where(item => !dbContext.FeedItemFavoriteMatches
                .Any(m => m.UserId == userId && m.FeedItemId == item.Id))
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<FeedItemFavoriteMatch> matches, CancellationToken ct = default) =>
        await dbContext.FeedItemFavoriteMatches.AddRangeAsync(matches, ct);

    public async Task<IReadOnlyDictionary<FeedItemId, IReadOnlyList<FavoriteMention>>> GetMentionsForUserAsync(
        UserId userId,
        IReadOnlyCollection<FeedItemId> feedItemIds,
        CancellationToken ct = default)
    {
        if (feedItemIds.Count == 0)
        {
            return new Dictionary<FeedItemId, IReadOnlyList<FavoriteMention>>();
        }

        var rows = await dbContext.FeedItemFavoriteMatches
            .Where(m => m.UserId == userId && feedItemIds.Contains(m.FeedItemId))
            .Select(m => new { m.FeedItemId, m.Siren, m.MatchedName })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.FeedItemId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<FavoriteMention>)g
                    .Select(x => new FavoriteMention(x.Siren.Value, x.MatchedName))
                    .ToList());
    }

    public async Task<IReadOnlyCollection<FeedItemId>> GetMentionedFeedItemIdsAsync(
        UserId userId,
        CancellationToken ct = default) =>
        await dbContext.FeedItemFavoriteMatches
            .Where(m => m.UserId == userId)
            .Select(m => m.FeedItemId)
            .Distinct()
            .ToListAsync(ct);
}
