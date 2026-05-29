using System.Security.Claims;
using Atlas.Application.Companies;
using Atlas.Application.Companies.DownloadCompanyAttachment;
using Atlas.Application.Companies.GetCompanyAttachments;
using Atlas.Application.Companies.GetCompanyBySiren;
using Atlas.Application.Companies.SearchCompaniesByName;
using Atlas.Domain.Companies.Attachments;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Endpoints;

internal static class CompaniesEndpoints
{
    public static IEndpointRouteBuilder MapCompaniesEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("/companies").WithTags("Companies").RequireAuthorization();

        group.MapGet("/", SearchByNameAsync);
        group.MapGet("/{siren}", GetBySirenAsync);
        // F-013 — actes et bilans
        group.MapGet("/{siren}/attachments", GetAttachmentsAsync);
        group.MapGet("/{siren}/attachments/{attachmentId}/download", DownloadAttachmentAsync);

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

        var query = new SearchCompaniesByNameQuery(userId, name ?? string.Empty, page ?? 1, pageSize ?? 20);
        Result<PagedResult<CompanySummaryDto>> result = await sender.Send(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetBySirenAsync(
        string siren,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<CompanyDto> result = await sender.Send(new GetCompanyBySirenQuery(userId, siren), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetAttachmentsAsync(
        string siren,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<IReadOnlyList<CompanyAttachmentDto>> result =
            await sender.Send(new GetCompanyAttachmentsQuery(userId, siren), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DownloadAttachmentAsync(
        string siren,
        string attachmentId,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        if (!principal.TryGetUserId(out Guid userId))
        {
            return Results.Unauthorized();
        }

        Result<AttachmentContent> result =
            await sender.Send(new DownloadCompanyAttachmentQuery(userId, siren, attachmentId), ct);

        if (result.IsFailure)
        {
            return result.Error!.ToProblem();
        }

        AttachmentContent content = result.Value!;
        // Streamé directement vers le client. ASP.NET dispose le stream à la fin de la réponse.
        return Results.Stream(content.Stream, content.ContentType, content.FileName);
    }
}
