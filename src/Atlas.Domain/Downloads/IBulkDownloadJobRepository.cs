using Atlas.Domain.Users;

namespace Atlas.Domain.Downloads;

public interface IBulkDownloadJobRepository
{
    Task<BulkDownloadJob?> GetByIdAsync(BulkDownloadJobId id, CancellationToken ct = default);

    Task AddAsync(BulkDownloadJob job, CancellationToken ct = default);

    /// <summary>Mise à jour explicite (status, ArchiveKey, ErrorMessage…).</summary>
    void Update(BulkDownloadJob job);

    Task<IReadOnlyList<BulkDownloadJob>> GetByUserAsync(UserId userId, CancellationToken ct = default);
}
