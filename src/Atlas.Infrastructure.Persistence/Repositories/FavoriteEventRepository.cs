using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class FavoriteEventRepository(AtlasDbContext dbContext) : IFavoriteEventRepository
{
    public async Task AddAsync(FavoriteEvent favoriteEvent, CancellationToken ct = default) =>
        await dbContext.FavoriteEvents.AddAsync(favoriteEvent, ct);

    // Aligné sur l'ordre des uuid PostgreSQL (cf. FeedItemRepository) pour le départage keyset en mémoire.
    private static readonly IComparer<Guid> GuidComparer = Comparer<Guid>.Create(TimelineKeyset.CompareGuid);

    public async Task<IReadOnlyList<FavoriteEvent>> GetForUserAsync(
        UserId userId,
        DateTimeOffset? after,
        DateTimeOffset? before,
        TimelineCursor? cursor,
        int limit,
        CancellationToken ct = default)
    {
        IQueryable<FavoriteEvent> query = dbContext.FavoriteEvents
            .Where(evt => evt.UserId == userId);

        if (after is { } afterValue)
        {
            query = query.Where(evt => evt.OccurredAt >= afterValue);
        }

        if (before is { } beforeValue)
        {
            query = query.Where(evt => evt.OccurredAt <= beforeValue);
        }

        // Pagination keyset sur (OccurredAt DESC, Id DESC), cohérente avec la timeline fusionnée.
        if (cursor is null)
        {
            return await query
                .OrderByDescending(evt => evt.OccurredAt)
                .ThenByDescending(evt => evt.Id)
                .Take(limit)
                .ToListAsync(ct);
        }

        List<FavoriteEvent> older = await query
            .Where(evt => evt.OccurredAt < cursor.OccurredAt)
            .OrderByDescending(evt => evt.OccurredAt)
            .ThenByDescending(evt => evt.Id)
            .Take(limit)
            .ToListAsync(ct);

        // Événements au même horodatage que le curseur : départage par Id en mémoire (EF ne compare pas les Guid).
        List<FavoriteEvent> equalTime = await query
            .Where(evt => evt.OccurredAt == cursor.OccurredAt)
            .ToListAsync(ct);

        IEnumerable<FavoriteEvent> equalAfterCursor = equalTime
            .Where(evt => TimelineKeyset.CompareGuid(evt.Id.Value, cursor.Id) < 0)
            .OrderByDescending(evt => evt.Id.Value, GuidComparer);

        return equalAfterCursor.Concat(older).Take(limit).ToList();
    }

    public async Task<IReadOnlyCollection<string>> GetKnownExternalIdsAsync(
        UserId userId,
        IReadOnlyCollection<string> candidateExternalIds,
        CancellationToken ct = default)
    {
        if (candidateExternalIds.Count == 0)
        {
            return [];
        }

        return await dbContext.FavoriteEvents
            .Where(evt => evt.UserId == userId
                && evt.ExternalId != null
                && candidateExternalIds.Contains(evt.ExternalId))
            .Select(evt => evt.ExternalId!)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyDictionary<UserId, IReadOnlyCollection<string>>> GetKnownExternalIdsForUsersAsync(
        IReadOnlyCollection<UserId> userIds,
        IReadOnlyCollection<string> candidateExternalIds,
        CancellationToken ct = default)
    {
        if (userIds.Count == 0 || candidateExternalIds.Count == 0)
        {
            return new Dictionary<UserId, IReadOnlyCollection<string>>();
        }

        var rows = await dbContext.FavoriteEvents
            .Where(evt => userIds.Contains(evt.UserId)
                && evt.ExternalId != null
                && candidateExternalIds.Contains(evt.ExternalId))
            .Select(evt => new { evt.UserId, ExternalId = evt.ExternalId! })
            .ToListAsync(ct);

        return rows
            .GroupBy(row => row.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<string>)group.Select(row => row.ExternalId).ToHashSet());
    }
}
