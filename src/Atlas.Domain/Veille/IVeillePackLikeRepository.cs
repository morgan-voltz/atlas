using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

/// <summary>
/// Port d'accès aux likes communautaires de packs de veille (F-049).
/// </summary>
public interface IVeillePackLikeRepository
{
    Task<bool> ExistsAsync(UserId userId, VeillePackId veillePackId, CancellationToken ct = default);

    Task<VeillePackLike?> GetAsync(UserId userId, VeillePackId veillePackId, CancellationToken ct = default);

    Task AddAsync(VeillePackLike like, CancellationToken ct = default);

    Task RemoveAsync(VeillePackLike like, CancellationToken ct = default);
}
