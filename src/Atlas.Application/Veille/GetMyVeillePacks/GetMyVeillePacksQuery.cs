using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetMyVeillePacks;

/// <summary>Packs appliqués par l'utilisateur, avec indication d'une mise à jour disponible (F-042).</summary>
public sealed record GetMyVeillePacksQuery(Guid UserId) : IRequest<Result<IReadOnlyList<MyVeillePackDto>>>;

public sealed record MyVeillePackDto(
    string Code,
    string Name,
    int AppliedVersion,
    int CurrentVersion,
    bool UpdateAvailable,
    DateTimeOffset AppliedAt);
