using System.Security.Claims;
using Atlas.Application.Favorites;
using Atlas.Application.Favorites.AddCompanyFavorite;
using Atlas.Application.Favorites.AddPatentFavorite;
using Atlas.Application.Favorites.AddTrademarkFavorite;
using Atlas.Application.Favorites.GetMyCompanyFavorites;
using Atlas.Application.Favorites.GetMyPatentFavorites;
using Atlas.Application.Favorites.GetMyTrademarkFavorites;
using Atlas.Application.Favorites.RemoveCompanyFavorite;
using Atlas.Application.Favorites.RemovePatentFavorite;
using Atlas.Application.Favorites.RemoveTrademarkFavorite;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class FavoritesEndpoints
{
    public static IEndpointRouteBuilder MapFavoritesEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder companies = routes.MapGroup("/favorites/companies")
            .WithTags("Favorites")
            .RequireAuthorization();

        companies.MapPost("", AddCompanyAsync);
        companies.MapDelete("{siren}", RemoveCompanyAsync);
        companies.MapGet("", GetMyCompaniesAsync);

        // F-018 — favoris marques
        RouteGroupBuilder trademarks = routes.MapGroup("/favorites/trademarks")
            .WithTags("Favorites")
            .RequireAuthorization();

        trademarks.MapPost("", AddTrademarkAsync);
        trademarks.MapDelete("{depositNumber}", RemoveTrademarkAsync);
        trademarks.MapGet("", GetMyTrademarksAsync);

        // F-018 — favoris brevets
        RouteGroupBuilder patents = routes.MapGroup("/favorites/patents")
            .WithTags("Favorites")
            .RequireAuthorization();

        patents.MapPost("", AddPatentAsync);
        patents.MapDelete("{publicationNumber}", RemovePatentAsync);
        patents.MapGet("", GetMyPatentsAsync);

        return routes;
    }

    // ── Companies ───────────────────────────────────────────────────────────────────

    private static async Task<IResult> AddCompanyAsync(
        AddCompanyFavoriteRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(
            new AddCompanyFavoriteCommand(userId, request.Siren, request.Name),
            ct);

        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> RemoveCompanyAsync(
        string siren,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(new RemoveCompanyFavoriteCommand(userId, siren), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetMyCompaniesAsync(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<IReadOnlyList<CompanyFavoriteDto>> result = await sender.Send(new GetMyCompanyFavoritesQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    // ── Trademarks (F-018) ──────────────────────────────────────────────────────────

    private static async Task<IResult> AddTrademarkAsync(
        AddTrademarkFavoriteRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(
            new AddTrademarkFavoriteCommand(userId, request.DepositNumber, request.Name),
            ct);

        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> RemoveTrademarkAsync(
        string depositNumber,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(new RemoveTrademarkFavoriteCommand(userId, depositNumber), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetMyTrademarksAsync(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<IReadOnlyList<TrademarkFavoriteDto>> result =
            await sender.Send(new GetMyTrademarkFavoritesQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    // ── Patents (F-018) ─────────────────────────────────────────────────────────────

    private static async Task<IResult> AddPatentAsync(
        AddPatentFavoriteRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(
            new AddPatentFavoriteCommand(userId, request.PublicationNumber, request.Title),
            ct);

        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> RemovePatentAsync(
        string publicationNumber,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result result = await sender.Send(new RemovePatentFavoriteCommand(userId, publicationNumber), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetMyPatentsAsync(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<IReadOnlyList<PatentFavoriteDto>> result =
            await sender.Send(new GetMyPatentFavoritesQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }
}

internal sealed record AddCompanyFavoriteRequest(string Siren, string? Name);

internal sealed record AddTrademarkFavoriteRequest(string DepositNumber, string? Name);

internal sealed record AddPatentFavoriteRequest(string PublicationNumber, string? Title);
