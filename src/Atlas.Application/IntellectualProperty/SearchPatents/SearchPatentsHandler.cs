using Atlas.Application.Inpi;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Security;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.IntellectualProperty.SearchPatents;

internal sealed class SearchPatentsHandler(
    IInpiCredentialsRepository inpiCredentialsRepository,
    ICryptoService cryptoService,
    IIntellectualPropertyProvider provider)
    : IRequestHandler<SearchPatentsQuery, Result<PagedResult<PatentSummaryDto>>>
{
    public async Task<Result<PagedResult<PatentSummaryDto>>> Handle(
        SearchPatentsQuery request,
        CancellationToken cancellationToken)
    {
        string? title = Trim(request.Title);
        string? inventor = Trim(request.Inventor);
        string? applicant = Trim(request.Applicant);

        if (title is null && inventor is null && applicant is null)
        {
            return Result<PagedResult<PatentSummaryDto>>.Fail(PatentErrors.EmptySearch);
        }

        Result<InpiAccessCredentials> access = await InpiAccessResolver.ResolveAsync(
            inpiCredentialsRepository, cryptoService, request.UserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<PagedResult<PatentSummaryDto>>.Fail(access.Error!);
        }

        int page = request.Page < 1 ? 1 : request.Page;
        int pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        Result<PagedResult<PatentSummary>> result = await provider.SearchPatentsAsync(
            new PatentSearchQuery(title, inventor, applicant, page, pageSize),
            access.Value!,
            cancellationToken);
        if (result.IsFailure)
        {
            return Result<PagedResult<PatentSummaryDto>>.Fail(result.Error!);
        }

        PagedResult<PatentSummary> source = result.Value!;
        IReadOnlyList<PatentSummaryDto> items = source.Items
            .Select(p => new PatentSummaryDto(
                p.PublicationNumber.Value,
                p.Title,
                p.Applicant,
                p.DepositDate,
                p.Status))
            .ToList();

        return Result<PagedResult<PatentSummaryDto>>.Ok(
            new PagedResult<PatentSummaryDto>(items, source.Page, source.PageSize, source.TotalCount));
    }

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
