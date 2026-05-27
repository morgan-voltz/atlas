using Atlas.Application.Veille.Unsubscribe;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class UnsubscribeFromFeedSourceHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IVeilleSubscriptionRepository _subscriptionRepo = Substitute.For<IVeilleSubscriptionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private UnsubscribeFromFeedSourceHandler CreateHandler() => new(_subscriptionRepo, _unitOfWork);

    [Fact]
    public async Task Handle_removes_subscription_and_saves()
    {
        var userId = UserId.New();
        VeilleSubscription subscription = VeilleSubscription.Create(userId, FeedSourceId.New(), Now);
        _subscriptionRepo.GetAsync(userId, subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);

        Result result = await CreateHandler().Handle(
            new UnsubscribeFromFeedSourceCommand(userId.Value, subscription.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _subscriptionRepo.Received(1).Remove(subscription);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_not_found_when_subscription_absent()
    {
        _subscriptionRepo.GetAsync(Arg.Any<UserId>(), Arg.Any<VeilleSubscriptionId>(), Arg.Any<CancellationToken>())
            .Returns((VeilleSubscription?)null);

        Result result = await CreateHandler().Handle(
            new UnsubscribeFromFeedSourceCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.subscription_not_found");
        _subscriptionRepo.DidNotReceive().Remove(Arg.Any<VeilleSubscription>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
