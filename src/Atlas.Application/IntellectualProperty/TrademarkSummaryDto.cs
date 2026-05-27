namespace Atlas.Application.IntellectualProperty;

public sealed record TrademarkSummaryDto(
    string Denomination,
    string? Deposant,
    string DepositNumber,
    DateOnly? DateDepot,
    string? StatutJuridique);
