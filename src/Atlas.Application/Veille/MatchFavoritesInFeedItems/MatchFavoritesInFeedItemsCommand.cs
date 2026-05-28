using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.MatchFavoritesInFeedItems;

/// <summary>
/// Calcule les mentions des entreprises favorites dans les FeedItems récents (F-047).
/// Exécuté en fin de polling, après le clustering. Idempotent.
/// </summary>
public sealed record MatchFavoritesInFeedItemsCommand(int LookbackDays = 30)
    : IRequest<Result<FavoriteMatchSummary>>;

public sealed record FavoriteMatchSummary(int UsersScanned, int ItemsScanned, int MatchesCreated);
