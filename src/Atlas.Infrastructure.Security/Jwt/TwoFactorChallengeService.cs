using System.Security.Claims;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Atlas.Infrastructure.Security.Jwt;

/// <summary>
/// Jeton de défi 2FA : JWT court (5 min) signé avec la même clé RSA, audience dédiée « atlas-2fa »,
/// distincte de celle des access tokens pour qu'il ne puisse pas servir d'authentification complète.
/// </summary>
internal sealed class TwoFactorChallengeService(ISigningKeyProvider keyProvider) : ITwoFactorChallengeService
{
    private const string ChallengeAudience = "atlas-2fa";
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    private readonly JsonWebTokenHandler _handler = new();

    public string IssueChallenge(UserId userId)
    {
        DateTime now = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = keyProvider.Issuer,
            Audience = ChallengeAudience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(Lifetime),
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString())]),
            SigningCredentials = new SigningCredentials(keyProvider.SigningKey, keyProvider.Algorithm),
        };

        return _handler.CreateToken(descriptor);
    }

    public async Task<UserId?> ValidateChallengeAsync(string challengeToken, CancellationToken ct = default)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keyProvider.Issuer,
            ValidateAudience = true,
            ValidAudience = ChallengeAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = keyProvider.SigningKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        TokenValidationResult result = await _handler.ValidateTokenAsync(challengeToken, parameters);
        if (!result.IsValid)
        {
            return null;
        }

        Claim? subject = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub);
        if (subject is null || !Guid.TryParse(subject.Value, out Guid userId))
        {
            return null;
        }

        return new UserId(userId);
    }
}
