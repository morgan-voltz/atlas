using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Infrastructure.Security.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace Atlas.Infrastructure.Security.UnitTests.Jwt;

public sealed class JwtIssuerTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly RsaSigningKeyProvider _keyProvider =
        new(Options.Create(new JwtOptions { Issuer = "atlas", Audience = "atlas", AccessTokenMinutes = 15 }));

    public void Dispose() => _keyProvider.Dispose();

    private JwtIssuer CreateIssuer(DateTimeOffset? now = null)
    {
        IDateTimeProvider clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(now ?? Now);
        return new JwtIssuer(
            _keyProvider,
            Options.Create(new JwtOptions { Issuer = "atlas", Audience = "atlas", AccessTokenMinutes = 15 }),
            clock);
    }

    private static EmailAddress NewEmail() => EmailAddress.Create($"u-{Guid.NewGuid():N}@example.com").Value!;

    [Fact]
    public void Issue_returns_signed_token_with_expected_claims()
    {
        JwtIssuer issuer = CreateIssuer();
        UserId userId = UserId.New();
        EmailAddress email = NewEmail();

        AccessToken token = issuer.Issue(userId, email);

        var handler = new JsonWebTokenHandler();
        JsonWebToken jwt = handler.ReadJsonWebToken(token.Value);
        jwt.Subject.Should().Be(userId.Value.ToString());
        jwt.GetClaim(JwtRegisteredClaimNames.Email).Value.Should().Be(email.Value);
        jwt.GetClaim(JwtRegisteredClaimNames.Jti).Value.Should().NotBeNullOrEmpty();
        jwt.Issuer.Should().Be("atlas");
        jwt.Audiences.Should().Contain("atlas");
    }

    [Fact]
    public void Issue_token_expires_after_AccessTokenMinutes()
    {
        JwtIssuer issuer = CreateIssuer();

        AccessToken token = issuer.Issue(UserId.New(), NewEmail());

        token.ExpiresAt.Should().Be(Now.AddMinutes(15));
    }

    [Fact]
    public async Task Issued_token_validates_with_signing_key_and_expected_parameters()
    {
        JwtIssuer issuer = CreateIssuer();
        AccessToken token = issuer.Issue(UserId.New(), NewEmail());

        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Value, NewValidationParameters());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Token_with_tampered_signature_is_invalid()
    {
        JwtIssuer issuer = CreateIssuer();
        AccessToken token = issuer.Issue(UserId.New(), NewEmail());
        // Flip un caractère dans la signature (dernier segment du JWT).
        string[] parts = token.Value.Split('.');
        parts[2] = parts[2].Length > 0 ? parts[2][..^1] + (parts[2][^1] == 'A' ? 'B' : 'A') : "X";
        string tampered = string.Join('.', parts);

        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(tampered, NewValidationParameters());

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Token_with_wrong_audience_is_invalid()
    {
        JwtIssuer issuer = CreateIssuer();
        AccessToken token = issuer.Issue(UserId.New(), NewEmail());

        TokenValidationParameters parameters = NewValidationParameters();
        parameters.ValidAudience = "autre-audience";

        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Value, parameters);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Expired_token_is_invalid()
    {
        // Émis au temps T, validé avec validité < T - 15min ; ClockSkew=0 → expiré.
        JwtIssuer issuer = CreateIssuer(Now);
        AccessToken token = issuer.Issue(UserId.New(), NewEmail());

        TokenValidationParameters parameters = NewValidationParameters();
        parameters.LifetimeValidator = (notBefore, expires, _, _) => expires > Now.AddHours(1).UtcDateTime;

        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Value, parameters);

        result.IsValid.Should().BeFalse();
    }

    private TokenValidationParameters NewValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidIssuer = _keyProvider.Issuer,
        ValidateAudience = true,
        ValidAudience = _keyProvider.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = _keyProvider.SigningKey,
        ValidateLifetime = false, // contrôlé par LifetimeValidator quand pertinent
        ClockSkew = TimeSpan.Zero,
    };
}
