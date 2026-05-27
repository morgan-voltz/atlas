using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class SearchHistoryRepository(AtlasDbContext dbContext) : ISearchHistoryRepository
{
    public async Task AddAsync(SearchHistoryEntry entry, CancellationToken ct = default) =>
        await dbContext.SearchHistory.AddAsync(entry, ct);

    public async Task<IReadOnlyList<SearchHistoryEntry>> GetRecentByUserAsync(
        UserId userId,
        int limit,
        CancellationToken ct = default) =>
        await dbContext.SearchHistory
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task PruneAsync(UserId userId, int maxEntries, CancellationToken ct = default)
    {
        // Date de la (maxEntries)-ième entrée la plus récente : tout ce qui est plus ancien est supprimé.
        DateTimeOffset cutoff = await dbContext.SearchHistory
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.CreatedAt)
            .Skip(maxEntries - 1)
            .Select(entry => entry.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (cutoff == default)
        {
            return;
        }

        await dbContext.SearchHistory
            .Where(entry => entry.UserId == userId && entry.CreatedAt < cutoff)
            .ExecuteDeleteAsync(ct);
    }
}
