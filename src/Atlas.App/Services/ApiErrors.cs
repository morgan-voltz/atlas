using Atlas.Shared.Result;

namespace Atlas.App.Services;

/// <summary>Erreurs côté client liées aux appels à l'API Atlas (ADR-002).</summary>
public sealed record ApiError(string Code, string Message) : Error(Code, Message);

public static class ApiErrors
{
    public static ApiError RequestFailed(int statusCode) =>
        new("api.request_failed", $"L'appel à l'API a échoué (HTTP {statusCode}).");
}
