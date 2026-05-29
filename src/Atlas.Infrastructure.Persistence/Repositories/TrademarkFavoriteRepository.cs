using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class TrademarkFavoriteRepository(AtlasDbContext dbContext) : ITrademarkFavoriteRepository
{
    public Task<TrademarkFavorite?> GetByUserAndDepositNumberAsync(UserId userId, DepositNumber depositNumber, CancellationToken ct = default) =>
        dbContext.TrademarkFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.DepositNumber == depositNumber, ct);

    public async Task<IReadOnlyList<TrademarkFavorite>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await dbContext.TrademarkFavorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.AddedAt)
            .ToListAsync(ct);

    public async Task AddAsync(TrademarkFavorite favorite, CancellationToken ct = default) =>
        await dbContext.TrademarkFavorites.AddAsync(favorite, ct);

    public Task RemoveAsync(TrademarkFavorite favorite, CancellationToken ct = default)
    {
        dbContext.TrademarkFavorites.Remove(favorite);
        return Task.CompletedTask;
    }
}
