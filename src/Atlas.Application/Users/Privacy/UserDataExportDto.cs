namespace Atlas.Application.Users.Privacy;

/// <summary>
/// Export des données personnelles d'un utilisateur (RGPD art. 20, portabilité). N'inclut JAMAIS de
/// données sensibles : ni hash de mot de passe, ni identifiants/secrets INPI, ni secret TOTP.
/// </summary>
public sealed record UserDataExportDto(
    Guid UserId,
    string Email,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EmailVerifiedAt,
    bool TwoFactorEnabled,
    InpiConnectionExportDto? InpiConnection,
    IReadOnlyList<SearchHistoryExportDto> SearchHistory);

public sealed record InpiConnectionExportDto(string Status, DateTimeOffset? LastTestedAt);

public sealed record SearchHistoryExportDto(string Type, string Query, DateTimeOffset CreatedAt);
