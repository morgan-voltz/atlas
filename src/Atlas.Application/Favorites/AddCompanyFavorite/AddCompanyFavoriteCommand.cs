using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.AddCompanyFavorite;

public sealed record AddCompanyFavoriteCommand(Guid UserId, string SirenValue, string? NameSnapshot)
    : IRequest<Result>;
