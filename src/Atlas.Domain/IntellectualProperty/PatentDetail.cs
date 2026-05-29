namespace Atlas.Domain.IntellectualProperty;

/// <summary>
/// Notice complète d'un brevet (F-015) : identité, déposant, inventeurs, dates, statut.
/// </summary>
public sealed record PatentDetail(
    PublicationNumber PublicationNumber,
    string Title,
    string? Applicant,
    IReadOnlyList<string> Inventors,
    DateOnly? DepositDate,
    DateOnly? PublicationDate,
    string? Status,
    string? AbstractText);
