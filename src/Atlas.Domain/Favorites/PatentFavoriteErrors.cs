using Atlas.Domain.Common;

namespace Atlas.Domain.Favorites;

public static class PatentFavoriteErrors
{
    public static DomainError AlreadyFavorite(string publicationNumber) => new AlreadyFavoriteError(publicationNumber);

    public static DomainError NotFavorite(string publicationNumber) => new NotFavoriteError(publicationNumber);

    public static DomainError InvalidPublicationNumber(string value) => new InvalidPublicationNumberError(value);

    private sealed record AlreadyFavoriteError(string PublicationNumber)
        : DomainError("favorites.patent_already_favorite", $"Le brevet {PublicationNumber} est déjà dans vos favoris.");

    private sealed record NotFavoriteError(string PublicationNumber)
        : DomainError("favorites.patent_not_favorite", $"Le brevet {PublicationNumber} n'est pas dans vos favoris.");

    private sealed record InvalidPublicationNumberError(string Value)
        : DomainError("favorites.invalid_publication_number", $"Numéro de publication invalide : « {Value} ».");
}
