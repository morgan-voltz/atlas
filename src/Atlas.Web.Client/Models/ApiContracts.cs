namespace Atlas.Web.Client.Models;

// Contrats HTTP du client web. Records redéclarés ici (et non importés d'Atlas.Application)
// parce que l'assembly interactif part dans le navigateur et ne référence que Domain + Shared
// (ADR-017, vérifié par NetArchTest). Ils reflètent les réponses d'Atlas.Api.
// Le PagedResult<T> de résultats de recherche réutilise Atlas.Shared.Result.PagedResult<T>.

/// <summary>Corps de <c>POST /auth/login</c>.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Réponse de <c>POST /auth/login</c> et <c>/auth/refresh</c>. Superset des deux formes possibles :
/// authentification réussie (<see cref="AccessToken"/> + <see cref="ExpiresAt"/>) ou défi 2FA requis
/// (<see cref="TwoFactorRequired"/> + <see cref="ChallengeToken"/>). Les champs absents restent nuls.
/// </summary>
public sealed record LoginResponse(
    string? AccessToken,
    DateTimeOffset? ExpiresAt,
    bool? TwoFactorRequired,
    string? ChallengeToken);

/// <summary>Réponse de <c>GET /companies/{siren}</c> (fiche entreprise — utilisée à partir de M2).</summary>
public sealed record CompanyResponse(
    string Siren,
    string Denomination,
    string? FormeJuridique,
    string? NafCode,
    string? NafLabel,
    AddressResponse? Adresse,
    DateOnly? DateCreation,
    bool IsDiffusible,
    IReadOnlyList<DirigeantResponse> Dirigeants);

public sealed record AddressResponse(string? Line, string? PostalCode, string? City, string? Country);

public sealed record DirigeantResponse(string Nom, string? Qualite);

/// <summary>Élément de <c>GET /companies?name=…</c> (carte-aperçu de résultat de recherche).</summary>
public sealed record CompanySummaryResponse(string Siren, string Denomination, string? Ville, string? NafCode);

/// <summary>
/// Extrait de ProblemDetails (RFC 9457) renvoyé par l'API en cas d'erreur. Le champ <c>code</c>
/// (extension projet) porte le code métier stable utilisé pour choisir le bon message côté UI.
/// </summary>
public sealed record ApiProblem(string? Code, string? Detail, int? Status);
