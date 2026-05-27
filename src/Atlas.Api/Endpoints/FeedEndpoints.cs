using Atlas.Application.Veille.GetRecentFeedItems;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class FeedEndpoints
{
    public static IEndpointRouteBuilder MapFeedEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/feed").WithTags("Veille").RequireAuthorization();

        // Items de veille agrégés, du plus récent au plus ancien (timeline enrichie = F-044).
        group.MapGet("/items", async (ISender sender, int? page, int? pageSize, CancellationToken ct) =>
        {
            Result<PagedResult<FeedItemDto>> result =
                await sender.Send(new GetRecentFeedItemsQuery(page ?? 1, pageSize ?? 20), ct);

            return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
        });

        return routes;
    }
}
