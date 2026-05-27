using System.Security.Claims;
using Atlas.Application.Companies;
using Atlas.Application.Companies.GetCompanyBySiren;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class CompaniesEndpoints
{
    public static IEndpointRouteBuilder MapCompaniesEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/companies").WithTags("Companies").RequireAuthorization();

        group.MapGet("/{siren}", GetBySirenAsync);

        return routes;
    }

    private static async Task<IResult> GetBySirenAsync(
        string siren,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<CompanyDto> result = await sender.Send(new GetCompanyBySirenQuery(userId, siren), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }
}
