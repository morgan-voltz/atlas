using Atlas.Application.Veille.Rules.EvaluateFeedRules;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille.Rules;

public sealed class EvaluateFeedRulesHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 14, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Earlier = Now.AddHours(-1);

    private readonly IFeedRuleRepository _rules = Substitute.For<IFeedRuleRepository>();
    private readonly IFeedItemRepository _items = Substitute.For<IFeedItemRepository>();
    private readonly IFeedSourceRepository _sources = Substitute.For<IFeedSourceRepository>();
    private readonly IFeedItemFavoriteMatchRepository _favoriteMatches = Substitute.For<IFeedItemFavoriteMatchRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    public EvaluateFeedRulesHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _sources.GetActiveAsync(Arg.Any<CancellationToken>()).Returns([]);
    }

    private EvaluateFeedRulesHandler CreateHandler() => new(
        _rules, _items, _sources, _favoriteMatches, _users, _clock, _unitOfWork, _publisher,
        NullLogger<EvaluateFeedRulesHandler>.Instance);

    private static User CreateUser(UserId userId)
    {
        var email = EmailAddress.Create("user@example.test").Value!;
        var hash = PasswordHash.FromHash("argon2id$dummy");
        return User.Register(userId, email, hash, "tokenhash", Earlier, TimeSpan.FromHours(24));
    }

    [Fact]
    public async Task Handle_returns_empty_summary_when_no_rules()
    {
        _rules.ListActiveAsync(Arg.Any<CancellationToken>()).Returns([]);

        Result<FeedRuleEvaluationSummary> result = await CreateHandler().Handle(
            new EvaluateFeedRulesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RulesProcessed.Should().Be(0);
        result.Value!.RulesTriggered.Should().Be(0);
    }

    [Fact]
    public async Task Handle_publishes_notification_for_matched_rule()
    {
        var userId = new UserId(Guid.NewGuid());
        User user = CreateUser(userId);
        FeedSource source = FeedSource.Create("BlogX", "https://x.test/feed", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Earlier).Value!;
        FeedRule rule = FeedRule.Create(userId, "Mention RGPD", "RGPD", null, null, true, false, Earlier).Value!;

        // Item ingéré après le watermark de la règle, qui contient le mot-clé.
        FeedItem item = FeedItem.Create(source.Id, "Nouveau RGPD 2026", "https://x.test/a", null, Now, null, Now);

        _rules.ListActiveAsync(Arg.Any<CancellationToken>()).Returns([rule]);
        _sources.GetActiveAsync(Arg.Any<CancellationToken>()).Returns([source]);
        _items.ListFetchedSinceAsync(Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([item]);
        _users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        Result<FeedRuleEvaluationSummary> result = await CreateHandler().Handle(
            new EvaluateFeedRulesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RulesProcessed.Should().Be(1);
        result.Value!.RulesTriggered.Should().Be(1);
        result.Value!.MatchesTotal.Should().Be(1);
        rule.TimesTriggered.Should().Be(1);
        rule.LastTriggeredAt.Should().Be(Now);
        rule.LastEvaluatedAt.Should().Be(Now);
        await _publisher.Received(1).Publish(Arg.Any<FeedRuleMatchedNotification>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_skips_publish_when_no_match_but_registers_evaluation()
    {
        var userId = new UserId(Guid.NewGuid());
        User user = CreateUser(userId);
        FeedSource source = FeedSource.Create("BlogX", "https://x.test/feed", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Earlier).Value!;
        FeedRule rule = FeedRule.Create(userId, "Mention CSRD", "CSRD", null, null, true, false, Earlier).Value!;
        FeedItem item = FeedItem.Create(source.Id, "Autre sujet", "https://x.test/a", null, Now, null, Now);

        _rules.ListActiveAsync(Arg.Any<CancellationToken>()).Returns([rule]);
        _sources.GetActiveAsync(Arg.Any<CancellationToken>()).Returns([source]);
        _items.ListFetchedSinceAsync(Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([item]);
        _users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        Result<FeedRuleEvaluationSummary> result = await CreateHandler().Handle(
            new EvaluateFeedRulesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RulesProcessed.Should().Be(1);
        result.Value!.RulesTriggered.Should().Be(0);
        rule.TimesTriggered.Should().Be(0);
        rule.LastTriggeredAt.Should().BeNull();
        rule.LastEvaluatedAt.Should().Be(Now);
        await _publisher.DidNotReceive().Publish(Arg.Any<FeedRuleMatchedNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_does_not_re_evaluate_items_already_fetched_before_watermark()
    {
        var userId = new UserId(Guid.NewGuid());
        User user = CreateUser(userId);
        FeedSource source = FeedSource.Create("BlogX", "https://x.test/feed", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Earlier).Value!;
        FeedRule rule = FeedRule.Create(userId, "Mention RGPD", "RGPD", null, null, true, false, Earlier).Value!;
        rule.RegisterEvaluation(Now.AddMinutes(-5));

        // L'item a été fetched AVANT le watermark de la règle → ignoré.
        FeedItem oldItem = FeedItem.Create(source.Id, "RGPD 2025", "https://x.test/old", null, Earlier, null, Now.AddMinutes(-10));

        _rules.ListActiveAsync(Arg.Any<CancellationToken>()).Returns([rule]);
        _sources.GetActiveAsync(Arg.Any<CancellationToken>()).Returns([source]);
        _items.ListFetchedSinceAsync(Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([oldItem]);
        _users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        Result<FeedRuleEvaluationSummary> result = await CreateHandler().Handle(
            new EvaluateFeedRulesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RulesTriggered.Should().Be(0);
        await _publisher.DidNotReceive().Publish(Arg.Any<FeedRuleMatchedNotification>(), Arg.Any<CancellationToken>());
    }
}
