using System.Text.Json.Serialization;

namespace Atlas.Infrastructure.Inpi.Pi.DTOs;

// ⚠️ Sous-ensemble best-effort de la réponse de l'API INPI PI (POST /marques/search). Le format réel
// (JSON/XML, noms de champs, enveloppe de pagination) DOIT être validé contre l'API PI réelle.

internal sealed record PiTrademarkSearchResponse(
    [property: JsonPropertyName("results")] List<PiTrademark>? Results,
    [property: JsonPropertyName("total")] long? Total);

internal sealed record PiTrademark(
    [property: JsonPropertyName("marque")] string? Marque,
    [property: JsonPropertyName("deposant")] string? Deposant,
    [property: JsonPropertyName("numeroDepot")] string? NumeroDepot,
    [property: JsonPropertyName("dateDepot")] string? DateDepot,
    [property: JsonPropertyName("statut")] string? Statut);

internal sealed record PiTrademarkNotice(
    [property: JsonPropertyName("marque")] string? Marque,
    [property: JsonPropertyName("deposant")] string? Deposant,
    [property: JsonPropertyName("numeroDepot")] string? NumeroDepot,
    [property: JsonPropertyName("dateDepot")] string? DateDepot,
    [property: JsonPropertyName("dateEnregistrement")] string? DateEnregistrement,
    [property: JsonPropertyName("statut")] string? Statut,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("hasImage")] bool? HasImage,
    [property: JsonPropertyName("classes")] List<PiNiceClass>? Classes);

internal sealed record PiNiceClass(
    [property: JsonPropertyName("numero")] int? Numero,
    [property: JsonPropertyName("libelle")] string? Libelle);
