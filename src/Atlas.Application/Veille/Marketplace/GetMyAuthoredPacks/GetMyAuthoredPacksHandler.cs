using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.GetMyAuthoredPacks;

internal sealed class GetMyAuthoredPacksHandler(IVeillePackRepository packRepository)
    : IRequestHandler<GetMyAuthoredPacksQuery, Result<IReadOnlyList<VeillePackMarketplaceDto>>>
{
    public async Task<Result<IReadOnlyList<VeillePackMarketplaceDto>>> Handle(
        GetMyAuthoredPacksQuery request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        IReadOnlyList<VeillePack> packs = await packRepository.GetByAuthorAsync(userId, cancellationToken);
        IReadOnlyList<VeillePackMarketplaceDto> dtos = packs.Select(VeillePackMarketplaceDto.From).ToList();
        return Result<IReadOnlyList<VeillePackMarketplaceDto>>.Ok(dtos);
    }
}
