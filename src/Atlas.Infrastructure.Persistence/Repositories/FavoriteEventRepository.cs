using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class FavoriteEventRepository(AtlasDbContext dbContext) : IFavoriteEventRepository
{
    public async Task AddAsync(FavoriteEvent favoriteEvent, CancellationToken ct = default) =>
        await dbContext.FavoriteEvents.AddAsync(favoriteEvent, ct);

    public async Task<IReadOnlyList<FavoriteEvent>> GetForUserAsync(
        UserId userId,
        DateTimeOffset? after,
        DateTimeOffset? before,
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

        return await query
            .OrderByDescending(evt => evt.OccurredAt)
            .Take(limit)
            .ToListAsync(ct);
    }
}
