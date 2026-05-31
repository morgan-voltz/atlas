using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.PasswordReset;

internal sealed class ResetPasswordHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenGenerator tokenGenerator,
    IPasswordHasher passwordHasher,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
        if (user is null)
        {
            return Result.Fail(UserErrors.InvalidOrExpiredPasswordResetToken);
        }

        string providedTokenHash = tokenGenerator.Hash(request.Token);
        PasswordHash newPasswordHash = passwordHasher.Hash(request.NewPassword);

        Result reset = user.ResetPassword(providedTokenHash, newPasswordHash, clock.UtcNow);
        if (reset.IsFailure)
        {
            return reset;
        }

        // Sécurité : un mot de passe réinitialisé invalide toutes les sessions existantes.
        await refreshTokenRepository.RevokeAllForUserAsync(user.Id, clock.UtcNow, cancellationToken);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
