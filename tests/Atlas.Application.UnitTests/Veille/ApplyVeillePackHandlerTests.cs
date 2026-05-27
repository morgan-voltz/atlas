using Atlas.Application.Veille;
using Atlas.Application.Veille.ApplyVeillePack;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class ApplyVeillePackHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IVeillePackRepository _packRepo = Substitute.For<IVeillePackRepository>();
    private readonly IVeillePackEnrollmentRepository _enrollmentRepo = Substitute.For<IVeillePackEnrollmentRepository>();
    private readonly IVeilleSubscriptionRepository _subscriptionRepo = Substitute.For<IVeilleSubscriptionRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public ApplyVeillePackHandlerTests() => _clock.UtcNow.Returns(Now);

    private ApplyVeillePackHandler CreateHandler() =>
        new(_packRepo, _enrollmentRepo, new VeillePackEnroller(_subscriptionRepo, _clock), _clock, _unitOfWork);

    private static VeillePack PackWithSources(int count, out IReadOnlyList<FeedSourceId> ids)
    {
        VeillePack pack = VeillePack.Create("cabinet-pi", "Cabinet PI", "Veille PI", Now).Value!;
        var list = Enumerable.Range(0, count).Select(_ => FeedSourceId.New()).ToList();
        pack.SetSources(list);
        ids = list;
        return pack;
    }

    [Fact]
    public async Task Handle_returns_not_found_for_unknown_pack()
    {
        _packRepo.GetByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((VeillePack?)null);

        Result<ApplyVeillePackResult> result =
            await CreateHandler().Handle(new ApplyVeillePackCommand(Guid.NewGuid(), "inconnu"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_not_found");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_creates_enrollment_and_subscribes_to_all_sources()
    {
        // 5 sources : prouve l'absence de plafond (la limite F-043 n'est jamais consultée pour un pack).
        VeillePack pack = PackWithSources(5, out _);
        _packRepo.GetByCodeAsync("cabinet-pi", Arg.Any<CancellationToken>()).Returns(pack);
        _enrollmentRepo.GetAsync(Arg.Any<UserId>(), pack.Id, Arg.Any<CancellationToken>()).Returns((VeillePackEnrollment?)null);
        _subscriptionRepo.ExistsAsync(Arg.Any<UserId>(), Arg.Any<FeedSourceId>(), Arg.Any<CancellationToken>()).Returns(false);

        Result<ApplyVeillePackResult> result =
            await CreateHandler().Handle(new ApplyVeillePackCommand(Guid.NewGuid(), "CABINET-PI"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SubscriptionsAdded.Should().Be(5);
        result.Value!.Code.Should().Be("cabinet-pi");
        await _subscriptionRepo.Received(5).AddAsync(Arg.Any<VeilleSubscription>(), Arg.Any<CancellationToken>());
        await _enrollmentRepo.Received(1).AddAsync(Arg.Any<VeillePackEnrollment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_is_idempotent_when_already_subscribed()
    {
        VeillePack pack = PackWithSources(3, out _);
        VeillePackEnrollment enrollment = VeillePackEnrollment.Create(UserId.New(), pack.Id, pack.Version, Now);
        _packRepo.GetByCodeAsync("cabinet-pi", Arg.Any<CancellationToken>()).Returns(pack);
        _enrollmentRepo.GetAsync(Arg.Any<UserId>(), pack.Id, Arg.Any<CancellationToken>()).Returns(enrollment);
        _subscriptionRepo.ExistsAsync(Arg.Any<UserId>(), Arg.Any<FeedSourceId>(), Arg.Any<CancellationToken>()).Returns(true);

        Result<ApplyVeillePackResult> result =
            await CreateHandler().Handle(new ApplyVeillePackCommand(Guid.NewGuid(), "cabinet-pi"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SubscriptionsAdded.Should().Be(0);
        await _subscriptionRepo.DidNotReceive().AddAsync(Arg.Any<VeilleSubscription>(), Arg.Any<CancellationToken>());
        _enrollmentRepo.Received(1).Update(enrollment);
    }
}
