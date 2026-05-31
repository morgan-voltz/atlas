namespace Atlas.Web.Client.Services;

/// <summary>Issue d'une tentative de connexion, traduite en message par la page Connexion.</summary>
public enum LoginStatus
{
    Success,
    TwoFactorRequired,
    InvalidCredentials,
    EmailNotVerified,
    AccountLocked,
    Unavailable,
}

/// <summary>Résultat de <c>LoginAsync</c> : statut + jeton de défi 2FA le cas échéant.</summary>
public sealed record LoginResult(LoginStatus Status, string? ChallengeToken = null);

/// <summary>
/// Résultat léger d'un appel API pour piloter les états d'écran (chargement / vide / erreur).
/// <see cref="ErrorCode"/> nul = succès ; sinon, code métier stable (ex. <c>inpi.not_connected</c>,
/// <c>unauthorized</c>, <c>network</c>) que l'UI mappe vers un message.
/// </summary>
public sealed record ApiResult<T>(T? Value, string? ErrorCode)
{
    public bool IsSuccess => ErrorCode is null;

    public static ApiResult<T> Ok(T value) => new(value, null);

    public static ApiResult<T> Fail(string code) => new(default, code);
}

/// <summary>Variante sans valeur, pour les appels qui ne renvoient pas de corps (POST/DELETE → 204).</summary>
public sealed record ApiResult(string? ErrorCode)
{
    public bool IsSuccess => ErrorCode is null;

    public static ApiResult Ok() => new((string?)null);

    public static ApiResult Fail(string code) => new(code);
}
