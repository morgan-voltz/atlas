using Atlas.Domain.Common;

namespace Atlas.Domain.Favorites;

/// <summary>Codes d'erreur métier pour les favoris d'entreprises (F-017).</summary>
public static class CompanyFavoriteErrors
{
    public static DomainError AlreadyFavorite(string siren) => new AlreadyFavoriteError(siren);

    public static DomainError NotFavorite(string siren) => new NotFavoriteError(siren);

    public static DomainError InvalidSiren(string value) => new InvalidSirenError(value);

    private sealed record AlreadyFavoriteError(string Siren)
        : DomainError("favorites.company_already_favorite", $"L'entreprise {Siren} est déjà dans vos favoris.");

    private sealed record NotFavoriteError(string Siren)
        : DomainError("favorites.company_not_favorite", $"L'entreprise {Siren} n'est pas dans vos favoris.");

    private sealed record InvalidSirenError(string Value)
        : DomainError("favorites.invalid_siren", $"SIREN invalide : « {Value} ».");
}
