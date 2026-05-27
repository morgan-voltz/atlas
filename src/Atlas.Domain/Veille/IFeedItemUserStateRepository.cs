using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

public interface IFeedItemUserStateRepository
{
    Task<FeedItemUserState?> GetAsync(UserId userId, FeedItemId feedItemId, CancellationToken ct = default);

    Task AddAsync(FeedItemUserState state, CancellationToken ct = default);

    void Update(FeedItemUserState state);
}
