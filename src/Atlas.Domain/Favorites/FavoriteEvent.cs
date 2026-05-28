using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

/// <summary>
/// Événement chronologique attaché à une entreprise favorite d'un utilisateur (F-047 volet 2).
/// Apparaît dans la timeline aux côtés des items RSS, trié par <see cref="OccurredAt"/>.
/// Produit notamment par F-019 (changement RNE) et F-048 (annonce BODACC).
/// </summary>
public sealed class FavoriteEvent : Entity<FavoriteEventId>
{
    public const int MaxTitleLength = 256;
    public const int MaxSummaryLength = 2000;
    public const int MaxExternalIdLength = 128;

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
        DateTimeOffset occurredAt,
        string? externalId)
        : base(id)
    {
        UserId = userId;
        Siren = siren;
        Type = type;
        Title = title;
        Summary = summary;
        OccurredAt = occurredAt;
        ExternalId = externalId;
    }

    public UserId UserId { get; private set; }

    public Siren Siren { get; private set; }

    public FavoriteEventType Type { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Summary { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>
    /// Identifiant externe de l'événement source quand applicable (F-048 BODACC : numéro d'annonce).
    /// Utilisé pour la déduplication via index unique partiel sur <c>(UserId, ExternalId)</c>.
    /// <c>null</c> pour les événements internes (F-019) qui n'ont pas besoin de dédup externe.
    /// </summary>
    public string? ExternalId { get; private set; }

    public static FavoriteEvent Record(
        UserId userId,
        Siren siren,
        FavoriteEventType type,
        string title,
        string? summary,
        DateTimeOffset now,
        string? externalId = null)
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

        string? normalizedExternalId = externalId?.Trim();
        if (!string.IsNullOrEmpty(normalizedExternalId) && normalizedExternalId.Length > MaxExternalIdLength)
        {
            normalizedExternalId = normalizedExternalId[..MaxExternalIdLength];
        }

        return new FavoriteEvent(
            FavoriteEventId.New(),
            userId,
            siren,
            type,
            normalizedTitle,
            normalizedSummary,
            now,
            normalizedExternalId);
    }
}
