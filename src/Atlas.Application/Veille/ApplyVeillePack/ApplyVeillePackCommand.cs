using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.ApplyVeillePack;

/// <summary>Applique un VeillePack à l'utilisateur : s'abonne à toutes ses sources (F-042). Idempotent.</summary>
public sealed record ApplyVeillePackCommand(Guid UserId, string PackCode) : IRequest<Result<ApplyVeillePackResult>>;

public sealed record ApplyVeillePackResult(string Code, string Name, int SubscriptionsAdded, int Version);
