using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

/// <summary>
/// Entreprise marquée comme favorite par un utilisateur (F-017). Pose les bases du
/// tableau de bord et des alertes (F-019), et de la timeline mixte veille+favoris (F-047).
/// </summary>
public sealed class CompanyFavorite : Entity<CompanyFavoriteId>
{
    public const int MaxNameSnapshotLength = 256;

    private CompanyFavorite()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private CompanyFavorite(
        CompanyFavoriteId id,
        UserId userId,
        Siren siren,
        string? nameSnapshot,
        DateTimeOffset addedAt)
        : base(id)
    {
        UserId = userId;
        Siren = siren;
        NameSnapshot = nameSnapshot;
        AddedAt = addedAt;
    }

    public UserId UserId { get; private set; }

    public Siren Siren { get; private set; }

    /// <summary>Dénomination au moment du marquage, pour affichage de la liste sans re-fetch RNE.</summary>
    public string? NameSnapshot { get; private set; }

    public DateTimeOffset AddedAt { get; private set; }

    public static CompanyFavorite Mark(UserId userId, Siren siren, string? nameSnapshot, DateTimeOffset now)
    {
        string? normalized = nameSnapshot?.Trim();
        if (!string.IsNullOrEmpty(normalized) && normalized.Length > MaxNameSnapshotLength)
        {
            normalized = normalized[..MaxNameSnapshotLength];
        }

        return new CompanyFavorite(CompanyFavoriteId.New(), userId, siren, normalized, now);
    }
}
