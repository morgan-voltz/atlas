using Atlas.Application.Veille.Rules.DeleteFeedRule;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille.Rules;

public sealed class DeleteFeedRuleHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IFeedRuleRepository _rules = Substitute.For<IFeedRuleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private DeleteFeedRuleHandler CreateHandler() => new(_rules, _unitOfWork);

    [Fact]
    public async Task Handle_owner_deletes_successfully()
    {
        var userId = new UserId(Guid.NewGuid());
        FeedRule rule = FeedRule.Create(userId, "ma règle", "X", null, null, true, false, Now).Value!;
        _rules.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        Result result = await CreateHandler().Handle(
            new DeleteFeedRuleCommand(userId.Value, rule.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _rules.Received(1).RemoveAsync(rule, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_not_found_when_missing()
    {
        _rules.GetByIdAsync(Arg.Any<FeedRuleId>(), Arg.Any<CancellationToken>())
            .Returns((FeedRule?)null);

        Result result = await CreateHandler().Handle(
            new DeleteFeedRuleCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.feed_rule_not_found");
    }

    [Fact]
    public async Task Handle_returns_forbidden_when_caller_is_not_owner()
    {
        var ownerId = new UserId(Guid.NewGuid());
        FeedRule rule = FeedRule.Create(ownerId, "Privée", "X", null, null, true, false, Now).Value!;
        _rules.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        Result result = await CreateHandler().Handle(
            new DeleteFeedRuleCommand(Guid.NewGuid(), rule.Id.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.feed_rule_forbidden");
        await _rules.DidNotReceive().RemoveAsync(Arg.Any<FeedRule>(), Arg.Any<CancellationToken>());
    }
}
