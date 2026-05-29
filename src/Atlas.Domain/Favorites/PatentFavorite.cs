using Atlas.Domain.Common;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

/// <summary>
/// Brevet suivi en favori par un utilisateur (F-018). Référencé par
/// <see cref="PublicationNumber"/> (normalisé majuscules sans espace, cf. F-015).
/// </summary>
public sealed class PatentFavorite : Entity<PatentFavoriteId>
{
    public const int MaxTitleSnapshotLength = 256;

    private PatentFavorite()
        : base(default)
    {
        // Réhydratation EF Core.
    }

    private PatentFavorite(
        PatentFavoriteId id,
        UserId userId,
        PublicationNumber publicationNumber,
        string? titleSnapshot,
        DateTimeOffset addedAt)
        : base(id)
    {
        UserId = userId;
        PublicationNumber = publicationNumber;
        TitleSnapshot = titleSnapshot;
        AddedAt = addedAt;
    }

    public UserId UserId { get; private set; }

    public PublicationNumber PublicationNumber { get; private set; }

    /// <summary>Titre du brevet au moment du marquage, pour affichage sans re-fetch INPI.</summary>
    public string? TitleSnapshot { get; private set; }

    public DateTimeOffset AddedAt { get; private set; }

    public static PatentFavorite Mark(UserId userId, PublicationNumber publicationNumber, string? titleSnapshot, DateTimeOffset now)
    {
        string? normalized = titleSnapshot?.Trim();
        if (!string.IsNullOrEmpty(normalized) && normalized.Length > MaxTitleSnapshotLength)
        {
            normalized = normalized[..MaxTitleSnapshotLength];
        }

        return new PatentFavorite(PatentFavoriteId.New(), userId, publicationNumber, normalized, now);
    }
}
