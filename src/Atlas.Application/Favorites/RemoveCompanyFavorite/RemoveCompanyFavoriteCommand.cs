using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.RemoveCompanyFavorite;

public sealed record RemoveCompanyFavoriteCommand(Guid UserId, string SirenValue) : IRequest<Result>;
