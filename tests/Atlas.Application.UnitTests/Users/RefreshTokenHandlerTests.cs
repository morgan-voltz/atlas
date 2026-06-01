using Atlas.Application.Common;
using Atlas.Application.Users;
using Atlas.Application.Users.Refresh;
using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Users;

public class RefreshTokenHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IJwtIssuer _jwt = Substitute.For<IJwtIssuer>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly AuthSettings _settings = new();

    public RefreshTokenHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _tokens.Hash(Arg.Any<string>()).Returns("incoming-hash");
        _tokens.GenerateUrlSafeToken(Arg.Any<int>()).Returns("new-raw-refresh");
        _jwt.Issue(Arg.Any<UserId>(), Arg.Any<EmailAddress>()).Returns(new AccessToken("access", Now.AddMinutes(15)));
    }

    [Fact]
    public async Task Handle_with_unknown_token_fails_without_revoking_family()
    {
        _refreshTokens.GetByHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new RefreshTokenCommand("whatever"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_refresh_token");
        await _refreshTokens.DidNotReceive()
            .RevokeAllForUserAsync(Arg.Any<UserId>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_revoked_token_reuse_revokes_the_whole_family()
    {
        // theft detection (audit Lot 1) : rejouer un jeton déjà révoqué (donc tourné) déclenche
        // la révocation de tous les jetons de l'utilisateur.
        UserId userId = UserId.New();
        RefreshToken revoked = RefreshToken.Issue(RefreshTokenId.New(), userId, "incoming-hash", Now.AddMinutes(-5), TimeSpan.FromDays(7));
        revoked.Revoke(Now.AddMinutes(-1));
        _refreshTokens.GetByHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(revoked);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new RefreshTokenCommand("stolen"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_refresh_token");
        await _refreshTokens.Received(1).RevokeAllForUserAsync(userId, Now, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_expired_token_fails_without_revoking_family()
    {
        // Un jeton simplement expiré (jamais révoqué) est un échec normal : pas de theft detection.
        RefreshToken expired = RefreshToken.Issue(RefreshTokenId.New(), UserId.New(), "incoming-hash", Now.AddDays(-8), TimeSpan.FromDays(7));
        _refreshTokens.GetByHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(expired);

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new RefreshTokenCommand("old"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_refresh_token");
        await _refreshTokens.DidNotReceive()
            .RevokeAllForUserAsync(Arg.Any<UserId>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_active_token_rotates_and_returns_new_tokens()
    {
        UserId userId = UserId.New();
        RefreshToken active = RefreshToken.Issue(RefreshTokenId.New(), userId, "incoming-hash", Now, TimeSpan.FromDays(7));
        _refreshTokens.GetByHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(active);
        _users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(ActiveUser(userId));

        Result<AuthTokensDto> result = await CreateHandler()
            .Handle(new RefreshTokenCommand("valid"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RefreshToken.Should().Be("new-raw-refresh");
        active.RevokedAt.Should().Be(Now, "le jeton courant est révoqué lors de la rotation");
        await _refreshTokens.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private RefreshTokenHandler CreateHandler()
    {
        var factory = new AuthTokenFactory(_jwt, _tokens, _refreshTokens, _settings);
        return new RefreshTokenHandler(
            _users, _refreshTokens, _tokens, _clock, _unitOfWork, factory, NullLogger<RefreshTokenHandler>.Instance);
    }

    private static User ActiveUser(UserId userId)
    {
        EmailAddress email = EmailAddress.Create("user@example.com").Value!;
        User user = User.Register(userId, email, PasswordHash.FromHash("stored"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);
        return user;
    }
}
