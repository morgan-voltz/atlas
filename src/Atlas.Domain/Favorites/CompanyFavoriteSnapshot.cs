using System.Security.Cryptography;
using System.Text;
using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Users;

namespace Atlas.Domain.Favorites;

/// <summary>
/// Cliché d'une entreprise favorite à un instant T, utilisé pour détecter les changements
/// entre deux runs du job d'alerte (F-019). Un seul snapshot vivant par (User, Siren) — le
/// snapshot précédent est remplacé à chaque cycle.
/// </summary>
public sealed class CompanyFavoriteSnapshot : Entity<CompanyFavoriteSnapshotId>
{
    public const int MaxFieldLength = 512;

    private CompanyFavoriteSnapshot()
        : base(default)
    {
        // Réhydratation EF Core.
    }

    private CompanyFavoriteSnapshot(
        CompanyFavoriteSnapshotId id,
        UserId userId,
        Siren siren,
        string? denomination,
        string? formeJuridique,
        string? nafCode,
        string? adresseLine,
        string dirigeantsHash,
        DateTimeOffset capturedAt)
        : base(id)
    {
        UserId = userId;
        Siren = siren;
        Denomination = denomination;
        FormeJuridique = formeJuridique;
        NafCode = nafCode;
        AdresseLine = adresseLine;
        DirigeantsHash = dirigeantsHash;
        CapturedAt = capturedAt;
    }

    public UserId UserId { get; private set; }

    public Siren Siren { get; private set; }

    public string? Denomination { get; private set; }

    public string? FormeJuridique { get; private set; }

    public string? NafCode { get; private set; }

    public string? AdresseLine { get; private set; }

    /// <summary>Hash SHA-256 de la liste des dirigeants (nom|qualité, ordonnés) pour détecter les changements de gouvernance.</summary>
    public string DirigeantsHash { get; private set; } = null!;

    public DateTimeOffset CapturedAt { get; private set; }

    public static CompanyFavoriteSnapshot Capture(UserId userId, UniteLegale uniteLegale, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(uniteLegale);

        return new CompanyFavoriteSnapshot(
            CompanyFavoriteSnapshotId.New(),
            userId,
            uniteLegale.Siren,
            Truncate(uniteLegale.Denomination),
            Truncate(uniteLegale.FormeJuridique),
            Truncate(uniteLegale.ActivitePrincipale?.Code),
            Truncate(uniteLegale.Adresse?.Line),
            HashDirigeants(uniteLegale.Dirigeants),
            now);
    }

    /// <summary>Calcule la liste des changements observés entre ce snapshot (ancien) et la fiche actuelle.</summary>
    public IReadOnlyList<CompanyFavoriteChange> DiffWith(UniteLegale current)
    {
        ArgumentNullException.ThrowIfNull(current);

        var changes = new List<CompanyFavoriteChange>(5);

        AppendIfChanged(changes, "Denomination", Denomination, current.Denomination);
        AppendIfChanged(changes, "FormeJuridique", FormeJuridique, current.FormeJuridique);
        AppendIfChanged(changes, "NafCode", NafCode, current.ActivitePrincipale?.Code);
        AppendIfChanged(changes, "Adresse", AdresseLine, current.Adresse?.Line);

        string newDirigeantsHash = HashDirigeants(current.Dirigeants);
        if (!string.Equals(DirigeantsHash, newDirigeantsHash, StringComparison.Ordinal))
        {
            changes.Add(new CompanyFavoriteChange("Dirigeants", DirigeantsHash, newDirigeantsHash));
        }

        return changes;
    }

    private static void AppendIfChanged(List<CompanyFavoriteChange> changes, string field, string? oldValue, string? newValue)
    {
        if (!string.Equals(oldValue, Truncate(newValue), StringComparison.Ordinal))
        {
            changes.Add(new CompanyFavoriteChange(field, oldValue, Truncate(newValue)));
        }
    }

    private static string? Truncate(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return value.Length > MaxFieldLength ? value[..MaxFieldLength] : value;
    }

    private static string HashDirigeants(IReadOnlyList<Dirigeant> dirigeants)
    {
        if (dirigeants is null || dirigeants.Count == 0)
        {
            return string.Empty;
        }

        var ordered = dirigeants
            .Select(d => $"{d.Nom?.Trim() ?? string.Empty}|{d.Qualite?.Trim() ?? string.Empty}")
            .OrderBy(value => value, StringComparer.Ordinal);

        byte[] bytes = Encoding.UTF8.GetBytes(string.Join("\n", ordered));
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
