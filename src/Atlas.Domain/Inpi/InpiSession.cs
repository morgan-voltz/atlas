namespace Atlas.Domain.Inpi;

/// <summary>
/// Session INPI obtenue après authentification : JWT Bearer du RNE et sa date d'expiration.
/// Jamais persistée en base ; mise en cache court terme par l'adapter.
/// </summary>
public sealed record InpiSession(string AccessToken, DateTimeOffset ExpiresAt);
