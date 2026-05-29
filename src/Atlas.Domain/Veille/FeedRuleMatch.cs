namespace Atlas.Domain.Veille;

/// <summary>
/// Vue simplifiée d'un item ayant déclenché une <see cref="FeedRule"/> (F-046). Sert de payload
/// à l'envoi de notifications (email / push) sans coupler la couche notification à
/// l'entité <see cref="FeedItem"/> elle-même.
/// </summary>
public sealed record FeedRuleMatch(
    FeedItemId FeedItemId,
    string Title,
    string? Url,
    string? Summary,
    string SourceName,
    DateTimeOffset PublishedAt);
