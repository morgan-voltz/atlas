using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Refresh;

internal sealed class RefreshTokenHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    AuthTokenFactory tokenFactory) : IRequestHandler<RefreshTokenCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = clock.UtcNow;
        string tokenHash = tokenGenerator.Hash(request.RefreshToken);

        RefreshToken? existing = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        if (existing is null || !existing.IsActive(now))
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
