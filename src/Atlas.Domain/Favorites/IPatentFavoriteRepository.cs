using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

public interface IPatentFavoriteRepository
{
    Task<PatentFavorite?> GetByUserAndNumberAsync(UserId userId, PublicationNumber publicationNumber, CancellationToken ct = default);

    Task<IReadOnlyList<PatentFavorite>> GetByUserAsync(UserId userId, CancellationToken ct = default);

    Task AddAsync(PatentFavorite favorite, CancellationToken ct = default);

    Task RemoveAsync(PatentFavorite favorite, CancellationToken ct = default);
}
