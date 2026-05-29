using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.GetMyTrademarkFavorites;

public sealed record GetMyTrademarkFavoritesQuery(Guid UserId)
    : IRequest<Result<IReadOnlyList<TrademarkFavoriteDto>>>;
