using System.Security.Claims;
using Atlas.Application.IntellectualProperty.GetPatent;
using Atlas.Application.IntellectualProperty.SearchPatents;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class PatentsEndpoints
{
    public static IEndpointRouteBuilder MapPatentsEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/patents").WithTags("Patents").RequireAuthorization();

        // F-016 — recherche multi-critères. Précède la route paramétrée pour éviter la collision.
        group.MapGet("/", SearchAsync);
        // F-015 — notice brevet par numéro de publication.
        group.MapGet("/{publicationNumber}", GetByPublicationNumberAsync);

        return routes;
    }

    private static async Task<IResult> SearchAsync(
        string? title,
        string? inventor,
        string? applicant,
        int? page,
        int? pageSize,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        var query = new SearchPatentsQuery(userId, title, inventor, applicant, page ?? 1, pageSize ?? 20);
        Result<PagedResult<PatentSummaryDto>> result = await sender.Send(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
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
