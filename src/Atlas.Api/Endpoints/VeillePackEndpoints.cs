using System.Security.Claims;
using Atlas.Application.Veille.ApplyVeillePack;
using Atlas.Application.Veille.GetMyVeillePacks;
using Atlas.Application.Veille.GetVeilleCatalog;
using Atlas.Application.Veille.SyncVeillePack;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class VeillePackEndpoints
{
    public static IEndpointRouteBuilder MapVeillePackEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/veille/packs").WithTags("Veille").RequireAuthorization();

        // Catalogue des packs proposés (F-042).
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            Result<IReadOnlyList<VeillePackDto>> result = await sender.Send(new GetVeilleCatalogQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
        });

        // Packs déjà appliqués par l'utilisateur, avec signal de mise à jour disponible.
        group.MapGet("/mine", GetMineAsync);

        // Appliquer un pack : s'abonner à toutes ses sources.
        group.MapPost("/{code}/apply", ApplyAsync);

        // Re-synchroniser un pack déjà appliqué sur sa version courante.
        group.MapPost("/{code}/sync", SyncAsync);

        return routes;
    }

    private static async Task<IResult> GetMineAsync(ClaimsPrincipal principal, ISender sender, CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<IReadOnlyList<MyVeillePackDto>> result = await sender.Send(new GetMyVeillePacksQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> ApplyAsync(ClaimsPrincipal principal, string code, ISender sender, CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<ApplyVeillePackResult> result = await sender.Send(new ApplyVeillePackCommand(userId, code), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> SyncAsync(ClaimsPrincipal principal, string code, ISender sender, CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<ApplyVeillePackResult> result = await sender.Send(new SyncVeillePackCommand(userId, code), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }
}
