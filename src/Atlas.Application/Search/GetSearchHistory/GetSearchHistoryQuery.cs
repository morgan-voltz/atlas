using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Search.GetSearchHistory;

public sealed record GetSearchHistoryQuery(Guid UserId) : IRequest<Result<IReadOnlyList<SearchHistoryEntryDto>>>;
