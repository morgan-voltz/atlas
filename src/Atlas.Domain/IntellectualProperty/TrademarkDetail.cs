namespace Atlas.Domain.IntellectualProperty;

/// <summary>
/// Notice complète d'une marque (cf. F-007) : identité, déposant, dates, statut, type et classes de Nice.
/// </summary>
public sealed record TrademarkDetail(
    string Denomination,
    string? Deposant,
    DepositNumber DepositNumber,
    DateOnly? DateDepot,
    DateOnly? DateEnregistrement,
    string? StatutJuridique,
    string? Type,
    bool HasImage,
    IReadOnlyList<NiceClassification> ClassesNice);
