using System.Security.Claims;
using Atlas.Application.IntellectualProperty;
using Atlas.Application.IntellectualProperty.SearchTrademarksByName;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class TrademarksEndpoints
{
    public static IEndpointRouteBuilder MapTrademarksEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/trademarks").WithTags("Trademarks").RequireAuthorization();

        group.MapGet("/", SearchByNameAsync);

        return routes;
    }

    private static async Task<IResult> SearchByNameAsync(
        string? name,
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

        var query = new SearchTrademarksByNameQuery(userId, name ?? string.Empty, page ?? 1, pageSize ?? 20);
        Result<PagedResult<TrademarkSummaryDto>> result = await sender.Send(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }
}
