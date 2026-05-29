using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.GetMyPatentFavorites;

public sealed record GetMyPatentFavoritesQuery(Guid UserId)
    : IRequest<Result<IReadOnlyList<PatentFavoriteDto>>>;
