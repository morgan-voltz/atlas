using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.PublishVeillePack;

/// <summary>
/// Publie un pack utilisateur (<c>Private</c> → <c>Public</c>, F-049). Idempotent.
/// </summary>
public sealed record PublishVeillePackCommand(Guid UserId, string Code) : IRequest<Result<VeillePackMarketplaceDto>>;
