using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.ResendVerification;

internal sealed class ResendVerificationHandler(
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    AuthSettings settings) : IRequestHandler<ResendVerificationCommand, Result>
{
    public async Task<Result> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        // Anti-énumération : quelle que soit l'issue (email invalide, inconnu, déjà vérifié), on
        // renvoie un succès uniforme. On n'agit que si un compte en attente correspond.
        Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Ok();
        }

        User? user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null || user.Status != UserStatus.PendingEmailVerification)
        {
            return Result.Ok();
        }

        string token = tokenGenerator.GenerateUrlSafeToken();
        user.RegenerateEmailVerificationToken(
            tokenGenerator.Hash(token), clock.UtcNow, settings.EmailVerificationTokenLifetime);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailVerificationAsync(emailResult.Value, user.Id, token, cancellationToken);

        return Result.Ok();
    }
}
