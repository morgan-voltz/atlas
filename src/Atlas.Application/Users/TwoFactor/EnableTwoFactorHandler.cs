using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.TwoFactor;

internal sealed class EnableTwoFactorHandler(
    IUserRepository userRepository,
    ITwoFactorRecoveryCodeRepository recoveryCodeRepository,
    ITotpProvider totpProvider,
    ICryptoService cryptoService,
    ITokenGenerator tokenGenerator,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    AuthSettings settings) : IRequestHandler<EnableTwoFactorCommand, Result<TwoFactorEnabledDto>>
{
    public async Task<Result<TwoFactorEnabledDto>> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
        if (user is null)
        {
            return Result<TwoFactorEnabledDto>.Fail(UserErrors.NotFound);
        }

        if (user.TwoFactorEnabled)
        {
            return Result<TwoFactorEnabledDto>.Fail(UserErrors.TwoFactorAlreadyEnabled);
        }

        if (user.PendingTwoFactorSecret is null)
        {
            return Result<TwoFactorEnabledDto>.Fail(UserErrors.TwoFactorSetupNotStarted);
        }

        string secret = cryptoService.Decrypt(user.PendingTwoFactorSecret);
        if (!totpProvider.VerifyCode(secret, request.Code))
        {
            return Result<TwoFactorEnabledDto>.Fail(UserErrors.InvalidTwoFactorCode);
        }

        Result enable = user.EnableTwoFactor();
        if (enable.IsFailure)
        {
            return Result<TwoFactorEnabledDto>.Fail(enable.Error!);
        }

        DateTimeOffset now = clock.UtcNow;
        var rawCodes = new List<string>(settings.RecoveryCodeCount);
        var entities = new List<TwoFactorRecoveryCode>(settings.RecoveryCodeCount);

        for (int index = 0; index < settings.RecoveryCodeCount; index++)
        {
            string rawCode = tokenGenerator.GenerateUrlSafeToken(8);
            rawCodes.Add(rawCode);
            entities.Add(TwoFactorRecoveryCode.Create(user.Id, tokenGenerator.Hash(rawCode), now));
        }

        await recoveryCodeRepository.DeleteByUserAsync(user.Id, cancellationToken);
        await recoveryCodeRepository.AddRangeAsync(entities, cancellationToken);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TwoFactorEnabledDto>.Ok(new TwoFactorEnabledDto(rawCodes));
    }
}
