using Atlas.Application.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;

namespace Atlas.Application.Users;

/// <summary>
/// Émet un access token JWT et crée (sans persister via SaveChanges) le refresh token associé.
/// La persistance effective est confiée au handler appelant via l'unité de travail.
/// </summary>
internal sealed class AuthTokenFactory(
    IJwtIssuer jwtIssuer,
    ITokenGenerator tokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    AuthSettings settings)
{
    public async Task<AuthTokensDto> IssueAsync(User user, DateTimeOffset now, CancellationToken ct)
    {
        AccessToken access = jwtIssuer.Issue(user.Id, user.Email);

        string rawRefreshToken = tokenGenerator.GenerateUrlSafeToken();
        string refreshTokenHash = tokenGenerator.Hash(rawRefreshToken);

        var refreshToken = RefreshToken.Issue(
            RefreshTokenId.New(),
            user.Id,
            refreshTokenHash,
            now,
            settings.RefreshTokenLifetime);

        await refreshTokenRepository.AddAsync(refreshToken, ct);

        return new AuthTokensDto(access.Value, access.ExpiresAt, rawRefreshToken);
    }
}
