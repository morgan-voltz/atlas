using Atlas.Application.Veille.GetMyVeillePacks;
using Atlas.Application.Veille.GetVeilleCatalog;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class VeillePackQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IVeillePackRepository _packRepo = Substitute.For<IVeillePackRepository>();
    private readonly IVeillePackEnrollmentRepository _enrollmentRepo = Substitute.For<IVeillePackEnrollmentRepository>();

    private static VeillePack Pack(string code, int sources)
    {
        VeillePack pack = VeillePack.Create(code, code, "desc", Now).Value!;
        pack.SetSources(Enumerable.Range(0, sources).Select(_ => FeedSourceId.New()).ToList());
        return pack;
    }

    [Fact]
    public async Task GetVeilleCatalog_maps_packs_with_source_count()
    {
        _packRepo.GetActiveAsync(Arg.Any<CancellationToken>()).Returns(new[] { Pack("cabinet-pi", 3) });

        Result<IReadOnlyList<VeillePackDto>> result =
            await new GetVeilleCatalogHandler(_packRepo).Handle(new GetVeilleCatalogQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value![0].Code.Should().Be("cabinet-pi");
        result.Value![0].SourceCount.Should().Be(3);
    }

    [Fact]
    public async Task GetMyVeillePacks_flags_update_available_when_pack_version_is_higher()
    {
        var userId = UserId.New();
        VeillePack pack = Pack("cabinet-pi", 2);
        pack.BumpVersion(); // version 2
        VeillePackEnrollment enrollment = VeillePackEnrollment.Create(userId, pack.Id, 1, Now);

        _enrollmentRepo.GetByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(new[] { enrollment });
        _packRepo.GetByIdAsync(pack.Id, Arg.Any<CancellationToken>()).Returns(pack);

        Result<IReadOnlyList<MyVeillePackDto>> result =
            await new GetMyVeillePacksHandler(_enrollmentRepo, _packRepo).Handle(
                new GetMyVeillePacksQuery(userId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        MyVeillePackDto dto = result.Value!.Should().ContainSingle().Subject;
        dto.AppliedVersion.Should().Be(1);
        dto.CurrentVersion.Should().Be(2);
        dto.UpdateAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task GetMyVeillePacks_skips_enrollment_when_pack_missing()
    {
        var userId = UserId.New();
        VeillePackEnrollment enrollment = VeillePackEnrollment.Create(userId, VeillePackId.New(), 1, Now);
        _enrollmentRepo.GetByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(new[] { enrollment });
        _packRepo.GetByIdAsync(Arg.Any<VeillePackId>(), Arg.Any<CancellationToken>()).Returns((VeillePack?)null);

        Result<IReadOnlyList<MyVeillePackDto>> result =
            await new GetMyVeillePacksHandler(_enrollmentRepo, _packRepo).Handle(
                new GetMyVeillePacksQuery(userId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
