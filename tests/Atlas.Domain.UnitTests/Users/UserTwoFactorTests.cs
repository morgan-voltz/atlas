using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Users;

public class UserTwoFactorTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void BeginTwoFactorSetup_stores_pending_secret_without_enabling()
    {
        var user = ActiveUser();

        Result result = user.BeginTwoFactorSetup("encrypted");

        result.IsSuccess.Should().BeTrue();
        user.PendingTwoFactorSecret.Should().Be("encrypted");
        user.TwoFactorEnabled.Should().BeFalse();
    }

    [Fact]
    public void EnableTwoFactor_without_setup_fails()
    {
        var user = ActiveUser();

        Result result = user.EnableTwoFactor();

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.two_factor_setup_not_started");
    }

    [Fact]
    public void EnableTwoFactor_after_setup_promotes_secret_and_clears_pending()
    {
        var user = ActiveUser();
        user.BeginTwoFactorSetup("encrypted");

        Result result = user.EnableTwoFactor();

        result.IsSuccess.Should().BeTrue();
        user.TwoFactorEnabled.Should().BeTrue();
        user.TwoFactorSecret.Should().Be("encrypted");
        user.PendingTwoFactorSecret.Should().BeNull();
    }

    [Fact]
    public void BeginTwoFactorSetup_when_already_enabled_fails()
    {
        var user = EnabledUser();

        Result result = user.BeginTwoFactorSetup("other");

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.two_factor_already_enabled");
    }

    [Fact]
    public void DisableTwoFactor_clears_all_state()
    {
        var user = EnabledUser();

        Result result = user.DisableTwoFactor();

        result.IsSuccess.Should().BeTrue();
        user.TwoFactorEnabled.Should().BeFalse();
        user.TwoFactorSecret.Should().BeNull();
        user.PendingTwoFactorSecret.Should().BeNull();
    }

    [Fact]
    public void DisableTwoFactor_when_not_enabled_fails()
    {
        var user = ActiveUser();

        Result result = user.DisableTwoFactor();

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.two_factor_not_enabled");
    }

    private static User ActiveUser()
    {
        EmailAddress email = EmailAddress.Create("user@example.com").Value!;
        var user = User.Register(UserId.New(), email, PasswordHash.FromHash("stored"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);
        return user;
    }

    private static User EnabledUser()
    {
        User user = ActiveUser();
        user.BeginTwoFactorSetup("encrypted");
        user.EnableTwoFactor();
        return user;
    }
}
