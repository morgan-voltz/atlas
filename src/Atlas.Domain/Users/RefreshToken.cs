using Atlas.Domain.Common;

namespace Atlas.Domain.Users;

/// <summary>
/// Jeton de rafraîchissement persisté côté serveur (seul le hash est stocké, jamais le clair).
/// Support de la rotation et de la révocation (déconnexion, logout).
/// </summary>
public sealed class RefreshToken : Entity<RefreshTokenId>
{
    private RefreshToken()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private RefreshToken(
        RefreshTokenId id,
        UserId userId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public UserId UserId { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public static RefreshToken Issue(
        RefreshTokenId id,
        UserId userId,
        string tokenHash,
        DateTimeOffset now,
        TimeSpan lifetime) => new(id, userId, tokenHash, now, now.Add(lifetime));

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && now < ExpiresAt;

    public void Revoke(DateTimeOffset now)
    {
        if (RevokedAt is null)
        {
            RevokedAt = now;
        }
    }
}
