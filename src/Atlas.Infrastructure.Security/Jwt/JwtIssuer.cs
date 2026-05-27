using System.Security.Claims;
using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Atlas.Infrastructure.Security.Jwt;

internal sealed class JwtIssuer(
    ISigningKeyProvider keyProvider,
    IOptions<JwtOptions> options,
    IDateTimeProvider clock) : IJwtIssuer
{
    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken Issue(UserId userId, EmailAddress email)
    {
        DateTime issuedAt = clock.UtcNow.UtcDateTime;
        DateTime expires = issuedAt.AddMinutes(options.Value.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = keyProvider.Issuer,
            Audience = keyProvider.Audience,
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expires,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ]),
            SigningCredentials = new SigningCredentials(keyProvider.SigningKey, keyProvider.Algorithm),
        };

        string token = _handler.CreateToken(descriptor);
        return new AccessToken(token, new DateTimeOffset(expires, TimeSpan.Zero));
    }
}
