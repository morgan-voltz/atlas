using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.GetMyCompanyFavorites;

public sealed record GetMyCompanyFavoritesQuery(Guid UserId)
    : IRequest<Result<IReadOnlyList<CompanyFavoriteDto>>>;
