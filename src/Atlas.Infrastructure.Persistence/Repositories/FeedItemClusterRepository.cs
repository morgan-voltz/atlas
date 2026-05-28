using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class FeedItemClusterRepository(AtlasDbContext dbContext) : IFeedItemClusterRepository
{
    public async Task<IReadOnlyList<FeedItemCluster>> GetActiveSinceAsync(
        DateTimeOffset since,
        CancellationToken ct = default) =>
        await dbContext.FeedItemClusters
            .Where(cluster => cluster.LastPublishedAt >= since)
            .ToListAsync(ct);

    public async Task AddAsync(FeedItemCluster cluster, CancellationToken ct = default) =>
        await dbContext.FeedItemClusters.AddAsync(cluster, ct);

    public void Update(FeedItemCluster cluster) => dbContext.FeedItemClusters.Update(cluster);
}
