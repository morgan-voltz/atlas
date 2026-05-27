using Atlas.Application.Common;
using Atlas.Application.Users;
using Atlas.Application.Users.TwoFactor;
using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Users;

public class VerifyTwoFactorHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly ITwoFactorChallengeService _challenge = Substitute.For<ITwoFactorChallengeService>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITwoFactorRecoveryCodeRepository _recoveryCodes = Substitute.For<ITwoFactorRecoveryCodeRepository>();
    private readonly ITotpProvider _totp = Substitute.For<ITotpProvider>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IJwtIssuer _jwt = Substitute.For<IJwtIssuer>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly AuthSettings _settings = new();

    public VerifyTwoFactorHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _crypto.Decrypt(Arg.Any<string>()).Returns("SECRET");
        _jwt.Issue(Arg.Any<UserId>(), Arg.Any<EmailAddress>()).Returns(new AccessToken("access-token", Now.AddMinutes(15)));
        _tokens.GenerateUrlSafeToken(Arg.Any<int>()).Returns("raw-refresh");
        _tokens.Hash(Arg.Any<string>()).Returns("code-hash");
    }

    [Fact]
    public async Task Handle_with_valid_totp_issues_tokens()
    {
        User user = EnabledUser();
        _challenge.ValidateChallengeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((UserId?)user.Id);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _totp.VerifyCode("SECRET", "123456").Returns(true);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new VerifyTwoFactorCommand("challenge", "123456"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        await _refreshTokens.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_falls_back_to_recovery_code_and_consumes_it()
    {
        User user = EnabledUser();
        var recoveryCode = TwoFactorRecoveryCode.Create(user.Id, "code-hash", Now);
        _challenge.ValidateChallengeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((UserId?)user.Id);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _totp.VerifyCode(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        _recoveryCodes.GetActiveByHashAsync(user.Id, "code-hash", Arg.Any<CancellationToken>()).Returns(recoveryCode);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new VerifyTwoFactorCommand("challenge", "recovery"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        recoveryCode.IsUsed.Should().BeTrue();
        _recoveryCodes.Received(1).Update(recoveryCode);
    }

    [Fact]
    public async Task Handle_with_invalid_challenge_fails()
    {
        _challenge.ValidateChallengeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((UserId?)null);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new VerifyTwoFactorCommand("bad", "123456"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_two_factor_challenge");
    }

    [Fact]
    public async Task Handle_with_wrong_code_and_no_recovery_fails()
    {
        User user = EnabledUser();
        _challenge.ValidateChallengeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((UserId?)user.Id);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _totp.VerifyCode(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        _recoveryCodes.GetActiveByHashAsync(Arg.Any<UserId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((TwoFactorRecoveryCode?)null);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new VerifyTwoFactorCommand("challenge", "000000"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_two_factor_code");
    }

    private VerifyTwoFactorHandler CreateHandler()
    {
        var factory = new AuthTokenFactory(_jwt, _tokens, _refreshTokens, _settings);
        return new VerifyTwoFactorHandler(
            _challenge, _users, _recoveryCodes, _totp, _crypto, _tokens, _clock, _unitOfWork, factory);
    }

    private static User EnabledUser()
    {
        EmailAddress email = EmailAddress.Create("user@example.com").Value!;
        var user = User.Register(UserId.New(), email, PasswordHash.FromHash("stored"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);
        user.BeginTwoFactorSetup("encrypted-secret");
        user.EnableTwoFactor();
        return user;
    }
}
