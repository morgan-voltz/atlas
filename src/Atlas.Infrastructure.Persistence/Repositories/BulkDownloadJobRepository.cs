using Atlas.Domain.Downloads;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class BulkDownloadJobRepository(AtlasDbContext dbContext) : IBulkDownloadJobRepository
{
    public Task<BulkDownloadJob?> GetByIdAsync(BulkDownloadJobId id, CancellationToken ct = default) =>
        dbContext.BulkDownloadJobs.FirstOrDefaultAsync(job => job.Id == id, ct);

    public async Task AddAsync(BulkDownloadJob job, CancellationToken ct = default) =>
        await dbContext.BulkDownloadJobs.AddAsync(job, ct);

    public void Update(BulkDownloadJob job) =>
        dbContext.BulkDownloadJobs.Update(job);

    public async Task<IReadOnlyList<BulkDownloadJob>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await dbContext.BulkDownloadJobs
            .Where(job => job.UserId == userId)
            .OrderByDescending(job => job.RequestedAt)
            .ToListAsync(ct);
}
