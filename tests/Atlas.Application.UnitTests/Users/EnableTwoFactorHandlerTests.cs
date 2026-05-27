using Atlas.Application.Common;
using Atlas.Application.Users.TwoFactor;
using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Users;

public class EnableTwoFactorHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITwoFactorRecoveryCodeRepository _recoveryCodes = Substitute.For<ITwoFactorRecoveryCodeRepository>();
    private readonly ITotpProvider _totp = Substitute.For<ITotpProvider>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AuthSettings _settings = new();

    public EnableTwoFactorHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _crypto.Decrypt(Arg.Any<string>()).Returns("SECRET");
        _tokens.GenerateUrlSafeToken(Arg.Any<int>()).Returns("raw-code");
        _tokens.Hash(Arg.Any<string>()).Returns("code-hash");
    }

    [Fact]
    public async Task Handle_with_valid_code_enables_two_factor_and_returns_recovery_codes()
    {
        User user = PendingSetupUser();
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);
        _totp.VerifyCode("SECRET", "123456").Returns(true);

        Result<TwoFactorEnabledDto> result = await CreateHandler()
            .Handle(new EnableTwoFactorCommand(user.Id.Value, "123456"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RecoveryCodes.Should().HaveCount(_settings.RecoveryCodeCount);
        user.TwoFactorEnabled.Should().BeTrue();
        await _recoveryCodes.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<TwoFactorRecoveryCode>>(codes => codes.Count() == _settings.RecoveryCodeCount),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_invalid_code_fails()
    {
        User user = PendingSetupUser();
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);
        _totp.VerifyCode(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        Result<TwoFactorEnabledDto> result = await CreateHandler()
            .Handle(new EnableTwoFactorCommand(user.Id.Value, "000000"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_two_factor_code");
        user.TwoFactorEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_without_pending_setup_fails()
    {
        User user = ActiveUser();
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);

        Result<TwoFactorEnabledDto> result = await CreateHandler()
            .Handle(new EnableTwoFactorCommand(user.Id.Value, "123456"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.two_factor_setup_not_started");
    }

    private EnableTwoFactorHandler CreateHandler() =>
        new(_users, _recoveryCodes, _totp, _crypto, _tokens, _clock, _unitOfWork, _settings);

    private static User ActiveUser()
    {
        EmailAddress email = EmailAddress.Create("user@example.com").Value!;
        var user = User.Register(UserId.New(), email, PasswordHash.FromHash("stored"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);
        return user;
    }

    private static User PendingSetupUser()
    {
        User user = ActiveUser();
        user.BeginTwoFactorSetup("encrypted-secret");
        return user;
    }
}
