using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.AddTrademarkFavorite;

public sealed record AddTrademarkFavoriteCommand(Guid UserId, string DepositNumber, string? Name)
    : IRequest<Result>;
