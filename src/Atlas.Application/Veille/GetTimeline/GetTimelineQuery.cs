using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetTimeline;

/// <summary>Timeline unifiée de l'utilisateur : items RSS de ses sources abonnées + événements
/// de ses entreprises favorites (F-044 + F-047), filtrés et paginés, triés chronologiquement.</summary>
public sealed record GetTimelineQuery(
    Guid UserId,
    /// <summary>Curseur keyset opaque renvoyé par la page précédente ; <c>null</c> pour la première page.</summary>
    string? Cursor,
    int PageSize,
    Guid? SourceId,
    DateTimeOffset? After,
    DateTimeOffset? Before,
    string? Keyword,
    bool UnreadOnly,
    bool FavoritesOnly,
    bool IncludeArchived,
    /// <summary>F-047 : ne renvoyer que les items mentionnant au moins une entreprise favorite du user.</summary>
    bool MentionsFavoritesOnly,
    /// <summary>
    /// Veille (doc 12 §6) : ne renvoyer que le contenu éditorial (items RSS), en excluant les
    /// <c>FavoriteEvent</c> (qui appartiennent à l'Accueil). N'affecte pas le filtrage des items RSS.
    /// </summary>
    bool EditorialOnly = false) : IRequest<Result<CursorPage<TimelineItemDto>>>;

/// <summary>F-047 : référence à une entreprise favorite du user mentionnée dans un item.</summary>
public sealed record FavoriteMentionDto(string Siren, string Name);

/// <summary>
/// Item de timeline unifiée (F-044 + F-047 volet 2). <see cref="Kind"/> discrimine entre
/// <c>"RssItem"</c> (item d'un flux RSS) et <c>"FavoriteEvent"</c> (changement RNE / BODACC sur
/// un favori). Les champs non applicables au kind concerné valent <c>null</c> / valeur par défaut.
/// </summary>
public sealed record TimelineItemDto(
    string Kind,
    Guid Id,
    string Title,
    string? Url,
    string? Summary,
    DateTimeOffset OccurredAt,
    bool IsRead,
    bool IsFavorite,
    bool IsArchived,
    // RSS-only (null pour les events)
    Guid? SourceId,
    IReadOnlyList<string> Categories,
    int SourceCount,
    IReadOnlyList<FavoriteMentionDto> MentionedFavorites,
    // FavoriteEvent-only (null pour les RSS items)
    string? EventType,
    string? EventSiren);
