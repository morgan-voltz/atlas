using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Register;

internal sealed class RegisterUserHandler(
    IUserRepository userRepository,
    IAccountRepository accountRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    AuthSettings settings) : IRequestHandler<RegisterUserCommand, Result>
{
    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Fail(emailResult.Error!);
        }

        EmailAddress email = emailResult.Value;

        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return Result.Fail(UserErrors.EmailAlreadyInUse(email));
        }

        PasswordHash passwordHash = passwordHasher.Hash(request.Password);

        string verificationToken = tokenGenerator.GenerateUrlSafeToken();
        string verificationTokenHash = tokenGenerator.Hash(verificationToken);

        DateTimeOffset now = clock.UtcNow;

        var user = User.Register(
            UserId.New(),
            email,
            passwordHash,
            verificationTokenHash,
            now,
            settings.EmailVerificationTokenLifetime);

        Account account = Account.Create(AccountId.New(), user.Id, now);

        await userRepository.AddAsync(user, cancellationToken);
        await accountRepository.AddAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailVerificationAsync(email, user.Id, verificationToken, cancellationToken);

        return Result.Ok();
    }
}
