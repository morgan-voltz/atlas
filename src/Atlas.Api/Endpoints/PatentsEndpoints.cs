using System.Security.Claims;
using Atlas.Application.IntellectualProperty.GetPatent;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class PatentsEndpoints
{
    public static IEndpointRouteBuilder MapPatentsEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/patents").WithTags("Patents").RequireAuthorization();

        // F-015 — notice brevet par numéro de publication.
        group.MapGet("/{publicationNumber}", GetByPublicationNumberAsync);

        return routes;
    }

    private static async Task<IResult> GetByPublicationNumberAsync(
        string publicationNumber,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<PatentDetailDto> result =
            await sender.Send(new GetPatentQuery(userId, publicationNumber), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }
}
