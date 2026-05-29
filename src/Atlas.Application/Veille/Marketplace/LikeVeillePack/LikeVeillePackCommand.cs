using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.LikeVeillePack;

/// <summary>
/// Ajoute un like communautaire sur un pack public (F-049). Idempotent : si le user a déjà
/// liké, retourne succès sans incrémenter à nouveau.
/// </summary>
public sealed record LikeVeillePackCommand(Guid UserId, string Code) : IRequest<Result>;
