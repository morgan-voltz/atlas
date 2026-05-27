using System.Globalization;
using System.Text.Json;
using Atlas.Domain.Companies;
using Atlas.Shared.Result;

namespace Atlas.Infrastructure.Inpi.Rne;

/// <summary>
/// Mappe la réponse JSON du RNE vers le domaine, par navigation défensive : pour chaque champ, plusieurs
/// chemins candidats sont essayés (chemins de la doc technique INPI v4.0 en primaire, variantes en repli),
/// afin de rester robuste à la forme exacte renvoyée. ⚠️ À confirmer par un appel authentifié réel.
/// Structure documentée : formality.content.personneMorale.{identite.{denomination, formeJuridique,
/// dateImmatriculation}, entreprise.{activitePrincipale.codeNAF, adresseEntreprise}, composition,
/// indicateurDiffusionINSEE}.
/// </summary>
internal static class RneCompanyMapper
{
    public static UniteLegale Map(Siren siren, JsonElement root)
    {
        JsonElement entity = ResolveEntity(root);

        string denomination = FirstString(
            entity,
            ["identite", "denomination"],
            ["identite", "entreprise", "denomination"],
            ["identite", "entrepreneur", "descriptionPersonne", "nom"])
            ?? "(dénomination non disponible)";

        string? formeJuridique = FirstString(
            entity,
            ["identite", "formeJuridique"],
            ["identite", "entreprise", "formeJuridique"]);

        string? codeNaf = FirstString(
            entity,
            ["entreprise", "activitePrincipale", "codeNAF"],
            ["identite", "entreprise", "activitePrincipale", "codeNAF"],
            ["identite", "entreprise", "codeApe"]);

        string? dateImmatriculation = FirstString(
            entity,
            ["identite", "dateImmatriculation"],
            ["identite", "entreprise", "dateImmatriculation"]);

        bool isDiffusible = !string.Equals(
            FirstString(entity, ["indicateurDiffusionINSEE"]) ?? FirstString(root, ["formality", "diffusionINSEE"]),
            "N",
            StringComparison.OrdinalIgnoreCase);

        return new UniteLegale(
            Siren: siren,
            Denomination: denomination,
            FormeJuridique: formeJuridique,
            ActivitePrincipale: string.IsNullOrWhiteSpace(codeNaf) ? null : new Naf(codeNaf!, Label: null),
            Adresse: MapAddress(entity),
            DateCreation: ParseDate(dateImmatriculation),
            IsDiffusible: isDiffusible,
            Dirigeants: MapDirigeants(entity));
    }

    public static CompanySummary? MapSummary(JsonElement root)
    {
        Result<Siren> siren = Siren.Create(FirstString(root, ["siren"]));
        if (siren.IsFailure)
        {
            return null;
        }

        JsonElement entity = ResolveEntity(root);

        string denomination = FirstString(
            entity,
            ["identite", "denomination"],
            ["identite", "entreprise", "denomination"])
            ?? "(dénomination non disponible)";

        string? ville = FirstString(
            entity,
            ["entreprise", "adresseEntreprise", "commune"],
            ["adresseEntreprise", "adresse", "commune"],
            ["adresseEntreprise", "commune"]);

        string? codeNaf = FirstString(
            entity,
            ["entreprise", "activitePrincipale", "codeNAF"],
            ["identite", "entreprise", "codeApe"]);

        return new CompanySummary(
            siren.Value!,
            denomination,
            ville,
            string.IsNullOrWhiteSpace(codeNaf) ? null : new Naf(codeNaf!, Label: null));
    }

    /// <summary>Renvoie l'objet personneMorale ou, à défaut, personnePhysique sous formality.content.</summary>
    private static JsonElement ResolveEntity(JsonElement root)
    {
        JsonElement? content = Navigate(root, "formality", "content");
        if (content is null)
        {
            return root;
        }

        return Navigate(content.Value, "personneMorale")
            ?? Navigate(content.Value, "personnePhysique")
            ?? content.Value;
    }

    private static Address? MapAddress(JsonElement entity)
    {
        JsonElement? address =
            Navigate(entity, "entreprise", "adresseEntreprise")
            ?? Navigate(entity, "adresseEntreprise", "adresse")
            ?? Navigate(entity, "adresseEntreprise");
        if (address is null)
        {
            return null;
        }

        JsonElement value = address.Value;
        string line = string.Join(
            ' ',
            new[] { StringOrNull(value, "numVoie"), StringOrNull(value, "typeVoie"), StringOrNull(value, "voie") }
                .Where(part => !string.IsNullOrWhiteSpace(part)));

        return new Address(
            Line: string.IsNullOrWhiteSpace(line) ? null : line,
            PostalCode: StringOrNull(value, "codePostal"),
            City: StringOrNull(value, "commune"),
            Country: StringOrNull(value, "pays"));
    }

    private static List<Dirigeant> MapDirigeants(JsonElement entity)
    {
        JsonElement? composition = Navigate(entity, "composition");
        if (composition is null)
        {
            return [];
        }

        JsonElement? list = Navigate(composition.Value, "pouvoirs") ?? Navigate(composition.Value, "dirigeants");
        if (list is not { ValueKind: JsonValueKind.Array })
        {
            return [];
        }

        var dirigeants = new List<Dirigeant>();
        foreach (JsonElement item in list.Value.EnumerateArray())
        {
            string? nom =
                FirstString(item, ["individu", "descriptionPersonne", "nom"])
                ?? FirstString(item, ["entreprise", "denomination"])
                ?? FirstString(item, ["descriptionPersonne", "nom"])
                ?? FirstString(item, ["nom"]);

            if (string.IsNullOrWhiteSpace(nom))
            {
                continue;
            }

            string? qualite = FirstString(item, ["roleEntreprise"]) ?? FirstString(item, ["qualite"]);
            dirigeants.Add(new Dirigeant(nom!, qualite));
        }

        return dirigeants;
    }

    private static string? FirstString(JsonElement root, params string[][] paths)
    {
        foreach (string[] path in paths)
        {
            if (Navigate(root, path) is { ValueKind: JsonValueKind.String } value)
            {
                return value.GetString();
            }
        }

        return null;
    }

    private static string? StringOrNull(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(property, out JsonElement value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static JsonElement? Navigate(JsonElement root, params string[] path)
    {
        JsonElement current = root;
        foreach (string property in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(property, out JsonElement next))
            {
                return null;
            }

            current = next;
        }

        return current;
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)
            ? date
            : null;
}
