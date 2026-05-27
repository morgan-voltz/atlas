using Atlas.Application.Veille.GetMySubscriptions;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class GetMySubscriptionsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IVeilleSubscriptionRepository _subscriptionRepo = Substitute.For<IVeilleSubscriptionRepository>();
    private readonly IFeedSourceRepository _sourceRepo = Substitute.For<IFeedSourceRepository>();

    private GetMySubscriptionsHandler CreateHandler() => new(_subscriptionRepo, _sourceRepo);

    [Fact]
    public async Task Handle_returns_empty_when_user_has_no_subscriptions()
    {
        _subscriptionRepo.GetByUserAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<VeilleSubscription>());

        Result<IReadOnlyList<VeilleSubscriptionDto>> result =
            await CreateHandler().Handle(new GetMySubscriptionsQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        await _sourceRepo.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<FeedSourceId>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_joins_subscriptions_with_their_sources()
    {
        var userId = UserId.New();
        FeedSource source = FeedSource.Create("Flux", "https://x.test/feed", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now).Value!;
        VeilleSubscription subscription = VeilleSubscription.Create(userId, source.Id, Now);

        _subscriptionRepo.GetByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(new[] { subscription });
        _sourceRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<FeedSourceId>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { source });

        Result<IReadOnlyList<VeilleSubscriptionDto>> result =
            await CreateHandler().Handle(new GetMySubscriptionsQuery(userId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        VeilleSubscriptionDto dto = result.Value![0];
        dto.SubscriptionId.Should().Be(subscription.Id.Value);
        dto.SourceId.Should().Be(source.Id.Value);
        dto.SourceName.Should().Be("Flux");
        dto.SourceUrl.Should().Be(source.Url);
    }

    [Fact]
    public async Task Handle_skips_subscriptions_whose_source_is_missing()
    {
        var userId = UserId.New();
        VeilleSubscription orphan = VeilleSubscription.Create(userId, FeedSourceId.New(), Now);

        _subscriptionRepo.GetByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(new[] { orphan });
        _sourceRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<FeedSourceId>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<FeedSource>());

        Result<IReadOnlyList<VeilleSubscriptionDto>> result =
            await CreateHandler().Handle(new GetMySubscriptionsQuery(userId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
