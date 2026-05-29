using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.GetMyAuthoredPacks;

/// <summary>Packs créés par l'utilisateur (inclut brouillons et publiés, F-049).</summary>
public sealed record GetMyAuthoredPacksQuery(Guid UserId)
    : IRequest<Result<IReadOnlyList<VeillePackMarketplaceDto>>>;
