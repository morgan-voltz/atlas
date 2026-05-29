using Atlas.Domain.Common;

namespace Atlas.Domain.Downloads;

public static class BulkDownloadErrors
{
    public static DomainError EmptySirens { get; } = new EmptySirensError();

    public static DomainError TooManySirens(int max) => new TooManySirensError(max);

    public static DomainError InvalidSiren(string value) => new InvalidSirenError(value);

    public static DomainError NotFound { get; } = new NotFoundError();

    public static DomainError NotReady { get; } = new NotReadyError();

    public static DomainError Expired { get; } = new ExpiredError();

    private sealed record EmptySirensError()
        : DomainError("downloads.empty_sirens", "La liste de SIREN ne peut pas être vide.");

    private sealed record TooManySirensError(int Max)
        : DomainError("downloads.too_many_sirens", $"Maximum {Max} SIREN par job.");

    private sealed record InvalidSirenError(string Value)
        : DomainError("downloads.invalid_siren", $"SIREN invalide : « {Value} ».");

    private sealed record NotFoundError()
        : DomainError("downloads.not_found", "Job de téléchargement introuvable.");

    private sealed record NotReadyError()
        : DomainError("downloads.not_ready", "L'archive n'est pas encore prête.");

    private sealed record ExpiredError()
        : DomainError("downloads.expired", "L'archive a expiré.");
}
