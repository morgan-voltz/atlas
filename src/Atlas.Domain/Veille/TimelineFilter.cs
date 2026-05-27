namespace Atlas.Domain.Veille;

/// <summary>
/// Critères de filtrage de la timeline d'un utilisateur (F-044). Tous optionnels ; les drapeaux par défaut
/// donnent la timeline « entrante » (non archivée, tous statuts de lecture).
/// </summary>
public sealed record TimelineFilter(
    FeedSourceId? SourceId = null,
    DateTimeOffset? PublishedAfter = null,
    DateTimeOffset? PublishedBefore = null,
    string? Keyword = null,
    bool UnreadOnly = false,
    bool FavoritesOnly = false,
    bool IncludeArchived = false);
