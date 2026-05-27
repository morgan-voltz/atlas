using System.Text.Json.Serialization;

namespace Atlas.Infrastructure.Inpi.Rne.DTOs;

// ⚠️ Sous-ensemble best-effort du JSON RNE (GET /companies/{siren}). La structure réelle des
// « formalités » est volumineuse et imbriquée : ces DTOs et le mapper associé DOIVENT être validés
// contre l'API RNE réelle (cf. doc technique INPI RNE) avant toute mise en production.

internal sealed record RneCompanyResponse(
    [property: JsonPropertyName("siren")] string? Siren,
    [property: JsonPropertyName("formality")] RneFormality? Formality);

internal sealed record RneFormality(
    [property: JsonPropertyName("content")] RneContent? Content,
    [property: JsonPropertyName("diffusionINSEE")] string? DiffusionInsee);

internal sealed record RneContent(
    [property: JsonPropertyName("personneMorale")] RnePersonneMorale? PersonneMorale,
    [property: JsonPropertyName("personnePhysique")] RnePersonnePhysique? PersonnePhysique);

internal sealed record RnePersonneMorale(
    [property: JsonPropertyName("identite")] RneIdentite? Identite,
    [property: JsonPropertyName("adresseEntreprise")] RneAdresseEntreprise? AdresseEntreprise);

internal sealed record RnePersonnePhysique(
    [property: JsonPropertyName("identite")] RneIdentite? Identite,
    [property: JsonPropertyName("adresseEntreprise")] RneAdresseEntreprise? AdresseEntreprise);

internal sealed record RneIdentite(
    [property: JsonPropertyName("entreprise")] RneEntreprise? Entreprise);

internal sealed record RneEntreprise(
    [property: JsonPropertyName("denomination")] string? Denomination,
    [property: JsonPropertyName("formeJuridique")] string? FormeJuridique,
    [property: JsonPropertyName("codeApe")] string? CodeApe,
    [property: JsonPropertyName("dateImmatriculation")] string? DateImmatriculation);

internal sealed record RneAdresseEntreprise(
    [property: JsonPropertyName("adresse")] RneAdresse? Adresse);

internal sealed record RneAdresse(
    [property: JsonPropertyName("numVoie")] string? NumVoie,
    [property: JsonPropertyName("typeVoie")] string? TypeVoie,
    [property: JsonPropertyName("voie")] string? Voie,
    [property: JsonPropertyName("codePostal")] string? CodePostal,
    [property: JsonPropertyName("commune")] string? Commune,
    [property: JsonPropertyName("pays")] string? Pays);
