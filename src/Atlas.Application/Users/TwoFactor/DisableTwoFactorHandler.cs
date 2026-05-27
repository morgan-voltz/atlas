using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.TwoFactor;

internal sealed class DisableTwoFactorHandler(
    IUserRepository userRepository,
    ITwoFactorRecoveryCodeRepository recoveryCodeRepository,
    ITotpProvider totpProvider,
    ICryptoService cryptoService,
    IUnitOfWork unitOfWork) : IRequestHandler<DisableTwoFactorCommand, Result>
{
    public async Task<Result> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
        if (user is null)
        {
            return Result.Fail(UserErrors.NotFound);
        }

        if (!user.TwoFactorEnabled || user.TwoFactorSecret is null)
        {
            return Result.Fail(UserErrors.TwoFactorNotEnabled);
        }

        if (!totpProvider.VerifyCode(cryptoService.Decrypt(user.TwoFactorSecret), request.Code))
        {
            return Result.Fail(UserErrors.InvalidTwoFactorCode);
        }

        Result disable = user.DisableTwoFactor();
        if (disable.IsFailure)
        {
            return disable;
        }

        await recoveryCodeRepository.DeleteByUserAsync(user.Id, cancellationToken);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
