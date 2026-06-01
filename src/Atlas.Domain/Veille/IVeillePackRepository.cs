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
    /// Incrémente atomiquement le compteur de likes (F-049) côté base, sans lecture préalable —
    /// évite le <em>lost update</em> entre deux likes concurrents (audit Lot 3, M7).
    /// </summary>
    Task IncrementLikesAsync(VeillePackId id, CancellationToken ct = default);

    /// <summary>Décrémente atomiquement le compteur de likes, borné à 0 (audit Lot 3, M7).</summary>
    Task DecrementLikesAsync(VeillePackId id, CancellationToken ct = default);

    /// <summary>
    /// Catalogue communautaire (F-049) — packs publics actifs, triés par <see cref="VeillePack.LikesCount"/>
    /// décroissant puis <see cref="VeillePack.CreatedAt"/> décroissant pour départager.
    /// </summary>
    Task<PagedResult<VeillePack>> GetPublicMarketplaceAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Packs créés par un utilisateur donné (F-049), inclut brouillons et publiés.</summary>
    Task<IReadOnlyList<VeillePack>> GetByAuthorAsync(UserId authorUserId, CancellationToken ct = default);
}
