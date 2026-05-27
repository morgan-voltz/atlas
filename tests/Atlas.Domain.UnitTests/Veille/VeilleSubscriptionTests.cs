using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class VeilleSubscriptionTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_sets_user_source_and_timestamp()
    {
        var userId = UserId.New();
        var sourceId = FeedSourceId.New();

        VeilleSubscription subscription = VeilleSubscription.Create(userId, sourceId, Now);

        subscription.UserId.Should().Be(userId);
        subscription.SourceId.Should().Be(sourceId);
        subscription.CreatedAt.Should().Be(Now);
        subscription.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_generates_distinct_ids()
    {
        var userId = UserId.New();
        var sourceId = FeedSourceId.New();

        VeilleSubscription first = VeilleSubscription.Create(userId, sourceId, Now);
        VeilleSubscription second = VeilleSubscription.Create(userId, sourceId, Now);

        first.Id.Should().NotBe(second.Id);
    }
}
