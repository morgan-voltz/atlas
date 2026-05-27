using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Privacy;

internal sealed class DeleteAccountHandler(IUserRepository userRepository)
    : IRequestHandler<DeleteAccountCommand, Result>
{
    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        // Effacement (RGPD art. 17) : la suppression de l'utilisateur cascade sur toutes ses données liées.
        await userRepository.DeleteAsync(new UserId(request.UserId), cancellationToken);

        // Idempotent : si le compte n'existe plus, la demande est considérée satisfaite.
        return Result.Ok();
    }
}
