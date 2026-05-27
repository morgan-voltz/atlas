using Atlas.Domain.Common;
using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

/// <summary>
/// État d'un <see cref="FeedItem"/> pour un utilisateur dans sa timeline (F-044) : lu, favori, archivé.
/// L'absence de ligne vaut « non-lu, non-favori, non-archivé » : une instance n'est créée qu'au premier
/// marquage. Cf. doc 08 §9.x.
/// </summary>
public sealed class FeedItemUserState : Entity<FeedItemUserStateId>
{
    private FeedItemUserState()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private FeedItemUserState(FeedItemUserStateId id, UserId userId, FeedItemId feedItemId, DateTimeOffset now)
        : base(id)
    {
        UserId = userId;
        FeedItemId = feedItemId;
        UpdatedAt = now;
    }

    public UserId UserId { get; private set; }

    public FeedItemId FeedItemId { get; private set; }

    public bool IsRead { get; private set; }

    public bool IsFavorite { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static FeedItemUserState Create(UserId userId, FeedItemId feedItemId, DateTimeOffset now) =>
        new(FeedItemUserStateId.New(), userId, feedItemId, now);

    public void SetRead(bool value, DateTimeOffset now)
    {
        IsRead = value;
        UpdatedAt = now;
    }

    public void SetFavorite(bool value, DateTimeOffset now)
    {
        IsFavorite = value;
        UpdatedAt = now;
    }

    public void SetArchived(bool value, DateTimeOffset now)
    {
        IsArchived = value;
        UpdatedAt = now;
    }
}
