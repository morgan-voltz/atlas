using System.Text.Json.Serialization;

namespace Atlas.Infrastructure.Inpi.Pi.DTOs;

// Lot 13 — la requête de recherche brevets est désormais portée par le record
// PatentQueryRequest interne à InpiPiTrademarkProvider (contrat PatentQuery v2 :
// collections + query SolR + position/size). Plus de PiPatentSearchRequest ici.

internal sealed record PiPatentSearchResponse(
    [property: JsonPropertyName("results")] List<PiPatentSummary>? Results,
    [property: JsonPropertyName("total")] long? Total);

internal sealed record PiPatentSummary(
    [property: JsonPropertyName("numeroPublication")] string? NumeroPublication,
    [property: JsonPropertyName("titre")] string? Titre,
    [property: JsonPropertyName("deposant")] string? Deposant,
    [property: JsonPropertyName("dateDepot")] string? DateDepot,
    [property: JsonPropertyName("statut")] string? Statut);
