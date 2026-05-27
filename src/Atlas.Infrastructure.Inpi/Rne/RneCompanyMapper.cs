using System.Globalization;
using Atlas.Domain.Companies;
using Atlas.Infrastructure.Inpi.Rne.DTOs;
using Atlas.Shared.Result;

namespace Atlas.Infrastructure.Inpi.Rne;

/// <summary>
/// Mappe la réponse RNE vers le domaine. ⚠️ Best-effort tolérant aux champs absents :
/// les chemins JSON sont à valider contre le schéma réel du RNE. Le mapping des dirigeants
/// (structure <c>composition.pouvoirs</c>, très variable) est laissé en TODO et renvoie une liste vide.
/// </summary>
internal static class RneCompanyMapper
{
    public static UniteLegale Map(Siren siren, RneCompanyResponse response)
    {
        RneContent? content = response.Formality?.Content;
        RneEntreprise? entreprise =
            content?.PersonneMorale?.Identite?.Entreprise
            ?? content?.PersonnePhysique?.Identite?.Entreprise;

        RneAdresse? adresse =
            content?.PersonneMorale?.AdresseEntreprise?.Adresse
            ?? content?.PersonnePhysique?.AdresseEntreprise?.Adresse;

        string denomination = string.IsNullOrWhiteSpace(entreprise?.Denomination)
            ? "(dénomination non disponible)"
            : entreprise!.Denomination!;

        Naf? naf = string.IsNullOrWhiteSpace(entreprise?.CodeApe)
            ? null
            : new Naf(entreprise!.CodeApe!, Label: null);

        bool isDiffusible = !string.Equals(response.Formality?.DiffusionInsee, "N", StringComparison.OrdinalIgnoreCase);

        return new UniteLegale(
            Siren: siren,
            Denomination: denomination,
            FormeJuridique: entreprise?.FormeJuridique,
            ActivitePrincipale: naf,
            Adresse: MapAddress(adresse),
            DateCreation: ParseDate(entreprise?.DateImmatriculation),
            IsDiffusible: isDiffusible,
            Dirigeants: []); // TODO F-004+: mapper composition.pouvoirs une fois le schéma RNE validé.
    }

    /// <summary>Mappe un item de résultat de recherche vers un résumé. Retourne null si le SIREN est inexploitable.</summary>
    public static CompanySummary? MapSummary(RneCompanyResponse response)
    {
        Result<Siren> siren = Siren.Create(response.Siren);
        if (siren.IsFailure)
        {
            return null;
        }

        RneContent? content = response.Formality?.Content;
        RneEntreprise? entreprise =
            content?.PersonneMorale?.Identite?.Entreprise
            ?? content?.PersonnePhysique?.Identite?.Entreprise;
        RneAdresse? adresse =
            content?.PersonneMorale?.AdresseEntreprise?.Adresse
            ?? content?.PersonnePhysique?.AdresseEntreprise?.Adresse;

        string denomination = string.IsNullOrWhiteSpace(entreprise?.Denomination)
            ? "(dénomination non disponible)"
            : entreprise!.Denomination!;

        Naf? naf = string.IsNullOrWhiteSpace(entreprise?.CodeApe)
            ? null
            : new Naf(entreprise!.CodeApe!, Label: null);

        return new CompanySummary(siren.Value!, denomination, adresse?.Commune, naf);
    }

    private static Address? MapAddress(RneAdresse? adresse)
    {
        if (adresse is null)
        {
            return null;
        }

        string? line = string.Join(' ', new[] { adresse.NumVoie, adresse.TypeVoie, adresse.Voie }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

        return new Address(
            Line: string.IsNullOrWhiteSpace(line) ? null : line,
            PostalCode: adresse.CodePostal,
            City: adresse.Commune,
            Country: adresse.Pays);
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)
            ? date
            : null;
}
