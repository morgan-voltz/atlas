using System.Security.Claims;
using Atlas.Api.Downloads;
using Atlas.Application.Downloads.DownloadBulkArchive;
using Atlas.Application.Downloads.GetBulkDownload;
using Atlas.Application.Downloads.RequestBulkDownload;
using Atlas.Shared.Result;
using Hangfire;
using MediatR;

namespace Atlas.Api.Endpoints;

/// <summary>
/// F-014 — endpoints pour les téléchargements en masse (création de job, suivi, récupération de l'archive).
/// </summary>
internal static class DownloadsEndpoints
{
    public static IEndpointRouteBuilder MapDownloadsEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/downloads").WithTags("Downloads").RequireAuthorization();

        group.MapPost("/bulk", RequestBulkAsync);
        group.MapGet("/bulk/{jobId:guid}", GetBulkAsync);
        group.MapGet("/bulk/{jobId:guid}/archive", DownloadArchiveAsync);

        return routes;
    }

    /// <summary>Corps de la requête de création de job. Liste de SIREN bruts (validation côté handler).</summary>
    public sealed record BulkDownloadRequest(IReadOnlyList<string> Sirens);

    private static async Task<IResult> RequestBulkAsync(
        BulkDownloadRequest body,
        ClaimsPrincipal principal,
        ISender sender,
        IBackgroundJobClient backgroundJobs,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        IReadOnlyList<string> sirens = body?.Sirens ?? [];
        Result<Guid> result = await sender.Send(new RequestBulkDownloadCommand(userId, sirens), ct);
        if (result.IsFailure)
        {
            return result.Error!.ToProblem();
        }

        // Le job est créé en base avant l'enqueue : si Hangfire est down, le user voit Pending
        // et un opérateur peut le relancer manuellement.
        Guid jobId = result.Value;
        backgroundJobs.Enqueue<BulkDownloadJob>(job => job.RunAsync(jobId));

        return Results.Accepted($"/downloads/bulk/{jobId}", new { jobId });
    }

    private static async Task<IResult> GetBulkAsync(
        Guid jobId,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<BulkDownloadJobDto> result = await sender.Send(new GetBulkDownloadQuery(userId, jobId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DownloadArchiveAsync(
        Guid jobId,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<BulkArchiveContent> result = await sender.Send(new DownloadBulkArchiveQuery(userId, jobId), ct);
        if (result.IsFailure)
        {
            return result.Error!.ToProblem();
        }

        BulkArchiveContent content = result.Value!;
        return Results.Stream(content.Stream, "application/zip", content.FileName);
    }
}
