using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class CompanyFavoriteRepository(AtlasDbContext dbContext) : ICompanyFavoriteRepository
{
    public Task<CompanyFavorite?> GetByUserAndSirenAsync(UserId userId, Siren siren, CancellationToken ct = default) =>
        dbContext.CompanyFavorites
            .FirstOrDefaultAsync(favorite => favorite.UserId == userId && favorite.Siren == siren, ct);

    public async Task<IReadOnlyList<CompanyFavorite>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await dbContext.CompanyFavorites
            .Where(favorite => favorite.UserId == userId)
            .OrderByDescending(favorite => favorite.AddedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CompanyFavorite>> GetAllAsync(CancellationToken ct = default) =>
        await dbContext.CompanyFavorites.ToListAsync(ct);

    public Task<int> CountByUserAsync(UserId userId, CancellationToken ct = default) =>
        dbContext.CompanyFavorites.CountAsync(favorite => favorite.UserId == userId, ct);

    public async Task AddAsync(CompanyFavorite favorite, CancellationToken ct = default) =>
        await dbContext.CompanyFavorites.AddAsync(favorite, ct);

    public Task RemoveAsync(CompanyFavorite favorite, CancellationToken ct = default)
    {
        dbContext.CompanyFavorites.Remove(favorite);
        return Task.CompletedTask;
    }
}
