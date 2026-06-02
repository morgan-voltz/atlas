using Atlas.Domain.Users;
using Atlas.Domain.Veille;

namespace Atlas.Domain.Favorites;

public interface IFavoriteEventRepository
{
    Task AddAsync(FavoriteEvent favoriteEvent, CancellationToken ct = default);

    /// <summary>
    /// Renvoie les événements d'un utilisateur sur la fenêtre temporelle demandée (F-047 volet 2,
    /// utilisé par la fusion timeline). <paramref name="limit"/> plafonne pour éviter d'exploser
    /// la mémoire quand un user a beaucoup d'historique.
    /// </summary>
    Task<IReadOnlyList<FavoriteEvent>> GetForUserAsync(
        UserId userId,
        DateTimeOffset? after,
        DateTimeOffset? before,
        TimelineCursor? cursor,
        int limit,
        CancellationToken ct = default);

    /// <summary>
    /// Renvoie les <see cref="FavoriteEvent.ExternalId"/> déjà connus pour ce user (F-048) afin
    /// d'éviter de recréer des doublons lors d'un nouveau polling BODACC.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetKnownExternalIdsAsync(
        UserId userId,
        IReadOnlyCollection<string> candidateExternalIds,
        CancellationToken ct = default);

    /// <summary>
    /// Variante batch (audit Lot 3b, E4c) : pour un ensemble d'utilisateurs et un même lot d'identifiants
    /// candidats, renvoie en une seule requête les identifiants déjà connus, regroupés par utilisateur.
    /// Évite le N+1 (un <see cref="GetKnownExternalIdsAsync"/> par favori) du polling BODACC.
    /// </summary>
    Task<IReadOnlyDictionary<UserId, IReadOnlyCollection<string>>> GetKnownExternalIdsForUsersAsync(
        IReadOnlyCollection<UserId> userIds,
        IReadOnlyCollection<string> candidateExternalIds,
        CancellationToken ct = default);
}
