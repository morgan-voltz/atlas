namespace Atlas.App.Models;

// DTOs *côté client* (ADR-002) : le client désérialise les réponses de l'API dans ses propres
// records, sans référencer Atlas.Application (où vivent les DTOs serveur). Calés sur les contrats
// exposés par Atlas.Api. Repris de Atlas.Web.Client/Models/ApiContracts.cs lors du portage Uno.

/// <summary>Corps de la requête de connexion (<c>POST /auth/login</c>).</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Réponse d'un access token (login réussi, refresh) — calé sur le contrat de l'API
/// (<c>Atlas.Api.Endpoints.AccessTokenResponse</c> : propriété <c>ExpiresAt</c>).</summary>
public sealed record AccessTokenResponse(string AccessToken, System.DateTimeOffset ExpiresAt);

/// <summary>Corps de <c>POST /auth/2fa/verify</c> (défi TOTP). Le challenge token reste en mémoire, hors URL.</summary>
public sealed record VerifyTwoFactorRequest(string ChallengeToken, string Code);

/// <summary>Résultat résumé d'une entreprise (liste de recherche, favoris).</summary>
public sealed record CompanySummaryResponse(string Siren, string Denomination, string? Ville, string? NafCode);

/// <summary>Élément de <c>GET /favorites/companies</c> (entreprise suivie — F-017).</summary>
public sealed record CompanyFavoriteResponse(string Siren, string? Name, System.DateTimeOffset AddedAt);

/// <summary>Corps de <c>POST /favorites/companies</c>.</summary>
public sealed record AddCompanyFavoriteRequest(string Siren, string? Name);

/// <summary>Adresse postale (siège). Champs optionnels selon la couverture RNE.</summary>
public sealed record AddressResponse(string? Line, string? PostalCode, string? City, string? Country);

/// <summary>Dirigeant / mandataire d'une unité légale (RNE).</summary>
public sealed record DirigeantResponse(string Nom, string? Qualite);

/// <summary>Fiche entreprise (<c>GET /companies/{siren}</c>, F-004) — calée sur le contrat de l'API.</summary>
public sealed record CompanyResponse(
    string Siren,
    string Denomination,
    string? FormeJuridique,
    string? NafCode,
    string? NafLabel,
    AddressResponse? Adresse,
    System.DateOnly? DateCreation,
    bool IsDiffusible,
    System.Collections.Generic.IReadOnlyList<DirigeantResponse> Dirigeants);

/// <summary>
/// Corps de <c>POST /inpi/connection</c> (F-003). Identifiants techniques INPI : transmis une fois
/// à l'API (chiffrés au repos côté serveur), JAMAIS persistés ni journalisés côté client (CLAUDE.md).
/// </summary>
public sealed record ConnectInpiRequest(string Username, string Password);

/// <summary>Réponse de <c>GET /inpi/connection</c> (statut, sans aucun secret).</summary>
public sealed record InpiConnectionStatusResponse(bool Connected, string? Status, System.DateTimeOffset? LastTestedAt);

/// <summary>Mention d'un favori dans un item de veille (puce navigable vers la fiche).</summary>
public sealed record FavoriteMentionResponse(string Siren, string Name);

/// <summary>
/// Élément de la timeline unifiée (<c>GET /feed/timeline</c>). <see cref="Kind"/> discrimine
/// <c>"RssItem"</c> (item de flux) et <c>"FavoriteEvent"</c> (mouvement RNE/BODACC d'un favori).
/// Sous-ensemble des champs consommés par la carte (calé sur Atlas.Web.Client).
/// </summary>
/// <summary>Corps de <c>PATCH /feed/items/{id}/state</c> (lu / favori / archivé).</summary>
public sealed record SetFeedItemStateRequest(bool? IsRead, bool? IsFavorite, bool? IsArchived);

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
