using Atlas.Domain.Users;
using Atlas.Shared.Result;

namespace Atlas.Domain.Veille;

public interface IVeillePackRepository
{
    Task<IReadOnlyList<VeillePack>> GetActiveAsync(CancellationToken ct = default);

    Task<VeillePack?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<VeillePack?> GetByIdAsync(VeillePackId id, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);

    Task AddAsync(VeillePack pack, CancellationToken ct = default);

    void Update(VeillePack pack);

    /// <summary>
    /// Catalogue communautaire (F-049) — packs publics actifs, triés par <see cref="VeillePack.LikesCount"/>
    /// décroissant puis <see cref="VeillePack.CreatedAt"/> décroissant pour départager.
    /// </summary>
    Task<PagedResult<VeillePack>> GetPublicMarketplaceAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Packs créés par un utilisateur donné (F-049), inclut brouillons et publiés.</summary>
    Task<IReadOnlyList<VeillePack>> GetByAuthorAsync(UserId authorUserId, CancellationToken ct = default);
}
