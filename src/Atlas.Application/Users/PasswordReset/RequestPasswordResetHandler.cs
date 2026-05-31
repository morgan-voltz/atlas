using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.PasswordReset;

internal sealed class RequestPasswordResetHandler(
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    AuthSettings settings) : IRequestHandler<RequestPasswordResetCommand, Result>
{
    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        // Anti-énumération : succès uniforme même si l'email est invalide ou inconnu.
        Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Ok();
        }

        User? user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
        {
            return Result.Ok();
        }

        string token = tokenGenerator.GenerateUrlSafeToken();
        user.BeginPasswordReset(
            tokenGenerator.Hash(token), clock.UtcNow, settings.PasswordResetTokenLifetime);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailSender.SendPasswordResetAsync(emailResult.Value, user.Id, token, cancellationToken);

        return Result.Ok();
    }
}
