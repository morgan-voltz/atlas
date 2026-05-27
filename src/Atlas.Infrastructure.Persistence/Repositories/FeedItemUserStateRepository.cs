using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class FeedItemUserStateRepository(AtlasDbContext dbContext) : IFeedItemUserStateRepository
{
    public async Task<FeedItemUserState?> GetAsync(UserId userId, FeedItemId feedItemId, CancellationToken ct = default) =>
        await dbContext.FeedItemUserStates
            .FirstOrDefaultAsync(state => state.UserId == userId && state.FeedItemId == feedItemId, ct);

    public async Task AddAsync(FeedItemUserState state, CancellationToken ct = default) =>
        await dbContext.FeedItemUserStates.AddAsync(state, ct);

    public void Update(FeedItemUserState state) => dbContext.FeedItemUserStates.Update(state);
}
