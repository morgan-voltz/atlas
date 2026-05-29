namespace Atlas.Domain.IntellectualProperty;

/// <summary>Vue allégée d'un brevet pour les listes de résultats (F-016).</summary>
public sealed record PatentSummary(
    PublicationNumber PublicationNumber,
    string Title,
    string? Applicant,
    DateOnly? DepositDate,
    string? Status);
