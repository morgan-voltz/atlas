using Atlas.Application.Search;
using Atlas.Domain.Companies;
using Atlas.Domain.Inpi;
using Atlas.Domain.Search;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Companies.SearchCompaniesByName;

internal sealed class SearchCompaniesByNameHandler(
    IInpiCredentialsRepository inpiCredentialsRepository,
    ICryptoService cryptoService,
    ICompanyDataProvider companyDataProvider,
    IPublisher publisher)
    : IRequestHandler<SearchCompaniesByNameQuery, Result<PagedResult<CompanySummaryDto>>>
{
    public async Task<Result<PagedResult<CompanySummaryDto>>> Handle(
        SearchCompaniesByNameQuery request,
        CancellationToken cancellationToken)
    {
        InpiCredentials? credentials =
            await inpiCredentialsRepository.GetByUserIdAsync(new UserId(request.UserId), cancellationToken);
        if (credentials is null)
        {
            return Result<PagedResult<CompanySummaryDto>>.Fail(InpiErrors.NotConnected);
        }

        var access = new InpiAccessCredentials(
            cryptoService.Decrypt(credentials.EncryptedUsername),
            cryptoService.Decrypt(credentials.EncryptedPassword));

        var query = new CompanySearchQuery(request.Term, request.Page, request.PageSize);

        Result<PagedResult<CompanySummary>> search =
            await companyDataProvider.SearchByNameAsync(query, access, cancellationToken);
        if (search.IsFailure)
        {
            return Result<PagedResult<CompanySummaryDto>>.Fail(search.Error!);
        }

        PagedResult<CompanySummary> page = search.Value!;
        IReadOnlyList<CompanySummaryDto> items = page.Items
            .Select(summary => new CompanySummaryDto(
                summary.Siren.Value,
                summary.Denomination,
                summary.Ville,
                summary.ActivitePrincipale?.Code))
            .ToList();

        await publisher.Publish(
            new SearchPerformedNotification(request.UserId, SearchType.CompanyByName, request.Term),
            cancellationToken);

        var dto = new PagedResult<CompanySummaryDto>(items, page.Page, page.PageSize, page.TotalCount);
        return Result<PagedResult<CompanySummaryDto>>.Ok(dto);
    }
}
