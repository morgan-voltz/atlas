using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

/// <summary>
/// Événement chronologique attaché à une entreprise favorite d'un utilisateur (F-047 volet 2).
/// Apparaît dans la timeline aux côtés des items RSS, trié par <see cref="OccurredAt"/>.
/// Produit notamment par F-019 (à chaque détection de changement RNE).
/// </summary>
public sealed class FavoriteEvent : Entity<FavoriteEventId>
{
    public const int MaxTitleLength = 256;
    public const int MaxSummaryLength = 2000;

    private FavoriteEvent()
        : base(default)
    {
        // Réhydratation EF Core.
    }

    private FavoriteEvent(
        FavoriteEventId id,
        UserId userId,
        Siren siren,
        FavoriteEventType type,
        string title,
        string? summary,
        DateTimeOffset occurredAt)
        : base(id)
    {
        UserId = userId;
        Siren = siren;
        Type = type;
        Title = title;
        Summary = summary;
        OccurredAt = occurredAt;
    }

    public UserId UserId { get; private set; }

    public Siren Siren { get; private set; }

    public FavoriteEventType Type { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Summary { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public static FavoriteEvent Record(
        UserId userId,
        Siren siren,
        FavoriteEventType type,
        string title,
        string? summary,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        string normalizedTitle = title.Trim();
        if (normalizedTitle.Length > MaxTitleLength)
        {
            normalizedTitle = normalizedTitle[..MaxTitleLength];
        }

        string? normalizedSummary = summary?.Trim();
        if (!string.IsNullOrEmpty(normalizedSummary) && normalizedSummary.Length > MaxSummaryLength)
        {
            normalizedSummary = normalizedSummary[..MaxSummaryLength];
        }

        return new FavoriteEvent(
            FavoriteEventId.New(),
            userId,
            siren,
            type,
            normalizedTitle,
            normalizedSummary,
            now);
    }
}
