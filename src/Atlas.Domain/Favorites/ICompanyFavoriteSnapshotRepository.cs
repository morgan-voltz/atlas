using Atlas.Domain.Companies;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

public interface ICompanyFavoriteSnapshotRepository
{
    /// <summary>Renvoie le dernier snapshot connu pour ce (user, siren), ou null s'il n'y en a aucun.</summary>
    Task<CompanyFavoriteSnapshot?> GetCurrentAsync(UserId userId, Siren siren, CancellationToken ct = default);

    /// <summary>
    /// Renvoie tous les snapshots vivants d'un utilisateur (au plus un par siren). Permet de précharger
    /// l'ensemble en une requête plutôt qu'un <see cref="GetCurrentAsync"/> par favori (audit Lot 3, E4b).
    /// </summary>
    Task<IReadOnlyList<CompanyFavoriteSnapshot>> GetByUserAsync(UserId userId, CancellationToken ct = default);

    Task AddAsync(CompanyFavoriteSnapshot snapshot, CancellationToken ct = default);

    /// <summary>Supprime le snapshot précédent (au plus un par (user, siren) — politique « 1 snapshot vivant »).</summary>
    Task RemoveAsync(CompanyFavoriteSnapshot snapshot, CancellationToken ct = default);

    /// <summary>Renvoie les couples (UserId, Favori) à actualiser pour le job nocturne.</summary>
    Task<IReadOnlyList<UserId>> GetUserIdsWithFavoritesAsync(CancellationToken ct = default);
}
