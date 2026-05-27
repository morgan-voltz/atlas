namespace Atlas.Application.Companies;

public sealed record CompanyDto(
    string Siren,
    string Denomination,
    string? FormeJuridique,
    string? NafCode,
    string? NafLabel,
    AddressDto? Adresse,
    DateOnly? DateCreation,
    bool IsDiffusible,
    IReadOnlyList<DirigeantDto> Dirigeants);

public sealed record AddressDto(string? Line, string? PostalCode, string? City, string? Country);

public sealed record DirigeantDto(string Nom, string? Qualite);
