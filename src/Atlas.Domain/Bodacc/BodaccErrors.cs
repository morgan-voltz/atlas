using Atlas.Domain.Common;

namespace Atlas.Domain.Bodacc;

public static class BodaccErrors
{
    public static DomainError Unavailable { get; } = new UnavailableError();

    public static DomainError InvalidResponse(string detail) => new InvalidResponseError(detail);

    private sealed record UnavailableError()
        : DomainError("bodacc.unavailable", "Le service BODACC est indisponible.");

    private sealed record InvalidResponseError(string Detail)
        : DomainError("bodacc.invalid_response", $"Réponse BODACC inattendue : {Detail}");
}
