using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Logout;

internal sealed class LogoutHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        string tokenHash = tokenGenerator.Hash(request.RefreshToken);

        RefreshToken? existing = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        if (existing is not null)
        {
            existing.Revoke(clock.UtcNow);
            refreshTokenRepository.Update(existing);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Idempotent : déconnexion considérée réussie même si le jeton est inconnu/déjà révoqué.
        return Result.Ok();
    }
}
