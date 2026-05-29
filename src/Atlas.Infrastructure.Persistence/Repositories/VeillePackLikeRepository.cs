using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class VeillePackLikeRepository(AtlasDbContext dbContext) : IVeillePackLikeRepository
{
    public Task<bool> ExistsAsync(UserId userId, VeillePackId veillePackId, CancellationToken ct = default) =>
        dbContext.VeillePackLikes.AnyAsync(
            like => like.UserId == userId && like.VeillePackId == veillePackId,
            ct);

    public Task<VeillePackLike?> GetAsync(UserId userId, VeillePackId veillePackId, CancellationToken ct = default) =>
        dbContext.VeillePackLikes.FirstOrDefaultAsync(
            like => like.UserId == userId && like.VeillePackId == veillePackId,
            ct);

    public async Task AddAsync(VeillePackLike like, CancellationToken ct = default) =>
        await dbContext.VeillePackLikes.AddAsync(like, ct);

    public Task RemoveAsync(VeillePackLike like, CancellationToken ct = default)
    {
        dbContext.VeillePackLikes.Remove(like);
        return Task.CompletedTask;
    }
}
