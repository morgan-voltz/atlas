namespace Atlas.App.Models;

// DTOs *côté client* (ADR-002) : le client désérialise les réponses de l'API dans ses propres
// records, sans référencer Atlas.Application (où vivent les DTOs serveur). Calés sur les contrats
// exposés par Atlas.Api. Repris de Atlas.Web.Client/Models/ApiContracts.cs lors du portage Uno.

/// <summary>Résultat résumé d'une entreprise (liste de recherche, favoris).</summary>
public sealed record CompanySummaryResponse(string Siren, string Denomination, string? Ville, string? NafCode);

/// <summary>Mention d'un favori dans un item de veille (puce navigable vers la fiche).</summary>
public sealed record FavoriteMentionResponse(string Siren, string Name);

/// <summary>
/// Élément de la timeline unifiée (<c>GET /feed/timeline</c>). <see cref="Kind"/> discrimine
/// <c>"RssItem"</c> (item de flux) et <c>"FavoriteEvent"</c> (mouvement RNE/BODACC d'un favori).
/// Sous-ensemble des champs consommés par la carte (calé sur Atlas.Web.Client).
/// </summary>
public sealed record TimelineItemResponse(
    string Kind,
    System.Guid Id,
    string Title,
    string? Url,
    string? Summary,
    System.DateTimeOffset OccurredAt,
    bool IsRead,
    int SourceCount,
    System.Collections.Generic.IReadOnlyList<FavoriteMentionResponse> MentionedFavorites,
    string? EventSiren);
