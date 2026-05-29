using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

public interface ITrademarkFavoriteRepository
{
    Task<TrademarkFavorite?> GetByUserAndDepositNumberAsync(UserId userId, DepositNumber depositNumber, CancellationToken ct = default);

    Task<IReadOnlyList<TrademarkFavorite>> GetByUserAsync(UserId userId, CancellationToken ct = default);

    Task AddAsync(TrademarkFavorite favorite, CancellationToken ct = default);

    Task RemoveAsync(TrademarkFavorite favorite, CancellationToken ct = default);
}
