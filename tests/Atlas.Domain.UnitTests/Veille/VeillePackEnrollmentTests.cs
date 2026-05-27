using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class VeillePackEnrollmentTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_sets_user_pack_version_and_timestamps()
    {
        var userId = UserId.New();
        var packId = VeillePackId.New();

        VeillePackEnrollment enrollment = VeillePackEnrollment.Create(userId, packId, 2, Now);

        enrollment.UserId.Should().Be(userId);
        enrollment.PackId.Should().Be(packId);
        enrollment.AppliedVersion.Should().Be(2);
        enrollment.CreatedAt.Should().Be(Now);
        enrollment.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public void MarkSynced_updates_version_and_timestamp()
    {
        VeillePackEnrollment enrollment = VeillePackEnrollment.Create(UserId.New(), VeillePackId.New(), 1, Now);

        DateTimeOffset later = Now.AddDays(10);
        enrollment.MarkSynced(3, later);

        enrollment.AppliedVersion.Should().Be(3);
        enrollment.UpdatedAt.Should().Be(later);
        enrollment.CreatedAt.Should().Be(Now);
    }
}
