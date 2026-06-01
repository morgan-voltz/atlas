using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class VeillePackRepository(AtlasDbContext dbContext) : IVeillePackRepository
{
    // Les Items sont une collection possédée : EF Core les charge automatiquement avec le pack.
    public async Task<IReadOnlyList<VeillePack>> GetActiveAsync(CancellationToken ct = default) =>
        await dbContext.VeillePacks
            .Where(pack => pack.IsActive)
            .OrderBy(pack => pack.Name)
            .ToListAsync(ct);

    public async Task<VeillePack?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        await dbContext.VeillePacks.FirstOrDefaultAsync(pack => pack.Code == code, ct);

    public async Task<VeillePack?> GetByIdAsync(VeillePackId id, CancellationToken ct = default) =>
        await dbContext.VeillePacks.FirstOrDefaultAsync(pack => pack.Id == id, ct);

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default) =>
        await dbContext.VeillePacks.AnyAsync(pack => pack.Code == code, ct);

    public async Task AddAsync(VeillePack pack, CancellationToken ct = default) =>
        await dbContext.VeillePacks.AddAsync(pack, ct);

    public void Update(VeillePack pack) => dbContext.VeillePacks.Update(pack);

    // Incréments atomiques côté serveur (audit Lot 3, M7) : un UPDATE ... SET likes_count = likes_count + 1
    // unique, sans read-modify-write, donc insensible aux likes concurrents.
    public Task IncrementLikesAsync(VeillePackId id, CancellationToken ct = default) =>
        dbContext.VeillePacks
            .Where(pack => pack.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(pack => pack.LikesCount, pack => pack.LikesCount + 1), ct);

    public Task DecrementLikesAsync(VeillePackId id, CancellationToken ct = default) =>
        dbContext.VeillePacks
            .Where(pack => pack.Id == id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(pack => pack.LikesCount, pack => Math.Max(0, pack.LikesCount - 1)), ct);

    public async Task<PagedResult<VeillePack>> GetPublicMarketplaceAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        IQueryable<VeillePack> query = dbContext.VeillePacks
            .Where(pack => pack.IsActive && pack.Visibility == VeillePackVisibility.Public);

        long total = await query.LongCountAsync(ct);

        List<VeillePack> packs = await query
            .OrderByDescending(pack => pack.LikesCount)
            .ThenByDescending(pack => pack.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<VeillePack>(packs, page, pageSize, total);
    }

    public async Task<IReadOnlyList<VeillePack>> GetByAuthorAsync(UserId authorUserId, CancellationToken ct = default) =>
        await dbContext.VeillePacks
            .Where(pack => pack.AuthorUserId == authorUserId)
            .OrderByDescending(pack => pack.CreatedAt)
            .ToListAsync(ct);
}
