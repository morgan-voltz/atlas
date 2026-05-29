using Atlas.Domain.Common;

namespace Atlas.Domain.Favorites;

public static class TrademarkFavoriteErrors
{
    public static DomainError AlreadyFavorite(string depositNumber) => new AlreadyFavoriteError(depositNumber);

    public static DomainError NotFavorite(string depositNumber) => new NotFavoriteError(depositNumber);

    public static DomainError InvalidDepositNumber(string value) => new InvalidDepositNumberError(value);

    private sealed record AlreadyFavoriteError(string DepositNumber)
        : DomainError("favorites.trademark_already_favorite", $"La marque {DepositNumber} est déjà dans vos favoris.");

    private sealed record NotFavoriteError(string DepositNumber)
        : DomainError("favorites.trademark_not_favorite", $"La marque {DepositNumber} n'est pas dans vos favoris.");

    private sealed record InvalidDepositNumberError(string Value)
        : DomainError("favorites.invalid_deposit_number", $"Numéro de dépôt invalide : « {Value} ».");
}
