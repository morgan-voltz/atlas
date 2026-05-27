namespace Atlas.Application.Users;

/// <summary>
/// Résultat d'une tentative de connexion : soit les jetons d'accès (pas de 2FA), soit un défi 2FA
/// (<see cref="TwoFactorRequired"/> vrai) avec un jeton de défi à présenter à /auth/2fa/verify.
/// </summary>
public sealed record LoginResultDto(
    bool TwoFactorRequired,
    AuthTokensDto? Tokens,
    string? TwoFactorChallengeToken);
