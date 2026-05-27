using Atlas.Application.Search;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Search;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.IntellectualProperty.SearchTrademarksByName;

internal sealed class SearchTrademarksByNameHandler(
    IInpiCredentialsRepository inpiCredentialsRepository,
    ICryptoService cryptoService,
    IIntellectualPropertyProvider intellectualPropertyProvider,
    IPublisher publisher)
    : IRequestHandler<SearchTrademarksByNameQuery, Result<PagedResult<TrademarkSummaryDto>>>
{
    public async Task<Result<PagedResult<TrademarkSummaryDto>>> Handle(
        SearchTrademarksByNameQuery request,
        CancellationToken cancellationToken)
    {
        InpiCredentials? credentials =
            await inpiCredentialsRepository.GetByUserIdAsync(new UserId(request.UserId), cancellationToken);
        if (credentials is null)
        {
            return Result<PagedResult<TrademarkSummaryDto>>.Fail(InpiErrors.NotConnected);
        }

        var access = new InpiAccessCredentials(
            cryptoService.Decrypt(credentials.EncryptedUsername),
            cryptoService.Decrypt(credentials.EncryptedPassword));

        var query = new TrademarkSearchQuery(request.Term, request.Page, request.PageSize);

        Result<PagedResult<TrademarkSummary>> search =
            await intellectualPropertyProvider.SearchTrademarksAsync(query, access, cancellationToken);
        if (search.IsFailure)
        {
            return Result<PagedResult<TrademarkSummaryDto>>.Fail(search.Error!);
        }

        PagedResult<TrademarkSummary> page = search.Value!;
        IReadOnlyList<TrademarkSummaryDto> items = page.Items
            .Select(trademark => new TrademarkSummaryDto(
                trademark.Denomination,
                trademark.Deposant,
                trademark.DepositNumber.Value,
                trademark.DateDepot,
                trademark.StatutJuridique))
            .ToList();

        await publisher.Publish(
            new SearchPerformedNotification(request.UserId, SearchType.TrademarkByName, request.Term),
            cancellationToken);

        var dto = new PagedResult<TrademarkSummaryDto>(items, page.Page, page.PageSize, page.TotalCount);
        return Result<PagedResult<TrademarkSummaryDto>>.Ok(dto);
    }
}
