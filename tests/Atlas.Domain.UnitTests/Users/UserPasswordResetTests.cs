using Atlas.Domain.Users;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Users;

public class UserPasswordResetTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 31, 12, 0, 0, TimeSpan.Zero);

    private static User NewUser() => User.Register(
        UserId.New(),
        EmailAddress.Create("u@example.com").Value!,
        PasswordHash.FromHash("old-argon2-hash"),
        "verify-hash",
        Now,
        TimeSpan.FromHours(24));

    [Fact]
    public void ResetPassword_succeeds_with_valid_token_and_replaces_hash()
    {
        var user = NewUser();
        user.BeginPasswordReset("reset-hash", Now, TimeSpan.FromHours(1));

        var result = user.ResetPassword("reset-hash", PasswordHash.FromHash("new-argon2-hash"), Now.AddMinutes(5));

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be(PasswordHash.FromHash("new-argon2-hash"));
    }

    [Fact]
    public void ResetPassword_is_single_use()
    {
        var user = NewUser();
        user.BeginPasswordReset("reset-hash", Now, TimeSpan.FromHours(1));
        user.ResetPassword("reset-hash", PasswordHash.FromHash("new-hash"), Now.AddMinutes(1));

        var second = user.ResetPassword("reset-hash", PasswordHash.FromHash("other-hash"), Now.AddMinutes(2));

        second.IsFailure.Should().BeTrue();
        second.Error!.Code.Should().Be("users.invalid_password_reset_token");
    }

    [Fact]
    public void ResetPassword_fails_with_wrong_token()
    {
        var user = NewUser();
        user.BeginPasswordReset("reset-hash", Now, TimeSpan.FromHours(1));

        var result = user.ResetPassword("wrong-hash", PasswordHash.FromHash("new-hash"), Now.AddMinutes(1));

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_password_reset_token");
    }

    [Fact]
    public void ResetPassword_fails_when_expired()
    {
        var user = NewUser();
        user.BeginPasswordReset("reset-hash", Now, TimeSpan.FromHours(1));

        var result = user.ResetPassword("reset-hash", PasswordHash.FromHash("new-hash"), Now.AddHours(2));

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_password_reset_token");
    }

    [Fact]
    public void ResetPassword_fails_when_no_reset_requested()
    {
        var user = NewUser();

        var result = user.ResetPassword("any-hash", PasswordHash.FromHash("new-hash"), Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_password_reset_token");
    }

    [Fact]
    public void RegenerateEmailVerificationToken_lets_new_token_confirm_and_invalidates_old()
    {
        var user = NewUser();
        user.RegenerateEmailVerificationToken("new-verify-hash", Now.AddMinutes(10), TimeSpan.FromHours(24));

        // L'ancien token ne marche plus.
        user.ConfirmEmail("verify-hash", Now.AddMinutes(11)).IsFailure.Should().BeTrue();
        // Le nouveau token active le compte.
        user.ConfirmEmail("new-verify-hash", Now.AddMinutes(12)).IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void RegenerateEmailVerificationToken_is_noop_when_already_active()
    {
        var user = NewUser();
        user.ConfirmEmail("verify-hash", Now).IsSuccess.Should().BeTrue();

        // Sans effet : régénérer sur un compte déjà actif ne le replonge pas en attente.
        user.RegenerateEmailVerificationToken("ignored-hash", Now, TimeSpan.FromHours(24));

        user.Status.Should().Be(UserStatus.Active);
    }
}
