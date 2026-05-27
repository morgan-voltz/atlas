using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.SetFeedItemState;

/// <summary>Met à jour l'état d'un item de veille pour l'utilisateur (lu/favori/archivé). Drapeaux null = inchangés (F-044).</summary>
public sealed record SetFeedItemStateCommand(
    Guid UserId,
    Guid FeedItemId,
    bool? IsRead,
    bool? IsFavorite,
    bool? IsArchived) : IRequest<Result<FeedItemStateDto>>;

public sealed record FeedItemStateDto(Guid FeedItemId, bool IsRead, bool IsFavorite, bool IsArchived);
