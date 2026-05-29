using Atlas.Application.Veille.Rules;
using Atlas.Application.Veille.Rules.CreateFeedRule;
using Atlas.Domain.Common;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille.Rules;

public sealed class CreateFeedRuleHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IFeedRuleRepository _rules = Substitute.For<IFeedRuleRepository>();
    private readonly IFeedSourceRepository _sources = Substitute.For<IFeedSourceRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public CreateFeedRuleHandlerTests() => _clock.UtcNow.Returns(Now);

    private CreateFeedRuleHandler CreateHandler() => new(_rules, _sources, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_with_keyword_only_persists_rule()
    {
        var command = new CreateFeedRuleCommand(
            Guid.NewGuid(), "Mention RGPD", "RGPD", null, null, true, true);

        Result<FeedRuleDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.KeywordPattern.Should().Be("RGPD");
        await _rules.Received(1).AddAsync(Arg.Any<FeedRule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_invalid_when_no_criteria()
    {
        var command = new CreateFeedRuleCommand(
            Guid.NewGuid(), "Sans critère", null, null, null, true, false);

        Result<FeedRuleDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.invalid_feed_rule");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_invalid_when_source_unknown()
    {
        Guid unknownSourceId = Guid.NewGuid();
        _sources.GetByIdsAsync(Arg.Any<IReadOnlyCollection<FeedSourceId>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new CreateFeedRuleCommand(
            Guid.NewGuid(), "Source inconnue", null, unknownSourceId, null, true, false);

        Result<FeedRuleDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.invalid_feed_rule");
        await _rules.DidNotReceive().AddAsync(Arg.Any<FeedRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_invalid_when_mentioned_siren_is_invalid()
    {
        var command = new CreateFeedRuleCommand(
            Guid.NewGuid(), "SIREN invalide", null, null, "123", true, false);

        Result<FeedRuleDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("companies.invalid_siren");
        await _rules.DidNotReceive().AddAsync(Arg.Any<FeedRule>(), Arg.Any<CancellationToken>());
    }
}
