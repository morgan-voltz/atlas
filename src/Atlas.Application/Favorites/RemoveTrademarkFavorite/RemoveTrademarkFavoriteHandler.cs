using Atlas.Domain.Common;
using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.RemoveTrademarkFavorite;

internal sealed class RemoveTrademarkFavoriteHandler(
    ITrademarkFavoriteRepository favorites,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveTrademarkFavoriteCommand, Result>
{
    public async Task<Result> Handle(RemoveTrademarkFavoriteCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DepositNumber))
        {
            return Result.Fail(TrademarkFavoriteErrors.InvalidDepositNumber(request.DepositNumber ?? string.Empty));
        }

        var depositNumber = new DepositNumber(request.DepositNumber.Trim());
        var userId = new UserId(request.UserId);

        TrademarkFavorite? favorite = await favorites.GetByUserAndDepositNumberAsync(userId, depositNumber, cancellationToken);
        if (favorite is null)
        {
            return Result.Fail(TrademarkFavoriteErrors.NotFavorite(depositNumber.Value));
        }

        await favorites.RemoveAsync(favorite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
