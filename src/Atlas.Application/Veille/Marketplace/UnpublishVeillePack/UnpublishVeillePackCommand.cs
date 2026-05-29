using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.UnpublishVeillePack;

/// <summary>
/// Repasse un pack publié en brouillon (<c>Public</c> → <c>Private</c>, F-049). Idempotent.
/// </summary>
public sealed record UnpublishVeillePackCommand(Guid UserId, string Code) : IRequest<Result<VeillePackMarketplaceDto>>;
