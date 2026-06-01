using Atlas.Application.Common;
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
        // F-021 — export CSV
        companies.MapGet("/export", ExportCompaniesAsync);

        // F-018 — favoris marques
        RouteGroupBuilder trademarks = routes.MapGroup("/favorites/trademarks")
            .WithTags("Favorites")
            .RequireAuthorization();

        trademarks.MapPost("", AddTrademarkAsync);
        trademarks.MapDelete("{depositNumber}", RemoveTrademarkAsync);
        trademarks.MapGet("", GetMyTrademarksAsync);
        trademarks.MapGet("/export", ExportTrademarksAsync);

        // F-018 — favoris brevets
        RouteGroupBuilder patents = routes.MapGroup("/favorites/patents")
            .WithTags("Favorites")
            .RequireAuthorization();

        patents.MapPost("", AddPatentAsync);
        patents.MapDelete("{publicationNumber}", RemovePatentAsync);
        patents.MapGet("", GetMyPatentsAsync);
        patents.MapGet("/export", ExportPatentsAsync);

        return routes;
    }

    // ── Companies ───────────────────────────────────────────────────────────────────

    private static async Task<IResult> AddCompanyAsync(
        AddCompanyFavoriteRequest request,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(
            new AddCompanyFavoriteCommand(user.Id, request.Siren, request.Name),
            ct);

        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> RemoveCompanyAsync(
        string siren,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(new RemoveCompanyFavoriteCommand(user.Id, siren), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetMyCompaniesAsync(
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result<IReadOnlyList<CompanyFavoriteDto>> result = await sender.Send(new GetMyCompanyFavoritesQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static Task<IResult> ExportCompaniesAsync(CurrentUser user, ISender sender, CancellationToken ct) =>
        ExportAsync(new GetMyCompanyFavoritesQuery(user.Id), FavoritesExports.CompaniesToCsv, "favoris-entreprises.csv", sender, ct);

    // ── Trademarks (F-018) ──────────────────────────────────────────────────────────

    private static async Task<IResult> AddTrademarkAsync(
        AddTrademarkFavoriteRequest request,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(
            new AddTrademarkFavoriteCommand(user.Id, request.DepositNumber, request.Name),
            ct);

        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> RemoveTrademarkAsync(
        string depositNumber,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(new RemoveTrademarkFavoriteCommand(user.Id, depositNumber), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetMyTrademarksAsync(
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result<IReadOnlyList<TrademarkFavoriteDto>> result =
            await sender.Send(new GetMyTrademarkFavoritesQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static Task<IResult> ExportTrademarksAsync(CurrentUser user, ISender sender, CancellationToken ct) =>
        ExportAsync(new GetMyTrademarkFavoritesQuery(user.Id), FavoritesExports.TrademarksToCsv, "favoris-marques.csv", sender, ct);

    // ── Patents (F-018) ─────────────────────────────────────────────────────────────

    private static async Task<IResult> AddPatentAsync(
        AddPatentFavoriteRequest request,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(
            new AddPatentFavoriteCommand(user.Id, request.PublicationNumber, request.Title),
            ct);

        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> RemovePatentAsync(
        string publicationNumber,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(new RemovePatentFavoriteCommand(user.Id, publicationNumber), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetMyPatentsAsync(
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result<IReadOnlyList<PatentFavoriteDto>> result =
            await sender.Send(new GetMyPatentFavoritesQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static Task<IResult> ExportPatentsAsync(CurrentUser user, ISender sender, CancellationToken ct) =>
        ExportAsync(new GetMyPatentFavoritesQuery(user.Id), FavoritesExports.PatentsToCsv, "favoris-brevets.csv", sender, ct);

    // Factorisation des exports CSV (audit Lot 4b — F4) : même flux query → CSV → fichier téléchargeable.
    private static async Task<IResult> ExportAsync<TDto>(
        IRequest<Result<IReadOnlyList<TDto>>> query,
        Func<IReadOnlyList<TDto>, byte[]> toCsv,
        string fileName,
        ISender sender,
        CancellationToken ct)
    {
        Result<IReadOnlyList<TDto>> result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            return result.Error!.ToProblem();
        }

        return Results.File(toCsv(result.Value!), "text/csv; charset=utf-8", fileName);
    }
}

internal sealed record AddCompanyFavoriteRequest(string Siren, string? Name);

internal sealed record AddTrademarkFavoriteRequest(string DepositNumber, string? Name);

internal sealed record AddPatentFavoriteRequest(string PublicationNumber, string? Title);

internal static class FavoritesExports
{
    public static byte[] CompaniesToCsv(IEnumerable<CompanyFavoriteDto> items) =>
        CsvWriter.WriteToBytes(items, new List<CsvColumn<CompanyFavoriteDto>>
        {
            new("SIREN", x => x.Siren),
            new("Dénomination", x => x.Name),
            new("AjoutéLe", x => x.AddedAt),
        });

    public static byte[] TrademarksToCsv(IEnumerable<TrademarkFavoriteDto> items) =>
        CsvWriter.WriteToBytes(items, new List<CsvColumn<TrademarkFavoriteDto>>
        {
            new("NuméroDépôt", x => x.DepositNumber),
            new("Nom", x => x.Name),
            new("AjoutéLe", x => x.AddedAt),
        });

    public static byte[] PatentsToCsv(IEnumerable<PatentFavoriteDto> items) =>
        CsvWriter.WriteToBytes(items, new List<CsvColumn<PatentFavoriteDto>>
        {
            new("NuméroPublication", x => x.PublicationNumber),
            new("Titre", x => x.Title),
            new("AjoutéLe", x => x.AddedAt),
        });
}
