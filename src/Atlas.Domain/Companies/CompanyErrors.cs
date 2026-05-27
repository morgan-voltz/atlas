using Atlas.Domain.Common;

namespace Atlas.Domain.Companies;

public static class CompanyErrors
{
    public static DomainError InvalidSiren(string value) => new InvalidSirenError(value);

    public static DomainError NotFound(Siren siren) => new CompanyNotFoundError(siren.Value);

    private sealed record InvalidSirenError(string Value)
        : DomainError("companies.invalid_siren", $"Le SIREN « {Value} » est invalide (9 chiffres, contrôle Luhn).");

    private sealed record CompanyNotFoundError(string Siren)
        : DomainError("companies.not_found", $"Aucune entreprise trouvée pour le SIREN {Siren}.");
}
