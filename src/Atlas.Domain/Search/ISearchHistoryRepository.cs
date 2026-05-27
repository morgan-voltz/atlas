using Atlas.Domain.Users;

namespace Atlas.Domain.Search;

public interface ISearchHistoryRepository
{
    Task AddAsync(SearchHistoryEntry entry, CancellationToken ct = default);

    Task<IReadOnlyList<SearchHistoryEntry>> GetRecentByUserAsync(UserId userId, int limit, CancellationToken ct = default);

    /// <summary>Conserve les <paramref name="maxEntries"/> entrées les plus récentes de l'utilisateur, supprime le reste.</summary>
    Task PruneAsync(UserId userId, int maxEntries, CancellationToken ct = default);
}
