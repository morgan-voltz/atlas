using Atlas.Domain.Common;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

/// <summary>
/// Marque suivie en favori par un utilisateur (F-018). Équivalent <see cref="CompanyFavorite"/>
/// pour la PI ; pose les bases d'un tableau de bord IP et d'alertes futures.
/// </summary>
public sealed class TrademarkFavorite : Entity<TrademarkFavoriteId>
{
    public const int MaxNameSnapshotLength = 256;

    private TrademarkFavorite()
        : base(default)
    {
        // Réhydratation EF Core.
    }

    private TrademarkFavorite(
        TrademarkFavoriteId id,
        UserId userId,
        DepositNumber depositNumber,
        string? nameSnapshot,
        DateTimeOffset addedAt)
        : base(id)
    {
        UserId = userId;
        DepositNumber = depositNumber;
        NameSnapshot = nameSnapshot;
        AddedAt = addedAt;
    }

    public UserId UserId { get; private set; }

    public DepositNumber DepositNumber { get; private set; }

    /// <summary>Dénomination marque au moment du marquage, pour affichage sans re-fetch INPI.</summary>
    public string? NameSnapshot { get; private set; }

    public DateTimeOffset AddedAt { get; private set; }

    public static TrademarkFavorite Mark(UserId userId, DepositNumber depositNumber, string? nameSnapshot, DateTimeOffset now)
    {
        string? normalized = nameSnapshot?.Trim();
        if (!string.IsNullOrEmpty(normalized) && normalized.Length > MaxNameSnapshotLength)
        {
            normalized = normalized[..MaxNameSnapshotLength];
        }

        return new TrademarkFavorite(TrademarkFavoriteId.New(), userId, depositNumber, normalized, now);
    }
}
