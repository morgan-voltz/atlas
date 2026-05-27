using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.TwoFactor;

internal sealed class VerifyTwoFactorHandler(
    ITwoFactorChallengeService challengeService,
    IUserRepository userRepository,
    ITwoFactorRecoveryCodeRepository recoveryCodeRepository,
    ITotpProvider totpProvider,
    ICryptoService cryptoService,
    ITokenGenerator tokenGenerator,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    AuthTokenFactory tokenFactory) : IRequestHandler<VerifyTwoFactorCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        UserId? challengedUserId = await challengeService.ValidateChallengeAsync(request.ChallengeToken, cancellationToken);
        if (challengedUserId is null)
        {
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidTwoFactorChallenge);
        }

        User? user = await userRepository.GetByIdAsync(challengedUserId.Value, cancellationToken);
        if (user is null || !user.TwoFactorEnabled || user.TwoFactorSecret is null)
        {
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidTwoFactorChallenge);
        }

        DateTimeOffset now = clock.UtcNow;

        bool verified = totpProvider.VerifyCode(cryptoService.Decrypt(user.TwoFactorSecret), request.Code);

        if (!verified)
        {
            // Repli sur un code de secours à usage unique.
            string codeHash = tokenGenerator.Hash(request.Code);
            TwoFactorRecoveryCode? recoveryCode =
                await recoveryCodeRepository.GetActiveByHashAsync(user.Id, codeHash, cancellationToken);

            if (recoveryCode is not null)
            {
                recoveryCode.MarkUsed(now);
                recoveryCodeRepository.Update(recoveryCode);
                verified = true;
            }
        }

        if (!verified)
        {
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidTwoFactorCode);
        }

        AuthTokensDto tokens = await tokenFactory.IssueAsync(user, now, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthTokensDto>.Ok(tokens);
    }
}
