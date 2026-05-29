using Atlas.Application.Veille.Marketplace;
using Atlas.Application.Veille.Marketplace.CreateUserVeillePack;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille.Marketplace;

public sealed class CreateUserVeillePackHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IVeillePackRepository _packs = Substitute.For<IVeillePackRepository>();
    private readonly IVeilleSubscriptionRepository _subs = Substitute.For<IVeilleSubscriptionRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public CreateUserVeillePackHandlerTests() => _clock.UtcNow.Returns(Now);

    private CreateUserVeillePackHandler CreateHandler() =>
        new(_packs, _subs, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_creates_private_pack_when_subscriptions_owned()
    {
        Guid userId = Guid.NewGuid();
        FeedSourceId src1 = FeedSourceId.New();
        FeedSourceId src2 = FeedSourceId.New();
        VeilleSubscription sub1 = VeilleSubscription.Create(new UserId(userId), src1, Now);
        VeilleSubscription sub2 = VeilleSubscription.Create(new UserId(userId), src2, Now);
        _subs.GetByUserAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns([sub1, sub2]);
        _packs.ExistsByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateUserVeillePackCommand(
            userId, "ma-veille", "Ma veille", "Mon pack curé",
            [sub1.Id.Value, sub2.Id.Value]);

        Result<VeillePackMarketplaceDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("ma-veille");
        result.Value.Visibility.Should().Be("Private");
        result.Value.AuthorUserId.Should().Be(userId);
        result.Value.SourceCount.Should().Be(2);
        await _packs.Received(1).AddAsync(Arg.Any<VeillePack>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_code_already_used_when_collision()
    {
        _packs.ExistsByCodeAsync("colliding", Arg.Any<CancellationToken>()).Returns(true);

        Result<VeillePackMarketplaceDto> result = await CreateHandler().Handle(
            new CreateUserVeillePackCommand(Guid.NewGuid(), "colliding", "Name", null!, [Guid.NewGuid()]),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_code_already_used");
        await _packs.DidNotReceive().AddAsync(Arg.Any<VeillePack>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_filters_subscriptions_not_owned_by_caller()
    {
        Guid userId = Guid.NewGuid();
        Guid otherUser = Guid.NewGuid();
        VeilleSubscription mine = VeilleSubscription.Create(new UserId(userId), FeedSourceId.New(), Now);
        _subs.GetByUserAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns([mine]);
        _packs.ExistsByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        // L'attaquant tente d'inclure un sub ID qui ne lui appartient pas → silencieusement ignoré.
        Guid bogusId = Guid.NewGuid();
        var command = new CreateUserVeillePackCommand(
            userId, "p", "P", string.Empty, [bogusId, mine.Id.Value]);

        Result<VeillePackMarketplaceDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SourceCount.Should().Be(1, "seul l'abonnement réel du caller est inclus");
    }

    [Fact]
    public async Task Handle_returns_invalid_when_no_sources_resolved()
    {
        _packs.ExistsByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _subs.GetByUserAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns([]);

        Result<VeillePackMarketplaceDto> result = await CreateHandler().Handle(
            new CreateUserVeillePackCommand(Guid.NewGuid(), "p", "P", null!, [Guid.NewGuid()]),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.invalid_veille_pack");
    }
}
