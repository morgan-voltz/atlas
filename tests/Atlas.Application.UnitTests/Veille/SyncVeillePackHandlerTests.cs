using Atlas.Application.Veille;
using Atlas.Application.Veille.ApplyVeillePack;
using Atlas.Application.Veille.SyncVeillePack;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class SyncVeillePackHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IVeillePackRepository _packRepo = Substitute.For<IVeillePackRepository>();
    private readonly IVeillePackEnrollmentRepository _enrollmentRepo = Substitute.For<IVeillePackEnrollmentRepository>();
    private readonly IVeilleSubscriptionRepository _subscriptionRepo = Substitute.For<IVeilleSubscriptionRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public SyncVeillePackHandlerTests() => _clock.UtcNow.Returns(Now);

    private SyncVeillePackHandler CreateHandler() =>
        new(_packRepo, _enrollmentRepo, new VeillePackEnroller(_subscriptionRepo, _clock), _unitOfWork);

    private static VeillePack PackAtVersion(int version, int sources)
    {
        VeillePack pack = VeillePack.Create("cabinet-pi", "Cabinet PI", "Veille PI", Now).Value!;
        pack.SetSources(Enumerable.Range(0, sources).Select(_ => FeedSourceId.New()).ToList());
        while (pack.Version < version)
        {
            pack.BumpVersion();
        }

        return pack;
    }

    [Fact]
    public async Task Handle_returns_not_enrolled_when_no_enrollment()
    {
        VeillePack pack = PackAtVersion(1, 2);
        _packRepo.GetByCodeAsync("cabinet-pi", Arg.Any<CancellationToken>()).Returns(pack);
        _enrollmentRepo.GetAsync(Arg.Any<UserId>(), pack.Id, Arg.Any<CancellationToken>()).Returns((VeillePackEnrollment?)null);

        Result<ApplyVeillePackResult> result =
            await CreateHandler().Handle(new SyncVeillePackCommand(Guid.NewGuid(), "cabinet-pi"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_not_enrolled");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_is_noop_when_up_to_date()
    {
        VeillePack pack = PackAtVersion(1, 3);
        VeillePackEnrollment enrollment = VeillePackEnrollment.Create(UserId.New(), pack.Id, 1, Now);
        _packRepo.GetByCodeAsync("cabinet-pi", Arg.Any<CancellationToken>()).Returns(pack);
        _enrollmentRepo.GetAsync(Arg.Any<UserId>(), pack.Id, Arg.Any<CancellationToken>()).Returns(enrollment);
        _subscriptionRepo.ExistsAsync(Arg.Any<UserId>(), Arg.Any<FeedSourceId>(), Arg.Any<CancellationToken>()).Returns(true);

        Result<ApplyVeillePackResult> result =
            await CreateHandler().Handle(new SyncVeillePackCommand(Guid.NewGuid(), "cabinet-pi"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SubscriptionsAdded.Should().Be(0);
        await _subscriptionRepo.DidNotReceive().AddAsync(Arg.Any<VeilleSubscription>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_subscribes_missing_sources_and_bumps_applied_version()
    {
        VeillePack pack = PackAtVersion(2, 3);
        VeillePackEnrollment enrollment = VeillePackEnrollment.Create(UserId.New(), pack.Id, 1, Now);
        _packRepo.GetByCodeAsync("cabinet-pi", Arg.Any<CancellationToken>()).Returns(pack);
        _enrollmentRepo.GetAsync(Arg.Any<UserId>(), pack.Id, Arg.Any<CancellationToken>()).Returns(enrollment);
        _subscriptionRepo.ExistsAsync(Arg.Any<UserId>(), Arg.Any<FeedSourceId>(), Arg.Any<CancellationToken>()).Returns(false);

        Result<ApplyVeillePackResult> result =
            await CreateHandler().Handle(new SyncVeillePackCommand(Guid.NewGuid(), "cabinet-pi"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SubscriptionsAdded.Should().Be(3);
        result.Value!.Version.Should().Be(2);
        enrollment.AppliedVersion.Should().Be(2);
        _enrollmentRepo.Received(1).Update(enrollment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
