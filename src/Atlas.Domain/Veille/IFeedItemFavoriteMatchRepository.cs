using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

public interface IFeedItemFavoriteMatchRepository
{
    /// <summary>
    /// Renvoie les FeedItemId à scanner pour un user : les items récents (limite <paramref name="lookbackDays"/>)
    /// pour lesquels aucun match n'a encore été calculé pour ce user.
    /// </summary>
    Task<IReadOnlyList<FeedItem>> GetCandidatesForUserAsync(
        UserId userId,
        int lookbackDays,
        CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<FeedItemFavoriteMatch> matches, CancellationToken ct = default);

    /// <summary>Renvoie les mentions de favoris pour ces items et ce user (jointure timeline, F-047).</summary>
    Task<IReadOnlyDictionary<FeedItemId, IReadOnlyList<FavoriteMention>>> GetMentionsForUserAsync(
        UserId userId,
        IReadOnlyCollection<FeedItemId> feedItemIds,
        CancellationToken ct = default);

    /// <summary>FeedItemId ayant au moins une mention pour ce user (utilisé par le filtre « favoris uniquement »).</summary>
    Task<IReadOnlyCollection<FeedItemId>> GetMentionedFeedItemIdsAsync(UserId userId, CancellationToken ct = default);
}
