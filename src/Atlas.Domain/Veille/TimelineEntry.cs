namespace Atlas.Domain.Veille;

/// <summary>
/// Élément de timeline : un <see cref="FeedItem"/> accompagné de l'état de l'utilisateur courant (F-044).
/// Read-model retourné par <see cref="IFeedItemRepository.GetTimelineAsync"/>.
/// <paramref name="SourceCount"/> = nombre de sources distinctes rapportant l'info (F-045, « N sources rapportent ») ;
/// vaut 1 pour un item standalone.
/// <paramref name="MentionedFavorites"/> = entreprises favorites du user mentionnées dans cet item (F-047).
/// </summary>
public sealed record TimelineEntry(
    FeedItem Item,
    bool IsRead,
    bool IsFavorite,
    bool IsArchived,
    int SourceCount,
    IReadOnlyList<FavoriteMention> MentionedFavorites);
