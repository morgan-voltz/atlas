using Atlas.Domain.Common;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Inpi.ConnectInpiAccount;

internal sealed class ConnectInpiAccountHandler(
    IInpiAuthenticationProvider authenticationProvider,
    IInpiCredentialsRepository credentialsRepository,
    ICryptoService cryptoService,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<ConnectInpiAccountCommand, Result>
{
    public async Task<Result> Handle(ConnectInpiAccountCommand request, CancellationToken cancellationToken)
    {
        // Test de connectivité : on ne stocke les identifiants qu'après une authentification INPI réussie.
        Result<InpiSession> authentication =
            await authenticationProvider.AuthenticateAsync(request.Username, request.Password, cancellationToken);

        if (authentication.IsFailure)
        {
            return Result.Fail(authentication.Error!);
        }

        string encryptedUsername = cryptoService.Encrypt(request.Username);
        string encryptedPassword = cryptoService.Encrypt(request.Password);
        DateTimeOffset now = clock.UtcNow;
        var userId = new UserId(request.UserId);

        InpiCredentials? existing = await credentialsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existing is null)
        {
            var credentials = InpiCredentials.Create(userId, encryptedUsername, encryptedPassword, now);
            await credentialsRepository.AddAsync(credentials, cancellationToken);
        }
        else
        {
            existing.UpdateCredentials(encryptedUsername, encryptedPassword, now);
            credentialsRepository.Update(existing);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
