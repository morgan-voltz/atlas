using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class CompanyFavoriteSnapshotRepository(AtlasDbContext dbContext) : ICompanyFavoriteSnapshotRepository
{
    public Task<CompanyFavoriteSnapshot?> GetCurrentAsync(UserId userId, Siren siren, CancellationToken ct = default) =>
        dbContext.CompanyFavoriteSnapshots
            .FirstOrDefaultAsync(snap => snap.UserId == userId && snap.Siren == siren, ct);

    public async Task<IReadOnlyList<CompanyFavoriteSnapshot>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        // Suivi (tracking) volontaire : le job de refresh peut ensuite Remove/Add ces snapshots.
        await dbContext.CompanyFavoriteSnapshots
            .Where(snap => snap.UserId == userId)
            .ToListAsync(ct);

    public async Task AddAsync(CompanyFavoriteSnapshot snapshot, CancellationToken ct = default) =>
        await dbContext.CompanyFavoriteSnapshots.AddAsync(snapshot, ct);

    public Task RemoveAsync(CompanyFavoriteSnapshot snapshot, CancellationToken ct = default)
    {
        dbContext.CompanyFavoriteSnapshots.Remove(snapshot);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<UserId>> GetUserIdsWithFavoritesAsync(CancellationToken ct = default) =>
        await dbContext.CompanyFavorites
            .Select(favorite => favorite.UserId)
            .Distinct()
            .ToListAsync(ct);
}
