using System.Security.Claims;
using Atlas.Application.Search.GetSearchHistory;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class SearchHistoryEndpoints
{
    public static IEndpointRouteBuilder MapSearchHistoryEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/search-history", GetAsync).WithTags("SearchHistory").RequireAuthorization();

        return routes;
    }

    private static async Task<IResult> GetAsync(ClaimsPrincipal principal, ISender sender, CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<IReadOnlyList<SearchHistoryEntryDto>> result = await sender.Send(new GetSearchHistoryQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }
}
