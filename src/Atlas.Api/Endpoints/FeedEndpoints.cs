using System.Security.Claims;
using Atlas.Application.Veille.AddUserFeedSource;
using Atlas.Application.Veille.GetMySubscriptions;
using Atlas.Application.Veille.GetRecentFeedItems;
using Atlas.Application.Veille.Unsubscribe;
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

        // F-043 — ajout libre d'une source RSS/Atom par l'utilisateur.
        group.MapPost("/sources", AddSourceAsync);

        // F-043 — abonnements de veille de l'utilisateur courant.
        group.MapGet("/subscriptions", GetSubscriptionsAsync);
        group.MapDelete("/subscriptions/{id:guid}", UnsubscribeAsync);

        return routes;
    }

    private static async Task<IResult> AddSourceAsync(
        ClaimsPrincipal principal,
        AddFeedSourceRequest request,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<VeilleSubscriptionDto> result =
            await sender.Send(new AddUserFeedSourceCommand(userId, request.Url, request.Name), ct);

        return result.IsSuccess
            ? Results.Created($"/feed/subscriptions/{result.Value!.SubscriptionId}", result.Value)
            : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetSubscriptionsAsync(ClaimsPrincipal principal, ISender sender, CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<IReadOnlyList<VeilleSubscriptionDto>> result = await sender.Send(new GetMySubscriptionsQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> UnsubscribeAsync(ClaimsPrincipal principal, Guid id, ISender sender, CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(new UnsubscribeFromFeedSourceCommand(userId, id), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private sealed record AddFeedSourceRequest(string Url, string? Name);
}
