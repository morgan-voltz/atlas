using System.Security.Claims;
using Atlas.Application.IntellectualProperty;
using Atlas.Application.IntellectualProperty.GetTrademark;
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
        group.MapGet("/{depositNumber}", GetByDepositNumberAsync);
        group.MapGet("/{depositNumber}/image", GetImageAsync);

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

    private static async Task<IResult> GetByDepositNumberAsync(
        string depositNumber,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<TrademarkDetailDto> result = await sender.Send(new GetTrademarkQuery(userId, depositNumber), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetImageAsync(
        string depositNumber,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<TrademarkImageDto> result = await sender.Send(new GetTrademarkImageQuery(userId, depositNumber), ct);
        return result.IsSuccess
            ? Results.File(result.Value!.Content, result.Value!.ContentType)
            : result.Error!.ToProblem();
    }
}
