namespace Atlas.Application.Common;

/// <summary>
/// Politique d'authentification (durées de vie des jetons, verrouillage). Bindée depuis la configuration.
/// </summary>
public sealed class AuthSettings
{
    public TimeSpan EmailVerificationTokenLifetime { get; init; } = TimeSpan.FromHours(24);

    public TimeSpan RefreshTokenLifetime { get; init; } = TimeSpan.FromDays(30);

    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    public int MaxFailedLoginAttempts { get; init; } = 5;

    public TimeSpan LockoutDuration { get; init; } = TimeSpan.FromMinutes(15);
}
