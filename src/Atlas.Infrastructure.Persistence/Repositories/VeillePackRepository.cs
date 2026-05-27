using Atlas.Domain.Veille;
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
}
