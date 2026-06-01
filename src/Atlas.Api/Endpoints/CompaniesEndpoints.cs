using Atlas.Api.Reports;
using Atlas.Api.Security;
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
        // F-022 — rapport PDF de fiche entreprise. Rate limiting dédié (M3) : génération PDF + 2 appels INPI.
        group.MapGet("/{siren}/report.pdf", DownloadReportPdfAsync).RequireRateLimiting(RateLimitOptions.ExpensivePolicy);

        return routes;
    }

    private static async Task<IResult> SearchByNameAsync(
        string? name,
        int? page,
        int? pageSize,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        var query = new SearchCompaniesByNameQuery(user.Id, name ?? string.Empty, page ?? 1, pageSize ?? 20);
        Result<PagedResult<CompanySummaryDto>> result = await sender.Send(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetBySirenAsync(
        string siren,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result<CompanyDto> result = await sender.Send(new GetCompanyBySirenQuery(user.Id, siren), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> GetAttachmentsAsync(
        string siren,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result<IReadOnlyList<CompanyAttachmentDto>> result =
            await sender.Send(new GetCompanyAttachmentsQuery(user.Id, siren), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DownloadAttachmentAsync(
        string siren,
        string attachmentId,
        CurrentUser user,
        ISender sender,
        CancellationToken ct)
    {
        Result<AttachmentContent> result =
            await sender.Send(new DownloadCompanyAttachmentQuery(user.Id, siren, attachmentId), ct);

        if (result.IsFailure)
        {
            return result.Error!.ToProblem();
        }

        AttachmentContent content = result.Value!;
        // Streamé directement vers le client. ASP.NET dispose le stream à la fin de la réponse.
        return Results.Stream(content.Stream, content.ContentType, content.FileName);
    }

    private static async Task<IResult> DownloadReportPdfAsync(
        string siren,
        CurrentUser user,
        ISender sender,
        TimeProvider clock,
        CancellationToken ct)
    {
        Result<CompanyDto> companyResult = await sender.Send(new GetCompanyBySirenQuery(user.Id, siren), ct);
        if (companyResult.IsFailure)
        {
            return companyResult.Error!.ToProblem();
        }

        // Les attachments sont best-effort : si la liste échoue (404, INPI down), on génère
        // le PDF avec une liste vide plutôt que de refuser tout le rapport.
        Result<IReadOnlyList<CompanyAttachmentDto>> attachmentsResult =
            await sender.Send(new GetCompanyAttachmentsQuery(user.Id, siren), ct);
        IReadOnlyList<CompanyAttachmentDto> attachments = attachmentsResult.IsSuccess
            ? attachmentsResult.Value!
            : [];

        byte[] pdf = CompanyReportRenderer.Render(companyResult.Value!, attachments, clock.GetUtcNow());
        return Results.File(pdf, "application/pdf", $"atlas-{siren}.pdf");
    }
}
