using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetTimeline;

/// <summary>Timeline unifiée de l'utilisateur : items de ses sources abonnées, filtrés et paginés (F-044).</summary>
public sealed record GetTimelineQuery(
    Guid UserId,
    int Page,
    int PageSize,
    Guid? SourceId,
    DateTimeOffset? After,
    DateTimeOffset? Before,
    string? Keyword,
    bool UnreadOnly,
    bool FavoritesOnly,
    bool IncludeArchived,
    /// <summary>F-047 : ne renvoyer que les items mentionnant au moins une entreprise favorite du user.</summary>
    bool MentionsFavoritesOnly) : IRequest<Result<PagedResult<TimelineItemDto>>>;

/// <summary>F-047 : référence à une entreprise favorite du user mentionnée dans un item.</summary>
public sealed record FavoriteMentionDto(string Siren, string Name);

public sealed record TimelineItemDto(
    Guid Id,
    Guid SourceId,
    string Title,
    string? Url,
    string? Summary,
    DateTimeOffset PublishedAt,
    IReadOnlyList<string> Categories,
    bool IsRead,
    bool IsFavorite,
    bool IsArchived,
    int SourceCount,
    /// <summary>F-047 : entreprises favorites du user mentionnées dans cet item (vide si aucune).</summary>
    IReadOnlyList<FavoriteMentionDto> MentionedFavorites);
