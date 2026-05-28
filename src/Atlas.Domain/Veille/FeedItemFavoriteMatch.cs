using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

/// <summary>
/// Lien entre un <see cref="FeedItem"/> et une entreprise favorite d'un utilisateur (F-047).
/// Calculé au polling : si le titre ou le résumé d'un item RSS contient le nom d'une
/// entreprise favorite (case-insensitive, mot entier), on enregistre la mention.
/// La timeline (F-044) join cette table pour afficher « mentionne {Entreprise} » sur chaque item.
/// </summary>
public sealed class FeedItemFavoriteMatch : Entity<FeedItemFavoriteMatchId>
{
    public const int MaxMatchedNameLength = 256;

    private FeedItemFavoriteMatch()
        : base(default)
    {
        // Réhydratation EF Core.
    }

    private FeedItemFavoriteMatch(
        FeedItemFavoriteMatchId id,
        FeedItemId feedItemId,
        UserId userId,
        Siren siren,
        string matchedName,
        DateTimeOffset matchedAt)
        : base(id)
    {
        FeedItemId = feedItemId;
        UserId = userId;
        Siren = siren;
        MatchedName = matchedName;
        MatchedAt = matchedAt;
    }

    public FeedItemId FeedItemId { get; private set; }

    public UserId UserId { get; private set; }

    public Siren Siren { get; private set; }

    /// <summary>Nom de l'entreprise tel qu'il était au moment du match (snapshot du favori).</summary>
    public string MatchedName { get; private set; } = null!;

    public DateTimeOffset MatchedAt { get; private set; }

    public static FeedItemFavoriteMatch Create(
        FeedItemId feedItemId,
        UserId userId,
        Siren siren,
        string matchedName,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(matchedName);

        string normalized = matchedName.Trim();
        if (normalized.Length > MaxMatchedNameLength)
        {
            normalized = normalized[..MaxMatchedNameLength];
        }

        return new FeedItemFavoriteMatch(
            FeedItemFavoriteMatchId.New(),
            feedItemId,
            userId,
            siren,
            normalized,
            now);
    }
}
