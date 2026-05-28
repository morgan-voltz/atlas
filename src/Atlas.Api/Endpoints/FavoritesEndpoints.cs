using System.Security.Claims;
using Atlas.Application.Favorites;
using Atlas.Application.Favorites.AddCompanyFavorite;
using Atlas.Application.Favorites.GetMyCompanyFavorites;
using Atlas.Application.Favorites.RemoveCompanyFavorite;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class FavoritesEndpoints
{
    public static IEndpointRouteBuilder MapFavoritesEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/favorites/companies")
            .WithTags("Favorites")
            .RequireAuthorization();

        group.MapPost("", AddAsync);
        group.MapDelete("{siren}", RemoveAsync);
        group.MapGet("", GetMineAsync);

        return routes;
    }

    private static async Task<IResult> AddAsync(
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

    private static async Task<IResult> RemoveAsync(
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

    private static async Task<IResult> GetMineAsync(
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
}

internal sealed record AddCompanyFavoriteRequest(string Siren, string? Name);
