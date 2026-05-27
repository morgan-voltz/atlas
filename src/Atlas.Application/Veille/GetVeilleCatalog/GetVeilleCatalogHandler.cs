using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetVeilleCatalog;

internal sealed class GetVeilleCatalogHandler(IVeillePackRepository packRepository)
    : IRequestHandler<GetVeilleCatalogQuery, Result<IReadOnlyList<VeillePackDto>>>
{
    public async Task<Result<IReadOnlyList<VeillePackDto>>> Handle(
        GetVeilleCatalogQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<VeillePack> packs = await packRepository.GetActiveAsync(cancellationToken);

        IReadOnlyList<VeillePackDto> dtos = packs
            .Select(pack => new VeillePackDto(pack.Code, pack.Name, pack.Description, pack.Version, pack.SourceIds.Count))
            .ToList();

        return Result<IReadOnlyList<VeillePackDto>>.Ok(dtos);
    }
}
