using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Users;

public class EmailAddressTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("  User@Example.COM ")]
    public void Create_with_valid_email_succeeds_and_normalizes(string raw)
    {
        Result<EmailAddress> result = EmailAddress.Create(raw);

        result.IsSuccess.Should().BeTrue();
        EmailAddress email = result.Value!;
        email.Value.Should().Be("user@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Create_with_invalid_email_fails(string? raw)
    {
        Result<EmailAddress> result = EmailAddress.Create(raw);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_email");
    }
}
