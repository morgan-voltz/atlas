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
    bool IncludeArchived) : IRequest<Result<PagedResult<TimelineItemDto>>>;

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
    bool IsArchived);
