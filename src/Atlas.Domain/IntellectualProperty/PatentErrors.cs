using Atlas.Domain.Common;

namespace Atlas.Domain.IntellectualProperty;

public static class PatentErrors
{
    public static DomainError InvalidPublicationNumber(string value) =>
        new InvalidPublicationNumberError(value);

    public static DomainError NotFound(PublicationNumber number) =>
        new NotFoundError(number.Value);

    public static DomainError EmptySearch { get; } = new EmptySearchError();

    private sealed record InvalidPublicationNumberError(string Value)
        : DomainError("patents.invalid_publication_number", $"Numéro de publication invalide : « {Value} ».");

    private sealed record NotFoundError(string Number)
        : DomainError("patents.not_found", $"Aucun brevet trouvé pour le numéro {Number}.");

    private sealed record EmptySearchError()
        : DomainError("patents.empty_search", "Au moins un critère de recherche (titre, inventeur ou déposant) doit être renseigné.");
}
