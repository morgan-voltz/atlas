using Atlas.Domain.Common;
using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

/// <summary>
/// « Like » communautaire d'un utilisateur sur un <see cref="VeillePack"/> publié (F-049 marketplace).
/// Sert d'unité de classement social du catalogue communautaire. Un user ne peut liker qu'une fois par
/// pack (index unique <c>(UserId, VeillePackId)</c>).
/// </summary>
public sealed class VeillePackLike : Entity<VeillePackLikeId>
{
    private VeillePackLike()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private VeillePackLike(VeillePackLikeId id, VeillePackId veillePackId, UserId userId, DateTimeOffset createdAt)
        : base(id)
    {
        VeillePackId = veillePackId;
        UserId = userId;
        CreatedAt = createdAt;
    }

    public VeillePackId VeillePackId { get; private set; }

    public UserId UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static VeillePackLike Create(VeillePackId veillePackId, UserId userId, DateTimeOffset now) =>
        new(VeillePackLikeId.New(), veillePackId, userId, now);
}
