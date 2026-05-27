using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.VerifyEmail;

internal sealed class VerifyEmailHandler(
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<VerifyEmailCommand, Result>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
        if (user is null)
        {
            return Result.Fail(UserErrors.InvalidOrExpiredVerificationToken);
        }

        string providedTokenHash = tokenGenerator.Hash(request.Token);

        Result confirmation = user.ConfirmEmail(providedTokenHash, clock.UtcNow);
        if (confirmation.IsFailure)
        {
            return confirmation;
        }

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
