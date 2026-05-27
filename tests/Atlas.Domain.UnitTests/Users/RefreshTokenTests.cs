using Atlas.Domain.Users;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Users;

public class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Issue_creates_an_active_token()
    {
        var token = RefreshToken.Issue(RefreshTokenId.New(), UserId.New(), "hash", Now, TimeSpan.FromDays(30));

        token.IsActive(Now.AddDays(1)).Should().BeTrue();
        token.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void Revoke_makes_the_token_inactive()
    {
        var token = RefreshToken.Issue(RefreshTokenId.New(), UserId.New(), "hash", Now, TimeSpan.FromDays(30));

        token.Revoke(Now.AddHours(1));

        token.IsActive(Now.AddHours(2)).Should().BeFalse();
        token.RevokedAt.Should().Be(Now.AddHours(1));
    }

    [Fact]
    public void Expired_token_is_inactive()
    {
        var token = RefreshToken.Issue(RefreshTokenId.New(), UserId.New(), "hash", Now, TimeSpan.FromDays(30));

        token.IsActive(Now.AddDays(31)).Should().BeFalse();
    }
}
