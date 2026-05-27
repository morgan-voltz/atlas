namespace Atlas.Domain.Veille;

/// <summary>
/// Élément brut renvoyé par un <see cref="IExternalContentSource"/> avant transformation en
/// <see cref="FeedItem"/> de domaine (pas encore rattaché à une source ni hashé).
/// </summary>
public sealed record FeedItemDraft(
    string Title,
    string? Url,
    string? Summary,
    DateTimeOffset PublishedAt,
    IReadOnlyList<string> Categories);
