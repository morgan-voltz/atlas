using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.GetMyTrademarkFavorites;

internal sealed class GetMyTrademarkFavoritesHandler(ITrademarkFavoriteRepository favorites)
    : IRequestHandler<GetMyTrademarkFavoritesQuery, Result<IReadOnlyList<TrademarkFavoriteDto>>>
{
    public async Task<Result<IReadOnlyList<TrademarkFavoriteDto>>> Handle(
        GetMyTrademarkFavoritesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TrademarkFavorite> items =
            await favorites.GetByUserAsync(new UserId(request.UserId), cancellationToken);

        IReadOnlyList<TrademarkFavoriteDto> dto = items
            .OrderByDescending(f => f.AddedAt)
            .Select(f => new TrademarkFavoriteDto(f.DepositNumber.Value, f.NameSnapshot, f.AddedAt))
            .ToList();

        return Result<IReadOnlyList<TrademarkFavoriteDto>>.Ok(dto);
    }
}
