using Atlas.Domain.Companies;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

public interface ICompanyFavoriteSnapshotRepository
{
    /// <summary>Renvoie le dernier snapshot connu pour ce (user, siren), ou null s'il n'y en a aucun.</summary>
    Task<CompanyFavoriteSnapshot?> GetCurrentAsync(UserId userId, Siren siren, CancellationToken ct = default);

    Task AddAsync(CompanyFavoriteSnapshot snapshot, CancellationToken ct = default);

    /// <summary>Supprime le snapshot précédent (au plus un par (user, siren) — politique « 1 snapshot vivant »).</summary>
    Task RemoveAsync(CompanyFavoriteSnapshot snapshot, CancellationToken ct = default);

    /// <summary>Renvoie les couples (UserId, Favori) à actualiser pour le job nocturne.</summary>
    Task<IReadOnlyList<UserId>> GetUserIdsWithFavoritesAsync(CancellationToken ct = default);
}
