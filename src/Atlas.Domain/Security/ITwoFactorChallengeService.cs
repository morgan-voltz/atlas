using Atlas.Domain.Users;

namespace Atlas.Domain.Security;

/// <summary>
/// Émet et valide un jeton de défi 2FA à courte durée de vie : preuve que le mot de passe a été
/// validé et qu'il ne reste que l'étape TOTP avant l'émission des jetons d'accès.
/// </summary>
public interface ITwoFactorChallengeService
{
    string IssueChallenge(UserId userId);

    Task<UserId?> ValidateChallengeAsync(string challengeToken, CancellationToken ct = default);
}
