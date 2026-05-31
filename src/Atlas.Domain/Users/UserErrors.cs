using Atlas.Domain.Common;

namespace Atlas.Domain.Users;

public static class UserErrors
{
    public static DomainError InvalidEmail(string value) => new InvalidEmailError(value);

    public static DomainError EmailAlreadyInUse(EmailAddress email) => new EmailAlreadyInUseError(email.Value);

    public static DomainError WeakPassword(string reason) => new WeakPasswordError(reason);

    public static readonly DomainError NotFound = new UserNotFoundError();

    public static readonly DomainError InvalidCredentials = new InvalidCredentialsError();

    public static readonly DomainError EmailNotVerified = new EmailNotVerifiedError();

    public static readonly DomainError AccountLocked = new AccountLockedError();

    public static readonly DomainError InvalidOrExpiredVerificationToken = new InvalidVerificationTokenError();

    public static readonly DomainError InvalidOrExpiredPasswordResetToken = new InvalidPasswordResetTokenError();

    public static readonly DomainError InvalidOrExpiredRefreshToken = new InvalidRefreshTokenError();

    public static readonly DomainError TwoFactorAlreadyEnabled = new TwoFactorAlreadyEnabledError();

    public static readonly DomainError TwoFactorNotEnabled = new TwoFactorNotEnabledError();

    public static readonly DomainError TwoFactorSetupNotStarted = new TwoFactorSetupNotStartedError();

    public static readonly DomainError InvalidTwoFactorCode = new InvalidTwoFactorCodeError();

    public static readonly DomainError InvalidTwoFactorChallenge = new InvalidTwoFactorChallengeError();

    private sealed record InvalidEmailError(string Value)
        : DomainError("users.invalid_email", $"L'adresse email « {Value} » est invalide.");

    private sealed record EmailAlreadyInUseError(string Value)
        : DomainError("users.email_already_in_use", "Cette adresse email est déjà utilisée.");

    private sealed record WeakPasswordError(string Reason)
        : DomainError("users.weak_password", Reason);

    private sealed record UserNotFoundError()
        : DomainError("users.not_found", "Utilisateur introuvable.");

    private sealed record InvalidCredentialsError()
        : DomainError("users.invalid_credentials", "Email ou mot de passe incorrect.");

    private sealed record EmailNotVerifiedError()
        : DomainError("users.email_not_verified", "L'adresse email n'a pas encore été vérifiée.");

    private sealed record AccountLockedError()
        : DomainError("users.account_locked", "Le compte est temporairement verrouillé. Réessayez plus tard.");

    private sealed record InvalidVerificationTokenError()
        : DomainError("users.invalid_verification_token", "Le lien de vérification est invalide ou expiré.");

    private sealed record InvalidPasswordResetTokenError()
        : DomainError("users.invalid_password_reset_token", "Le lien de réinitialisation est invalide ou expiré.");

    private sealed record InvalidRefreshTokenError()
        : DomainError("users.invalid_refresh_token", "La session est invalide ou expirée. Veuillez vous reconnecter.");

    private sealed record TwoFactorAlreadyEnabledError()
        : DomainError("users.two_factor_already_enabled", "La double authentification est déjà activée.");

    private sealed record TwoFactorNotEnabledError()
        : DomainError("users.two_factor_not_enabled", "La double authentification n'est pas activée.");

    private sealed record TwoFactorSetupNotStartedError()
        : DomainError("users.two_factor_setup_not_started", "Aucune configuration de double authentification en cours.");

    private sealed record InvalidTwoFactorCodeError()
        : DomainError("users.invalid_two_factor_code", "Code de double authentification invalide.");

    private sealed record InvalidTwoFactorChallengeError()
        : DomainError("users.invalid_two_factor_challenge", "La session de double authentification est invalide ou expirée.");
}
