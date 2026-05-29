using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.RemoveTrademarkFavorite;

public sealed record RemoveTrademarkFavoriteCommand(Guid UserId, string DepositNumber) : IRequest<Result>;
