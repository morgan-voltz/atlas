using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Companies.SearchCompaniesByName;

public sealed record SearchCompaniesByNameQuery(Guid UserId, string Term, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<CompanySummaryDto>>>;
