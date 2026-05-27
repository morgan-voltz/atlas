using Atlas.Domain.Common;
using Atlas.Domain.Inpi;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Inpi.DisconnectInpiAccount;

internal sealed class DisconnectInpiAccountHandler(
    IInpiCredentialsRepository credentialsRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DisconnectInpiAccountCommand, Result>
{
    public async Task<Result> Handle(DisconnectInpiAccountCommand request, CancellationToken cancellationToken)
    {
        await credentialsRepository.DeleteByUserAsync(new UserId(request.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Idempotent : déconnexion réussie même si aucun compte n'était connecté.
        return Result.Ok();
    }
}
