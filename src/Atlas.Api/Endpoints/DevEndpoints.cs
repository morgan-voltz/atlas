using Atlas.Api.Dev;

namespace Atlas.Api.Endpoints;

/// <summary>
/// Endpoints de DÉVELOPPEMENT, mappés uniquement si l'environnement est Development (cf. Program.cs).
/// Ils n'existent pas en production. Servent à automatiser les tests d'API (Bruno).
/// </summary>
internal static class DevEndpoints
{
    public static IEndpointRouteBuilder MapDevEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/dev").WithTags("Dev");

        // Renvoie le token de vérification d'email capturé pour un compte, afin de confirmer
        // l'email sans accès aux logs. DEV UNIQUEMENT.
        group.MapGet("/verification-token", (string email, DevVerificationTokenStore store) =>
            store.TryGet(email, out DevVerificationEntry entry)
                ? Results.Ok(new { userId = entry.UserId, token = entry.Token })
                : Results.NotFound(new { message = "Aucun token de vérification pour cet email." }));

        return routes;
    }
}
