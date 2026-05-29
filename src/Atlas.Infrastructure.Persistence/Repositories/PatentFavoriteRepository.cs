using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class PatentFavoriteRepository(AtlasDbContext dbContext) : IPatentFavoriteRepository
{
    public Task<PatentFavorite?> GetByUserAndNumberAsync(UserId userId, PublicationNumber publicationNumber, CancellationToken ct = default) =>
        dbContext.PatentFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PublicationNumber == publicationNumber, ct);

    public async Task<IReadOnlyList<PatentFavorite>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await dbContext.PatentFavorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.AddedAt)
            .ToListAsync(ct);

    public async Task AddAsync(PatentFavorite favorite, CancellationToken ct = default) =>
        await dbContext.PatentFavorites.AddAsync(favorite, ct);

    public Task RemoveAsync(PatentFavorite favorite, CancellationToken ct = default)
    {
        dbContext.PatentFavorites.Remove(favorite);
        return Task.CompletedTask;
    }
}
