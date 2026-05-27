using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class FeedSourceRepository(AtlasDbContext dbContext) : IFeedSourceRepository
{
    public async Task<IReadOnlyList<FeedSource>> GetActiveAsync(CancellationToken ct = default) =>
        await dbContext.FeedSources
            .Where(source => source.IsActive)
            .ToListAsync(ct);

    public async Task<bool> ExistsByUrlAsync(string url, CancellationToken ct = default) =>
        await dbContext.FeedSources.AnyAsync(source => source.Url == url, ct);

    public async Task<FeedSource?> GetByUrlAsync(string url, CancellationToken ct = default) =>
        await dbContext.FeedSources.FirstOrDefaultAsync(source => source.Url == url, ct);

    public async Task<IReadOnlyList<FeedSource>> GetByIdsAsync(
        IReadOnlyCollection<FeedSourceId> ids,
        CancellationToken ct = default) =>
        await dbContext.FeedSources
            .Where(source => ids.Contains(source.Id))
            .ToListAsync(ct);

    public async Task AddAsync(FeedSource source, CancellationToken ct = default) =>
        await dbContext.FeedSources.AddAsync(source, ct);

    public void Update(FeedSource source) => dbContext.FeedSources.Update(source);
}
