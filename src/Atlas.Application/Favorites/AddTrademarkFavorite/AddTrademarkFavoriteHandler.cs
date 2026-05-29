using Atlas.Domain.Common;
using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.AddTrademarkFavorite;

internal sealed class AddTrademarkFavoriteHandler(
    ITrademarkFavoriteRepository favorites,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<AddTrademarkFavoriteCommand, Result>
{
    public async Task<Result> Handle(AddTrademarkFavoriteCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DepositNumber))
        {
            return Result.Fail(TrademarkFavoriteErrors.InvalidDepositNumber(request.DepositNumber ?? string.Empty));
        }

        var depositNumber = new DepositNumber(request.DepositNumber.Trim());
        var userId = new UserId(request.UserId);

        TrademarkFavorite? existing = await favorites.GetByUserAndDepositNumberAsync(userId, depositNumber, cancellationToken);
        if (existing is not null)
        {
            return Result.Fail(TrademarkFavoriteErrors.AlreadyFavorite(depositNumber.Value));
        }

        var favorite = TrademarkFavorite.Mark(userId, depositNumber, request.Name, clock.UtcNow);
        await favorites.AddAsync(favorite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
