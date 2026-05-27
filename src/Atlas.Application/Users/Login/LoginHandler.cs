using Atlas.Application.Common;
using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Login;

internal sealed class LoginHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    AuthTokenFactory tokenFactory,
    AuthSettings settings) : IRequestHandler<LoginCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidCredentials);
        }

        User? user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
        {
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidCredentials);
        }

        DateTimeOffset now = clock.UtcNow;

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin(now, settings.MaxFailedLoginAttempts, settings.LockoutDuration);
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<AuthTokensDto>.Fail(UserErrors.InvalidCredentials);
        }

        Result canAuthenticate = user.EnsureCanAuthenticate(now);
        if (canAuthenticate.IsFailure)
        {
            return Result<AuthTokensDto>.Fail(canAuthenticate.Error!);
        }

        user.RegisterSuccessfulLogin();
        AuthTokensDto tokens = await tokenFactory.IssueAsync(user, now, cancellationToken);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthTokensDto>.Ok(tokens);
    }
}
