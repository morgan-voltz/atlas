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

    private static async Task<IResult> GetAsync(CurrentUser user, ISender sender, CancellationToken ct)
    {
        Result<IReadOnlyList<SearchHistoryEntryDto>> result = await sender.Send(new GetSearchHistoryQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }
}
