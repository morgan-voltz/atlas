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
    ITwoFactorChallengeService challengeService,
    AuthSettings settings) : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result<LoginResultDto>.Fail(UserErrors.InvalidCredentials);
        }

        User? user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
        {
            return Result<LoginResultDto>.Fail(UserErrors.InvalidCredentials);
        }

        DateTimeOffset now = clock.UtcNow;

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin(now, settings.MaxFailedLoginAttempts, settings.LockoutDuration);
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<LoginResultDto>.Fail(UserErrors.InvalidCredentials);
        }

        Result canAuthenticate = user.EnsureCanAuthenticate(now);
        if (canAuthenticate.IsFailure)
        {
            return Result<LoginResultDto>.Fail(canAuthenticate.Error!);
        }

        user.RegisterSuccessfulLogin();

        // Le mot de passe est correct : si le 2FA est actif, on n'émet pas encore les jetons mais un défi.
        if (user.TwoFactorEnabled)
        {
            string challenge = challengeService.IssueChallenge(user.Id);
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<LoginResultDto>.Ok(new LoginResultDto(TwoFactorRequired: true, Tokens: null, TwoFactorChallengeToken: challenge));
        }

        AuthTokensDto tokens = await tokenFactory.IssueAsync(user, now, cancellationToken);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LoginResultDto>.Ok(new LoginResultDto(TwoFactorRequired: false, Tokens: tokens, TwoFactorChallengeToken: null));
    }
}
