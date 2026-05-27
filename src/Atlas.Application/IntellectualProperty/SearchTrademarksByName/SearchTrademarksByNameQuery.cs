using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.IntellectualProperty.SearchTrademarksByName;

public sealed record SearchTrademarksByNameQuery(Guid UserId, string Term, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<TrademarkSummaryDto>>>;
