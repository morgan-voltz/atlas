using Atlas.Domain.Companies;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

/// <summary>
/// Port d'accès aux entreprises favorites des utilisateurs (F-017).
/// </summary>
public interface ICompanyFavoriteRepository
{
    Task<CompanyFavorite?> GetByUserAndSirenAsync(UserId userId, Siren siren, CancellationToken ct = default);

    Task<IReadOnlyList<CompanyFavorite>> GetByUserAsync(UserId userId, CancellationToken ct = default);

    Task<int> CountByUserAsync(UserId userId, CancellationToken ct = default);

    Task AddAsync(CompanyFavorite favorite, CancellationToken ct = default);

    Task RemoveAsync(CompanyFavorite favorite, CancellationToken ct = default);
}
