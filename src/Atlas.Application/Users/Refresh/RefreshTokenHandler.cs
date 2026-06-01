using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Users.Refresh;

internal sealed class RefreshTokenHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    AuthTokenFactory tokenFactory,
    ILogger<RefreshTokenHandler> logger) : IRequestHandler<RefreshTokenCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = clock.UtcNow;
        string tokenHash = tokenGenerator.Hash(request.RefreshToken);

        RefreshToken? existing = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        if (existing is null)
        {
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidOrExpiredRefreshToken);
        }

        // Détection de réutilisation (theft detection, audit Lot 1) : un jeton DÉJÀ révoqué est rejoué.
        // La rotation a normalement invalidé l'ancien jeton à sa première utilisation ; le revoir signale
        // qu'une copie circule (vol probable). On révoque alors TOUTE la famille de jetons de l'utilisateur
        // pour forcer une reconnexion complète et neutraliser la copie volée comme la session légitime.
        if (existing.RevokedAt is not null)
        {
            logger.LogWarning(
                "Réutilisation d'un refresh token révoqué (utilisateur {UserId}) : révocation de tous ses jetons (suspicion de compromission).",
                existing.UserId.Value);
            await refreshTokenRepository.RevokeAllForUserAsync(existing.UserId, now, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidOrExpiredRefreshToken);
        }

        // Jeton connu mais simplement expiré : échec normal, sans suspicion de compromission.
        if (!existing.IsActive(now))
        {
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidOrExpiredRefreshToken);
        }

        User? user = await userRepository.GetByIdAsync(existing.UserId, cancellationToken);
        if (user is null)
        {
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidOrExpiredRefreshToken);
        }

        // Rotation : on révoque l'ancien jeton et on en émet un nouveau.
        existing.Revoke(now);
        refreshTokenRepository.Update(existing);

        AuthTokensDto tokens = await tokenFactory.IssueAsync(user, now, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthTokensDto>.Ok(tokens);
    }
}
