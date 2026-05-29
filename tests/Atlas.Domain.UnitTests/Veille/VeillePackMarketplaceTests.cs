using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class VeillePackMarketplaceTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);
    private static readonly UserId Author = new(Guid.NewGuid());

    [Fact]
    public void System_pack_has_System_visibility_and_no_author()
    {
        VeillePack pack = VeillePack.Create("cabinet-pi", "Cabinet PI", "desc", Now).Value!;

        pack.Visibility.Should().Be(VeillePackVisibility.System);
        pack.AuthorUserId.Should().BeNull();
        pack.IsSystemPack.Should().BeTrue();
        pack.IsUserPack.Should().BeFalse();
        pack.LikesCount.Should().Be(0);
    }

    [Fact]
    public void User_pack_starts_in_Private_with_author_set()
    {
        VeillePack pack = VeillePack.CreateUserPack(Author, "ma-veille", "Ma veille", "Mon pack", Now).Value!;

        pack.Visibility.Should().Be(VeillePackVisibility.Private);
        pack.AuthorUserId.Should().Be(Author);
        pack.IsUserPack.Should().BeTrue();
        pack.IsSystemPack.Should().BeFalse();
    }

    [Fact]
    public void Publish_flips_Private_to_Public_on_user_pack()
    {
        VeillePack pack = VeillePack.CreateUserPack(Author, "p", "P", string.Empty, Now).Value!;

        Result published = pack.Publish();

        published.IsSuccess.Should().BeTrue();
        pack.Visibility.Should().Be(VeillePackVisibility.Public);
    }

    [Fact]
    public void Publish_is_idempotent_when_already_Public()
    {
        VeillePack pack = VeillePack.CreateUserPack(Author, "p", "P", string.Empty, Now).Value!;
        pack.Publish();

        Result second = pack.Publish();

        second.IsSuccess.Should().BeTrue();
        pack.Visibility.Should().Be(VeillePackVisibility.Public);
    }

    [Fact]
    public void Publish_fails_on_system_pack()
    {
        VeillePack pack = VeillePack.Create("cabinet-pi", "Cabinet PI", "desc", Now).Value!;

        Result result = pack.Publish();

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_immutable");
        pack.Visibility.Should().Be(VeillePackVisibility.System);
    }

    [Fact]
    public void Unpublish_flips_Public_to_Private()
    {
        VeillePack pack = VeillePack.CreateUserPack(Author, "p", "P", string.Empty, Now).Value!;
        pack.Publish();

        Result result = pack.Unpublish();

        result.IsSuccess.Should().BeTrue();
        pack.Visibility.Should().Be(VeillePackVisibility.Private);
    }

    [Fact]
    public void Unpublish_fails_on_system_pack()
    {
        VeillePack pack = VeillePack.Create("c", "C", string.Empty, Now).Value!;

        Result result = pack.Unpublish();

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.veille_pack_immutable");
    }

    [Fact]
    public void Likes_counter_increments_and_decrements_with_floor_zero()
    {
        VeillePack pack = VeillePack.CreateUserPack(Author, "p", "P", string.Empty, Now).Value!;

        pack.IncrementLikes();
        pack.IncrementLikes();
        pack.LikesCount.Should().Be(2);

        pack.DecrementLikes();
        pack.DecrementLikes();
        pack.DecrementLikes();
        pack.LikesCount.Should().Be(0, "plancher à 0");
    }

    [Fact]
    public void VeillePackReport_Create_with_valid_reason_succeeds()
    {
        VeillePackId packId = VeillePackId.New();
        UserId reporter = new(Guid.NewGuid());

        Result<VeillePackReport> result = VeillePackReport.Create(packId, reporter, "Contenu litigieux", Now);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(VeillePackReportStatus.Pending);
        result.Value.Reason.Should().Be("Contenu litigieux");
    }

    [Fact]
    public void VeillePackReport_Create_with_empty_reason_fails()
    {
        Result<VeillePackReport> result =
            VeillePackReport.Create(VeillePackId.New(), new UserId(Guid.NewGuid()), "   ", Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.invalid_veille_pack_report");
    }

    [Fact]
    public void VeillePackReport_MarkReviewed_transitions_status()
    {
        VeillePackReport report = VeillePackReport.Create(
            VeillePackId.New(), new UserId(Guid.NewGuid()), "Motif", Now).Value!;

        report.MarkReviewed(removed: true, Now.AddHours(1));

        report.Status.Should().Be(VeillePackReportStatus.ReviewedRemoved);
        report.ReviewedAt.Should().Be(Now.AddHours(1));
    }
}
