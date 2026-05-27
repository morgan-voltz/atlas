namespace Atlas.Domain.Veille;

public interface IVeillePackRepository
{
    Task<IReadOnlyList<VeillePack>> GetActiveAsync(CancellationToken ct = default);

    Task<VeillePack?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<VeillePack?> GetByIdAsync(VeillePackId id, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);

    Task AddAsync(VeillePack pack, CancellationToken ct = default);

    void Update(VeillePack pack);
}
