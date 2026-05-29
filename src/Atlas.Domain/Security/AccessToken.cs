namespace Atlas.Domain.Security;

/// <summary>
/// Jeton JWT d'accès émis par <see cref="IJwtIssuer"/> (RS256 / EdDSA, cf. docs/04-securite-rgpd.md §5.4.3).
/// </summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
