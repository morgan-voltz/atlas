using Atlas.Application.Veille.AddUserFeedSource;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class AddUserFeedSourceHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);
    private const string Url = "https://x.test/feed";

    private readonly IFeedSourceRepository _sourceRepo = Substitute.For<IFeedSourceRepository>();
    private readonly IVeilleSubscriptionRepository _subscriptionRepo = Substitute.For<IVeilleSubscriptionRepository>();
    private readonly IExternalContentSource _provider = Substitute.For<IExternalContentSource>();
    private readonly IFeedSubscriptionPolicy _policy = Substitute.For<IFeedSubscriptionPolicy>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public AddUserFeedSourceHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _policy.IsUrlAllowed(Arg.Any<string>()).Returns(true);
        _policy.MaxSubscriptionsPerUser.Returns((int?)null);
        _policy.UserFeedPollingInterval.Returns(TimeSpan.FromMinutes(30));
        _provider.CanHandle(FeedSourceType.Rss).Returns(true);
        _provider.FetchAsync(Arg.Any<FeedSource>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<FeedItemDraft>>.Ok([]));
    }

    private AddUserFeedSourceHandler CreateHandler() =>
        new(_sourceRepo, _subscriptionRepo, [_provider], _policy, _clock, _unitOfWork);

    private static AddUserFeedSourceCommand Command(string? name = null) =>
        new(Guid.NewGuid(), Url, name);

    [Fact]
    public async Task Handle_creates_source_and_subscription_for_new_valid_feed()
    {
        _sourceRepo.GetByUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((FeedSource?)null);

        Result<Application.Veille.GetMySubscriptions.VeilleSubscriptionDto> result =
            await CreateHandler().Handle(Command("Mon flux"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SourceName.Should().Be("Mon flux");
        await _sourceRepo.Received(1).AddAsync(Arg.Any<FeedSource>(), Arg.Any<CancellationToken>());
        await _subscriptionRepo.Received(1).AddAsync(Arg.Any<VeilleSubscription>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_derives_name_from_host_when_name_omitted()
    {
        _sourceRepo.GetByUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((FeedSource?)null);

        Result<Application.Veille.GetMySubscriptions.VeilleSubscriptionDto> result =
            await CreateHandler().Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SourceName.Should().Be("x.test");
    }

    [Fact]
    public async Task Handle_returns_source_blocked_when_url_not_allowed()
    {
        _policy.IsUrlAllowed(Url).Returns(false);

        Result<Application.Veille.GetMySubscriptions.VeilleSubscriptionDto> result =
            await CreateHandler().Handle(Command(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.source_blocked");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_limit_reached_when_count_at_max()
    {
        _policy.MaxSubscriptionsPerUser.Returns(2);
        _subscriptionRepo.CountByUserAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(2);

        Result<Application.Veille.GetMySubscriptions.VeilleSubscriptionDto> result =
            await CreateHandler().Handle(Command(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.subscription_limit_reached");
        await _sourceRepo.DidNotReceive().AddAsync(Arg.Any<FeedSource>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_feed_unreachable_when_fetch_fails()
    {
        _sourceRepo.GetByUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((FeedSource?)null);
        _provider.FetchAsync(Arg.Any<FeedSource>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<FeedItemDraft>>.Fail(VeilleErrors.FetchFailed));

        Result<Application.Veille.GetMySubscriptions.VeilleSubscriptionDto> result =
            await CreateHandler().Handle(Command(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.feed_unreachable");
        await _sourceRepo.DidNotReceive().AddAsync(Arg.Any<FeedSource>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_reuses_existing_source_without_refetching()
    {
        FeedSource existing = FeedSource.Create("Existant", Url, FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now).Value!;
        _sourceRepo.GetByUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(existing);
        _subscriptionRepo.ExistsAsync(Arg.Any<UserId>(), existing.Id, Arg.Any<CancellationToken>()).Returns(false);

        Result<Application.Veille.GetMySubscriptions.VeilleSubscriptionDto> result =
            await CreateHandler().Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SourceId.Should().Be(existing.Id.Value);
        await _sourceRepo.DidNotReceive().AddAsync(Arg.Any<FeedSource>(), Arg.Any<CancellationToken>());
        await _provider.DidNotReceive().FetchAsync(Arg.Any<FeedSource>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
        await _subscriptionRepo.Received(1).AddAsync(Arg.Any<VeilleSubscription>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_already_subscribed_when_subscription_exists()
    {
        FeedSource existing = FeedSource.Create("Existant", Url, FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now).Value!;
        _sourceRepo.GetByUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(existing);
        _subscriptionRepo.ExistsAsync(Arg.Any<UserId>(), existing.Id, Arg.Any<CancellationToken>()).Returns(true);

        Result<Application.Veille.GetMySubscriptions.VeilleSubscriptionDto> result =
            await CreateHandler().Handle(Command(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.already_subscribed");
        await _subscriptionRepo.DidNotReceive().AddAsync(Arg.Any<VeilleSubscription>(), Arg.Any<CancellationToken>());
    }
}
