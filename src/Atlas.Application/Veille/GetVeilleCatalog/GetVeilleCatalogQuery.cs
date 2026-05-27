using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetVeilleCatalog;

/// <summary>Catalogue des VeillePacks proposés à l'utilisateur (F-042).</summary>
public sealed record GetVeilleCatalogQuery : IRequest<Result<IReadOnlyList<VeillePackDto>>>;

public sealed record VeillePackDto(string Code, string Name, string Description, int Version, int SourceCount);
