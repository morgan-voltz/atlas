namespace Atlas.Maui.Models;

// Modèles de désérialisation côté client. Atlas.Maui ne référence que Domain et Shared (cf. CLAUDE.md) :
// on ne réutilise donc pas les DTOs d'Atlas.Application ; ces modèles miroir sont volontairement locaux.

public sealed record AccessTokenResponse(string AccessToken, DateTimeOffset ExpiresAt);

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

public sealed record CompanySummaryResponse(string Siren, string Denomination, string? Ville, string? NafCode);

public sealed record SearchHistoryEntryResponse(string Type, string Query, DateTimeOffset CreatedAt);
