using System.Text.Json.Serialization;

namespace Atlas.Infrastructure.Inpi.Pi.DTOs;

/// <summary>
/// Payload de recherche brevet (F-016). Forme exacte à valider contre l'API INPI réelle ;
/// pour MVP on envoie les 3 critères tels quels, l'API SolR INPI fait le matching.
/// </summary>
internal sealed record PiPatentSearchRequest(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("inventor")] string? Inventor,
    [property: JsonPropertyName("applicant")] string? Applicant,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize);

internal sealed record PiPatentSearchResponse(
    [property: JsonPropertyName("results")] List<PiPatentSummary>? Results,
    [property: JsonPropertyName("total")] long? Total);

internal sealed record PiPatentSummary(
    [property: JsonPropertyName("numeroPublication")] string? NumeroPublication,
    [property: JsonPropertyName("titre")] string? Titre,
    [property: JsonPropertyName("deposant")] string? Deposant,
    [property: JsonPropertyName("dateDepot")] string? DateDepot,
    [property: JsonPropertyName("statut")] string? Statut);
