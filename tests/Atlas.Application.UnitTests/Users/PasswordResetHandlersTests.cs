using Atlas.Application.Common;
using Atlas.Application.Users.PasswordReset;
using Atlas.Application.Users.ResendVerification;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Users;

public class PasswordResetHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 31, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly AuthSettings _settings = new();

    public PasswordResetHandlersTests()
    {
        _clock.UtcNow.Returns(Now);
        _tokens.GenerateUrlSafeToken(Arg.Any<int>()).Returns("clear-token");
        _tokens.Hash(Arg.Any<string>()).Returns("token-hash");
        _hasher.Hash(Arg.Any<string>()).Returns(PasswordHash.FromHash("new-argon2-hash"));
    }

    private static User PendingUser() => User.Register(
        UserId.New(),
        EmailAddress.Create("u@example.com").Value!,
        PasswordHash.FromHash("old-hash"),
        "verify-hash",
        Now,
        TimeSpan.FromHours(24));

    private RequestPasswordResetHandler RequestHandler() =>
        new(_users, _tokens, _email, _clock, _uow, _settings);

    private ResetPasswordHandler ResetHandler() =>
        new(_users, _refreshTokens, _tokens, _hasher, _clock, _uow);

    [Fact]
    public async Task Request_unknown_email_returns_ok_without_sending()
    {
        _users.GetByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        Result result = await RequestHandler().Handle(new RequestPasswordResetCommand("ghost@example.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(); // anti-énumération
        await _email.DidNotReceive().SendPasswordResetAsync(
            Arg.Any<EmailAddress>(), Arg.Any<UserId>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Request_known_user_sends_email_and_persists()
    {
        _users.GetByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(PendingUser());

        Result result = await RequestHandler().Handle(new RequestPasswordResetCommand("u@example.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _email.Received(1).SendPasswordResetAsync(
            Arg.Any<EmailAddress>(), Arg.Any<UserId>(), "clear-token", Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reset_valid_token_sets_password_and_revokes_sessions()
    {
        User user = PendingUser();
        user.BeginPasswordReset("token-hash", Now, TimeSpan.FromHours(1));
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);

        Result result = await ResetHandler().Handle(
            new ResetPasswordCommand(user.Id.Value, "clear-token", "a-strong-password"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be(PasswordHash.FromHash("new-argon2-hash"));
        await _refreshTokens.Received(1).RevokeAllForUserAsync(user.Id, Now, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reset_unknown_user_fails()
    {
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        Result result = await ResetHandler().Handle(
            new ResetPasswordCommand(Guid.NewGuid(), "clear-token", "a-strong-password"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_password_reset_token");
        await _refreshTokens.DidNotReceive().RevokeAllForUserAsync(
            Arg.Any<UserId>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Resend_for_active_user_does_not_send()
    {
        User user = PendingUser();
        user.ConfirmEmail("verify-hash", Now); // devient Active
        _users.GetByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(user);

        var handler = new ResendVerificationHandler(_users, _tokens, _email, _clock, _uow, _settings);
        Result result = await handler.Handle(new ResendVerificationCommand("u@example.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _email.DidNotReceive().SendEmailVerificationAsync(
            Arg.Any<EmailAddress>(), Arg.Any<UserId>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
