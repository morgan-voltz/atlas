using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.FavoriteRefresh;

/// <summary>Déclenche un cycle complet de rafraîchissement des favoris (F-019). Idempotent.</summary>
public sealed record RefreshFavoritesCommand : IRequest<Result<FavoriteRefreshSummary>>;

public sealed record FavoriteRefreshSummary(
    int UsersProcessed,
    int FavoritesProcessed,
    int FavoritesWithChanges,
    int FavoritesFailed);
