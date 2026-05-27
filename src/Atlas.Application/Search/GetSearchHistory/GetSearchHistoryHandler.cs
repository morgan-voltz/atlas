using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Search.GetSearchHistory;

internal sealed class GetSearchHistoryHandler(ISearchHistoryRepository searchHistoryRepository)
    : IRequestHandler<GetSearchHistoryQuery, Result<IReadOnlyList<SearchHistoryEntryDto>>>
{
    private const int Limit = 200;

    public async Task<Result<IReadOnlyList<SearchHistoryEntryDto>>> Handle(
        GetSearchHistoryQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SearchHistoryEntry> entries =
            await searchHistoryRepository.GetRecentByUserAsync(new UserId(request.UserId), Limit, cancellationToken);

        IReadOnlyList<SearchHistoryEntryDto> dto = entries
            .Select(entry => new SearchHistoryEntryDto(entry.Type.ToString(), entry.Query, entry.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<SearchHistoryEntryDto>>.Ok(dto);
    }
}
