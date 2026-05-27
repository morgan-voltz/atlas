using Atlas.Domain.Users;
using Atlas.Domain.Users.Events;
using Atlas.Shared.Result;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Users;

public class UserTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

    [Fact]
    public void Register_creates_pending_user_and_raises_event()
    {
        var user = CreatePending("token-hash");

        user.Status.Should().Be(UserStatus.PendingEmailVerification);
        user.EmailVerifiedAt.Should().BeNull();
        user.DomainEvents.Should().ContainSingle(domainEvent => domainEvent is UserRegisteredDomainEvent);
    }

    [Fact]
    public void ConfirmEmail_with_correct_token_activates_account()
    {
        var user = CreatePending("token-hash");

        Result result = user.ConfirmEmail("token-hash", Now.AddHours(1));

        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.EmailVerifiedAt.Should().Be(Now.AddHours(1));
    }

    [Fact]
    public void ConfirmEmail_with_wrong_token_fails()
    {
        var user = CreatePending("token-hash");

        Result result = user.ConfirmEmail("wrong-hash", Now.AddHours(1));

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_verification_token");
        user.Status.Should().Be(UserStatus.PendingEmailVerification);
    }

    [Fact]
    public void ConfirmEmail_after_expiry_fails()
    {
        var user = CreatePending("token-hash");

        Result result = user.ConfirmEmail("token-hash", Now.AddHours(25));

        result.IsFailure.Should().BeTrue();
        user.Status.Should().Be(UserStatus.PendingEmailVerification);
    }

    [Fact]
    public void ConfirmEmail_is_idempotent_once_active()
    {
        var user = CreatePending("token-hash");
        user.ConfirmEmail("token-hash", Now);

        Result second = user.ConfirmEmail("now-irrelevant", Now);

        second.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void RegisterFailedLogin_locks_the_account_after_max_attempts()
    {
        var user = CreatePending("token-hash");
        user.ConfirmEmail("token-hash", Now);

        for (int attempt = 0; attempt < 5; attempt++)
        {
            user.RegisterFailedLogin(Now, maxAttempts: 5, lockoutDuration: TimeSpan.FromMinutes(15));
        }

        user.EnsureCanAuthenticate(Now.AddMinutes(1)).IsFailure.Should().BeTrue();
        user.EnsureCanAuthenticate(Now.AddMinutes(16)).IsSuccess.Should().BeTrue();
    }

    private static User CreatePending(string tokenHash)
    {
        EmailAddress email = EmailAddress.Create("user@example.com").Value!;
        return User.Register(UserId.New(), email, PasswordHash.FromHash("stored"), tokenHash, Now, TokenLifetime);
    }
}
