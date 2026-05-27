using Atlas.Domain.Users;

namespace Atlas.Domain.Security;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>
/// Émet un JWT d'accès signé (RS256/EdDSA, cf. docs/04-securite-rgpd.md §5.4.3) pour un utilisateur donné.
/// </summary>
public interface IJwtIssuer
{
    AccessToken Issue(UserId userId, EmailAddress email);
}
