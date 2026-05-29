using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.ListPublicMarketplace;

internal sealed class ListPublicMarketplaceHandler(IVeillePackRepository packRepository)
    : IRequestHandler<ListPublicMarketplaceQuery, Result<PagedResult<VeillePackMarketplaceDto>>>
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public async Task<Result<PagedResult<VeillePackMarketplaceDto>>> Handle(
        ListPublicMarketplaceQuery request,
        CancellationToken cancellationToken)
    {
        int page = request.Page < 1 ? 1 : request.Page;
        int pageSize = request.PageSize switch
        {
            <= 0 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => request.PageSize,
        };

        PagedResult<VeillePack> paged =
            await packRepository.GetPublicMarketplaceAsync(page, pageSize, cancellationToken);

        IReadOnlyList<VeillePackMarketplaceDto> dtos = paged.Items.Select(VeillePackMarketplaceDto.From).ToList();
        return Result<PagedResult<VeillePackMarketplaceDto>>.Ok(
            new PagedResult<VeillePackMarketplaceDto>(dtos, paged.Page, paged.PageSize, paged.TotalCount));
    }
}
