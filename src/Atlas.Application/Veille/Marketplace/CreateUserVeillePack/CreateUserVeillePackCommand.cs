using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.CreateUserVeillePack;

/// <summary>
/// Crée un nouveau <see cref="Atlas.Domain.Veille.VeillePack"/> à partir des abonnements actuels
/// de l'utilisateur (F-049 marketplace). Le pack est initialement en brouillon (<c>Private</c>).
/// </summary>
public sealed record CreateUserVeillePackCommand(
    Guid UserId,
    string Code,
    string Name,
    string Description,
    IReadOnlyList<Guid> SubscriptionIds) : IRequest<Result<VeillePackMarketplaceDto>>;
