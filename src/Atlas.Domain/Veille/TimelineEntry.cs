namespace Atlas.Domain.Veille;

/// <summary>
/// Élément de timeline : un <see cref="FeedItem"/> accompagné de l'état de l'utilisateur courant (F-044).
/// Read-model retourné par <see cref="IFeedItemRepository.GetTimelineAsync"/>.
/// </summary>
public sealed record TimelineEntry(FeedItem Item, bool IsRead, bool IsFavorite, bool IsArchived);
