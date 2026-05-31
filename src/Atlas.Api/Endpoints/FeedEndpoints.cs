using System.Security.Claims;
using Atlas.Application.Veille.AddUserFeedSource;
using Atlas.Application.Veille.GetMySubscriptions;
using Atlas.Application.Veille.GetRecentFeedItems;
using Atlas.Application.Veille.GetTimeline;
using Atlas.Application.Veille.Rules;
using Atlas.Application.Veille.Rules.CreateFeedRule;
using Atlas.Application.Veille.Rules.DeleteFeedRule;
using Atlas.Application.Veille.Rules.ListMyFeedRules;
using Atlas.Application.Veille.Rules.UpdateFeedRule;
using Atlas.Application.Veille.SetFeedItemState;
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

        // F-044 — timeline unifiée (sources abonnées) et état lu/favori/archivé par item.
        group.MapGet("/timeline", GetTimelineAsync);
        group.MapPatch("/items/{id:guid}/state", SetItemStateAsync);

        // F-046 — règles de surveillance personnalisées (critères AND, alerte email + push).
        group.MapPost("/rules", CreateRuleAsync);
        group.MapGet("/rules", ListRulesAsync);
        group.MapPatch("/rules/{id:guid}", UpdateRuleAsync);
        group.MapDelete("/rules/{id:guid}", DeleteRuleAsync);

        return routes;
    }

    private static async Task<IResult> CreateRuleAsync(
        ClaimsPrincipal principal,
        CreateFeedRuleRequest request,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<FeedRuleDto> result = await sender.Send(
            new CreateFeedRuleCommand(
                userId,
                request.Name,
                request.KeywordPattern,
                request.SourceId,
                request.MentionedSiren,
                request.NotifyEmail,
                request.NotifyPush),
            ct);

        return result.IsSuccess
            ? Results.Created($"/feed/rules/{result.Value!.Id}", result.Value)
            : result.Error!.ToProblem();
    }

    private static async Task<IResult> ListRulesAsync(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<IReadOnlyList<FeedRuleDto>> result =
            await sender.Send(new ListMyFeedRulesQuery(userId), ct);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> UpdateRuleAsync(
        ClaimsPrincipal principal,
        Guid id,
        UpdateFeedRuleRequest request,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<FeedRuleDto> result = await sender.Send(
            new UpdateFeedRuleCommand(
                userId,
                id,
                request.Name,
                request.KeywordPattern,
                request.SourceId,
                request.MentionedSiren,
                request.NotifyEmail,
                request.NotifyPush,
                request.IsActive),
            ct);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DeleteRuleAsync(
        ClaimsPrincipal principal,
        Guid id,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(new DeleteFeedRuleCommand(userId, id), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetTimelineAsync(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct,
        int? page = null,
        int? pageSize = null,
        Guid? sourceId = null,
        DateTimeOffset? after = null,
        DateTimeOffset? before = null,
        string? keyword = null,
        bool unread = false,
        bool favorites = false,
        bool includeArchived = false,
        bool mentionsFavoritesOnly = false,
        bool editorialOnly = false)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<PagedResult<TimelineItemDto>> result = await sender.Send(
            new GetTimelineQuery(
                userId, page ?? 1, pageSize ?? 20, sourceId, after, before, keyword,
                unread, favorites, includeArchived, mentionsFavoritesOnly, editorialOnly),
            ct);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> SetItemStateAsync(
        ClaimsPrincipal principal,
        Guid id,
        SetFeedItemStateRequest request,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<FeedItemStateDto> result = await sender.Send(
            new SetFeedItemStateCommand(userId, id, request.IsRead, request.IsFavorite, request.IsArchived), ct);

        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
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

    private sealed record SetFeedItemStateRequest(bool? IsRead, bool? IsFavorite, bool? IsArchived);

    private sealed record CreateFeedRuleRequest(
        string Name,
        string? KeywordPattern,
        Guid? SourceId,
        string? MentionedSiren,
        bool NotifyEmail,
        bool NotifyPush);

    private sealed record UpdateFeedRuleRequest(
        string Name,
        string? KeywordPattern,
        Guid? SourceId,
        string? MentionedSiren,
        bool NotifyEmail,
        bool NotifyPush,
        bool IsActive);
}
