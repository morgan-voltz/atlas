using Atlas.Application.Veille.ApplyVeillePack;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.SyncVeillePack;

/// <summary>Re-synchronise un VeillePack déjà appliqué : abonne aux nouvelles sources de la version courante (F-042).</summary>
public sealed record SyncVeillePackCommand(Guid UserId, string PackCode) : IRequest<Result<ApplyVeillePackResult>>;
