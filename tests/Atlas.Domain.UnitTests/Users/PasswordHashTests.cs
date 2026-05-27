using Atlas.Domain.Users;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Users;

public class PasswordHashTests
{
    [Fact]
    public void FromHash_keeps_value()
    {
        PasswordHash hash = PasswordHash.FromHash("argon2-encoded");
        hash.Value.Should().Be("argon2-encoded");
    }

    [Fact]
    public void ToString_never_reveals_the_hash()
    {
        PasswordHash hash = PasswordHash.FromHash("argon2-encoded");
        hash.ToString().Should().NotContain("argon2-encoded");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromHash_rejects_empty(string value)
    {
        Action act = () => PasswordHash.FromHash(value);
        act.Should().Throw<ArgumentException>();
    }
}
