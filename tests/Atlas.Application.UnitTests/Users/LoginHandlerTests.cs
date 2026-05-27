using Atlas.Application.Common;
using Atlas.Application.Users;
using Atlas.Application.Users.Login;
using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Users;

public class LoginHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IJwtIssuer _jwt = Substitute.For<IJwtIssuer>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly AuthSettings _settings = new();

    public LoginHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _jwt.Issue(Arg.Any<UserId>(), Arg.Any<EmailAddress>()).Returns(new AccessToken("access-token", Now.AddMinutes(15)));
        _tokens.GenerateUrlSafeToken(Arg.Any<int>()).Returns("raw-refresh");
        _tokens.Hash(Arg.Any<string>()).Returns("refresh-hash");
    }

    [Fact]
    public async Task Handle_with_valid_credentials_returns_tokens()
    {
        User user = ActiveUser();
        _users.GetByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("password", Arg.Any<PasswordHash>()).Returns(true);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new LoginCommand("user@example.com", "password"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value!.RefreshToken.Should().Be("raw-refresh");
        await _refreshTokens.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_wrong_password_fails_and_records_attempt()
    {
        User user = ActiveUser();
        _users.GetByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<PasswordHash>()).Returns(false);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new LoginCommand("user@example.com", "wrong"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_credentials");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_unknown_email_fails()
    {
        _users.GetByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new LoginCommand("ghost@example.com", "password"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_credentials");
    }

    [Fact]
    public async Task Handle_with_unverified_email_fails()
    {
        User pending = PendingUser();
        _users.GetByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(pending);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<PasswordHash>()).Returns(true);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new LoginCommand("user@example.com", "password"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.email_not_verified");
    }

    private LoginHandler CreateHandler()
    {
        var factory = new AuthTokenFactory(_jwt, _tokens, _refreshTokens, _settings);
        return new LoginHandler(_users, _hasher, _clock, _unitOfWork, factory, _settings);
    }

    private static User PendingUser()
    {
        EmailAddress email = EmailAddress.Create("user@example.com").Value!;
        return User.Register(UserId.New(), email, PasswordHash.FromHash("stored"), "tok", Now, TimeSpan.FromHours(24));
    }

    private static User ActiveUser()
    {
        User user = PendingUser();
        user.ConfirmEmail("tok", Now);
        return user;
    }
}
