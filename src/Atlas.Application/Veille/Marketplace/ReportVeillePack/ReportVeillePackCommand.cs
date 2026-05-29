using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.ReportVeillePack;

/// <summary>
/// Signale un pack public pour modération (F-049). Un utilisateur ne peut signaler qu'une fois
/// le même pack tant que son signalement précédent est en <c>Pending</c>.
/// </summary>
public sealed record ReportVeillePackCommand(Guid UserId, string Code, string Reason) : IRequest<Result>;
