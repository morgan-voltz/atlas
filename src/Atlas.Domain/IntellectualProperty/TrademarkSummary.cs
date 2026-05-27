namespace Atlas.Domain.IntellectualProperty;

/// <summary>
/// Vue allégée d'une marque pour les listes de résultats (cf. F-006).
/// </summary>
public sealed record TrademarkSummary(
    string Denomination,
    string? Deposant,
    DepositNumber DepositNumber,
    DateOnly? DateDepot,
    string? StatutJuridique);
