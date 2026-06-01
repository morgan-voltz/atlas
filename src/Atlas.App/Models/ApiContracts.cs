namespace Atlas.App.Models;

// DTOs *côté client* (ADR-002) : le client désérialise les réponses de l'API dans ses propres
// records, sans référencer Atlas.Application (où vivent les DTOs serveur). Calés sur les contrats
// exposés par Atlas.Api. Repris de Atlas.Web.Client/Models/ApiContracts.cs lors du portage Uno.

/// <summary>Résultat résumé d'une entreprise (liste de recherche, favoris).</summary>
public sealed record CompanySummaryResponse(string Siren, string Denomination, string? Ville, string? NafCode);
