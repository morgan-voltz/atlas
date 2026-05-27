using Atlas.Domain.Common;

namespace Atlas.Domain.IntellectualProperty;

public static class TrademarkErrors
{
    public static DomainError NotFound(DepositNumber depositNumber) => new TrademarkNotFoundError(depositNumber.Value);

    public static readonly DomainError ImageNotFound = new TrademarkImageNotFoundError();

    private sealed record TrademarkNotFoundError(string DepositNumber)
        : DomainError("trademarks.not_found", $"Aucune marque trouvée pour le numéro de dépôt {DepositNumber}.");

    private sealed record TrademarkImageNotFoundError()
        : DomainError("trademarks.image_not_found", "Aucune image disponible pour cette marque.");
}
