using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.TwoFactor;

internal sealed class SetupTwoFactorHandler(
    IUserRepository userRepository,
    ITotpProvider totpProvider,
    ICryptoService cryptoService,
    IUnitOfWork unitOfWork) : IRequestHandler<SetupTwoFactorCommand, Result<TwoFactorSetupDto>>
{
    private const string Issuer = "Atlas";

    public async Task<Result<TwoFactorSetupDto>> Handle(SetupTwoFactorCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
        if (user is null)
        {
            return Result<TwoFactorSetupDto>.Fail(UserErrors.NotFound);
        }

        string secret = totpProvider.GenerateSecret();

        Result begin = user.BeginTwoFactorSetup(cryptoService.Encrypt(secret));
        if (begin.IsFailure)
        {
            return Result<TwoFactorSetupDto>.Fail(begin.Error!);
        }

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        string provisioningUri = totpProvider.BuildProvisioningUri(secret, user.Email.Value, Issuer);
        return Result<TwoFactorSetupDto>.Ok(new TwoFactorSetupDto(secret, provisioningUri));
    }
}
