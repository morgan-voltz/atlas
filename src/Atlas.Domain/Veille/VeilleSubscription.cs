using Atlas.Domain.Common;
using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

/// <summary>
/// Abonnement d'un utilisateur à une <see cref="FeedSource"/> (partagée). Cf. doc 08 §9.5, F-043.
/// </summary>
public sealed class VeilleSubscription : Entity<VeilleSubscriptionId>
{
    private VeilleSubscription()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private VeilleSubscription(VeilleSubscriptionId id, UserId userId, FeedSourceId sourceId, DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        SourceId = sourceId;
        CreatedAt = createdAt;
    }

    public UserId UserId { get; private set; }

    public FeedSourceId SourceId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static VeilleSubscription Create(UserId userId, FeedSourceId sourceId, DateTimeOffset now) =>
        new(VeilleSubscriptionId.New(), userId, sourceId, now);
}
