using Atlas.Domain.Users;

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
}
