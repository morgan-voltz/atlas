using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.UnlikeVeillePack;

/// <summary>Retire un like communautaire (F-049). Idempotent.</summary>
public sealed record UnlikeVeillePackCommand(Guid UserId, string Code) : IRequest<Result>;
