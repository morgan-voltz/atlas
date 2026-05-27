using Atlas.Domain.Inpi;
using Atlas.Domain.Users;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Inpi;

public class InpiCredentialsTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_starts_active_and_timestamped()
    {
        var credentials = InpiCredentials.Create(UserId.New(), "enc-user", "enc-pass", Now);

        credentials.Status.Should().Be(InpiCredentialsStatus.Active);
        credentials.EncryptedUsername.Should().Be("enc-user");
        credentials.EncryptedPassword.Should().Be("enc-pass");
        credentials.CreatedAt.Should().Be(Now);
        credentials.LastTestedAt.Should().Be(Now);
    }

    [Fact]
    public void UpdateCredentials_replaces_secrets_and_refreshes_timestamps()
    {
        var credentials = InpiCredentials.Create(UserId.New(), "old-user", "old-pass", Now);
        DateTimeOffset later = Now.AddDays(10);

        credentials.UpdateCredentials("new-user", "new-pass", later);

        credentials.EncryptedUsername.Should().Be("new-user");
        credentials.EncryptedPassword.Should().Be("new-pass");
        credentials.Status.Should().Be(InpiCredentialsStatus.Active);
        credentials.UpdatedAt.Should().Be(later);
        credentials.LastTestedAt.Should().Be(later);
    }

    [Fact]
    public void MarkTested_failure_sets_invalid_status()
    {
        var credentials = InpiCredentials.Create(UserId.New(), "enc-user", "enc-pass", Now);

        credentials.MarkTested(success: false, Now.AddHours(1));

        credentials.Status.Should().Be(InpiCredentialsStatus.Invalid);
        credentials.LastTestedAt.Should().Be(Now.AddHours(1));
    }
}
