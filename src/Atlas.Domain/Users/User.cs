using System.Security.Cryptography;
using System.Text;
using Atlas.Domain.Common;
using Atlas.Domain.Users.Events;
using Atlas.Shared.Result;

namespace Atlas.Domain.Users;

public sealed class User : Entity<UserId>
{
    private string? _emailVerificationTokenHash;
    private DateTimeOffset? _emailVerificationTokenExpiresAt;

    private User()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private User(UserId id, EmailAddress email, PasswordHash passwordHash, DateTimeOffset createdAt)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        Status = UserStatus.PendingEmailVerification;
        CreatedAt = createdAt;
    }

    public EmailAddress Email { get; private set; }

    public PasswordHash PasswordHash { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? EmailVerifiedAt { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTimeOffset? LockoutEndsAt { get; private set; }

    public bool TwoFactorEnabled { get; private set; }

    /// <summary>Secret TOTP actif, chiffré au repos (jamais en clair sur l'entité).</summary>
    public string? TwoFactorSecret { get; private set; }

    /// <summary>Secret TOTP en cours de configuration (chiffré), non encore confirmé par un code.</summary>
    public string? PendingTwoFactorSecret { get; private set; }

    /// <summary>
    /// Crée un nouvel utilisateur en attente de vérification d'email.
    /// Le token de vérification est fourni déjà hashé (le clair est envoyé par email, jamais persisté).
    /// </summary>
    public static User Register(
        UserId id,
        EmailAddress email,
        PasswordHash passwordHash,
        string emailVerificationTokenHash,
        DateTimeOffset now,
        TimeSpan verificationTokenLifetime)
    {
        var user = new User(id, email, passwordHash, now)
        {
            _emailVerificationTokenHash = emailVerificationTokenHash,
            _emailVerificationTokenExpiresAt = now.Add(verificationTokenLifetime),
        };

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(id, email, now));
        return user;
    }

    /// <summary>
    /// Confirme l'email à partir du hash du token fourni par le porteur du lien. Idempotent si déjà actif.
    /// </summary>
    public Result ConfirmEmail(string providedTokenHash, DateTimeOffset now)
    {
        if (Status == UserStatus.Active)
        {
            return Result.Ok();
        }

        if (_emailVerificationTokenHash is null
            || _emailVerificationTokenExpiresAt is null
            || now > _emailVerificationTokenExpiresAt.Value
            || !FixedTimeEquals(_emailVerificationTokenHash, providedTokenHash))
        {
            return Result.Fail(UserErrors.InvalidOrExpiredVerificationToken);
        }

        Status = UserStatus.Active;
        EmailVerifiedAt = now;
        _emailVerificationTokenHash = null;
        _emailVerificationTokenExpiresAt = null;
        return Result.Ok();
    }

    /// <summary>
    /// Vérifie que l'utilisateur est en état de s'authentifier (email vérifié, non verrouillé, non suspendu).
    /// </summary>
    public Result EnsureCanAuthenticate(DateTimeOffset now)
    {
        if (LockoutEndsAt is not null && now < LockoutEndsAt.Value)
        {
            return Result.Fail(UserErrors.AccountLocked);
        }

        if (Status == UserStatus.Suspended)
        {
            return Result.Fail(UserErrors.AccountLocked);
        }

        if (Status != UserStatus.Active)
        {
            return Result.Fail(UserErrors.EmailNotVerified);
        }

        return Result.Ok();
    }

    public void RegisterFailedLogin(DateTimeOffset now, int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEndsAt = now.Add(lockoutDuration);
            FailedLoginAttempts = 0;
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutEndsAt = null;
    }

    /// <summary>
    /// Démarre la configuration du 2FA en mémorisant le secret (déjà chiffré) en attente de confirmation.
    /// </summary>
    public Result BeginTwoFactorSetup(string encryptedSecret)
    {
        if (TwoFactorEnabled)
        {
            return Result.Fail(UserErrors.TwoFactorAlreadyEnabled);
        }

        PendingTwoFactorSecret = encryptedSecret;
        return Result.Ok();
    }

    /// <summary>
    /// Active le 2FA après vérification d'un code par l'appelant (handler). Promeut le secret en attente.
    /// </summary>
    public Result EnableTwoFactor()
    {
        if (TwoFactorEnabled)
        {
            return Result.Fail(UserErrors.TwoFactorAlreadyEnabled);
        }

        if (PendingTwoFactorSecret is null)
        {
            return Result.Fail(UserErrors.TwoFactorSetupNotStarted);
        }

        TwoFactorSecret = PendingTwoFactorSecret;
        PendingTwoFactorSecret = null;
        TwoFactorEnabled = true;
        return Result.Ok();
    }

    public Result DisableTwoFactor()
    {
        if (!TwoFactorEnabled)
        {
            return Result.Fail(UserErrors.TwoFactorNotEnabled);
        }

        TwoFactorEnabled = false;
        TwoFactorSecret = null;
        PendingTwoFactorSecret = null;
        return Result.Ok();
    }

    private static bool FixedTimeEquals(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(left),
            Encoding.UTF8.GetBytes(right));
}
