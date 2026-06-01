using Atlas.Application.Veille.ApplyVeillePack;
using Atlas.Application.Veille.GetMyVeillePacks;
using Atlas.Application.Veille.GetVeilleCatalog;
using Atlas.Application.Veille.Marketplace;
using Atlas.Application.Veille.Marketplace.CreateUserVeillePack;
using Atlas.Application.Veille.Marketplace.GetMyAuthoredPacks;
using Atlas.Application.Veille.Marketplace.LikeVeillePack;
using Atlas.Application.Veille.Marketplace.ListPublicMarketplace;
using Atlas.Application.Veille.Marketplace.PublishVeillePack;
using Atlas.Application.Veille.Marketplace.ReportVeillePack;
using Atlas.Application.Veille.Marketplace.UnlikeVeillePack;
using Atlas.Application.Veille.Marketplace.UnpublishVeillePack;
using Atlas.Application.Veille.SyncVeillePack;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class VeillePackEndpoints
{
    public static IEndpointRouteBuilder MapVeillePackEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/veille/packs").WithTags("Veille").RequireAuthorization();

        // Catalogue des packs proposés (F-042).
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            Result<IReadOnlyList<VeillePackDto>> result = await sender.Send(new GetVeilleCatalogQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
        });

        // Packs déjà appliqués par l'utilisateur, avec signal de mise à jour disponible.
        group.MapGet("/mine", GetMineAsync);

        // Appliquer un pack : s'abonner à toutes ses sources.
        group.MapPost("/{code}/apply", ApplyAsync);

        // Re-synchroniser un pack déjà appliqué sur sa version courante.
        group.MapPost("/{code}/sync", SyncAsync);

        // F-049 marketplace.
        group.MapGet("/community", ListCommunityAsync);
        group.MapPost("/user", CreateUserPackAsync);
        group.MapGet("/mine/authored", GetMyAuthoredAsync);
        group.MapPatch("/user/{code}/publish", PublishAsync);
        group.MapPatch("/user/{code}/unpublish", UnpublishAsync);
        group.MapPost("/{code}/like", LikeAsync);
        group.MapDelete("/{code}/like", UnlikeAsync);
        group.MapPost("/{code}/report", ReportAsync);

        return routes;
    }

    private static async Task<IResult> GetMineAsync(CurrentUser user, ISender sender, CancellationToken ct)
    {
        Result<IReadOnlyList<MyVeillePackDto>> result = await sender.Send(new GetMyVeillePacksQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> ApplyAsync(CurrentUser user, string code, ISender sender, CancellationToken ct)
    {
        Result<ApplyVeillePackResult> result = await sender.Send(new ApplyVeillePackCommand(user.Id, code), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> SyncAsync(CurrentUser user, string code, ISender sender, CancellationToken ct)
    {
        Result<ApplyVeillePackResult> result = await sender.Send(new SyncVeillePackCommand(user.Id, code), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    // ---- F-049 marketplace ----

    private static async Task<IResult> ListCommunityAsync(
        ISender sender, int? page, int? pageSize, CancellationToken ct)
    {
        Result<PagedResult<VeillePackMarketplaceDto>> result = await sender.Send(
            new ListPublicMarketplaceQuery(page ?? 1, pageSize ?? 20), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> CreateUserPackAsync(
        CurrentUser user,
        CreateUserVeillePackRequest request,
        ISender sender,
        CancellationToken ct)
    {
        Result<VeillePackMarketplaceDto> result = await sender.Send(
            new CreateUserVeillePackCommand(
                user.Id,
                request.Code,
                request.Name,
                request.Description ?? string.Empty,
                request.SubscriptionIds ?? []),
            ct);

        return result.IsSuccess
            ? Results.Created($"/veille/packs/user/{result.Value!.Code}", result.Value)
            : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetMyAuthoredAsync(
        CurrentUser user, ISender sender, CancellationToken ct)
    {
        Result<IReadOnlyList<VeillePackMarketplaceDto>> result =
            await sender.Send(new GetMyAuthoredPacksQuery(user.Id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> PublishAsync(
        CurrentUser user, string code, ISender sender, CancellationToken ct)
    {
        Result<VeillePackMarketplaceDto> result =
            await sender.Send(new PublishVeillePackCommand(user.Id, code), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> UnpublishAsync(
        CurrentUser user, string code, ISender sender, CancellationToken ct)
    {
        Result<VeillePackMarketplaceDto> result =
            await sender.Send(new UnpublishVeillePackCommand(user.Id, code), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> LikeAsync(
        CurrentUser user, string code, ISender sender, CancellationToken ct)
    {
        Result result = await sender.Send(new LikeVeillePackCommand(user.Id, code), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> UnlikeAsync(
        CurrentUser user, string code, ISender sender, CancellationToken ct)
    {
        Result result = await sender.Send(new UnlikeVeillePackCommand(user.Id, code), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static async Task<IResult> ReportAsync(
        CurrentUser user,
        string code,
        ReportVeillePackRequest request,
        ISender sender,
        CancellationToken ct)
    {
        Result result = await sender.Send(
            new ReportVeillePackCommand(user.Id, code, request.Reason),
            ct);
        return result.IsSuccess ? Results.Accepted() : result.Error!.ToProblem();
    }

    private sealed record CreateUserVeillePackRequest(
        string Code,
        string Name,
        string? Description,
        IReadOnlyList<Guid>? SubscriptionIds);

    private sealed record ReportVeillePackRequest(string Reason);
}
