namespace Atlas.Application.Companies;

public sealed record CompanySummaryDto(
    string Siren,
    string Denomination,
    string? Ville,
    string? NafCode);
