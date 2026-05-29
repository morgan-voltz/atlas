using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.ListPublicMarketplace;

/// <summary>
/// Catalogue communautaire des packs publiés (F-049). Trié par <see cref="Atlas.Domain.Veille.VeillePack.LikesCount"/>
/// décroissant puis date de création décroissante. Pagination 1-based.
/// </summary>
public sealed record ListPublicMarketplaceQuery(int Page, int PageSize)
    : IRequest<Result<PagedResult<VeillePackMarketplaceDto>>>;
