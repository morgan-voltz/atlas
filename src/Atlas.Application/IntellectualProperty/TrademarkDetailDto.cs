namespace Atlas.Application.IntellectualProperty;

public sealed record TrademarkDetailDto(
    string Denomination,
    string? Deposant,
    string DepositNumber,
    DateOnly? DateDepot,
    DateOnly? DateEnregistrement,
    string? StatutJuridique,
    string? Type,
    bool HasImage,
    IReadOnlyList<NiceClassDto> ClassesNice);

public sealed record NiceClassDto(int Number, string? Label);

public sealed record TrademarkImageDto(byte[] Content, string ContentType);
