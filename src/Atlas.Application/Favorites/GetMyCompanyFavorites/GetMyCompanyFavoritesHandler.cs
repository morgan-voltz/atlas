using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.GetMyCompanyFavorites;

internal sealed class GetMyCompanyFavoritesHandler(ICompanyFavoriteRepository favorites)
    : IRequestHandler<GetMyCompanyFavoritesQuery, Result<IReadOnlyList<CompanyFavoriteDto>>>
{
    public async Task<Result<IReadOnlyList<CompanyFavoriteDto>>> Handle(
        GetMyCompanyFavoritesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CompanyFavorite> items =
            await favorites.GetByUserAsync(new UserId(request.UserId), cancellationToken);

        IReadOnlyList<CompanyFavoriteDto> dto = items
            .OrderByDescending(favorite => favorite.AddedAt)
            .Select(favorite => new CompanyFavoriteDto(favorite.Siren.Value, favorite.NameSnapshot, favorite.AddedAt))
            .ToList();

        return Result<IReadOnlyList<CompanyFavoriteDto>>.Ok(dto);
    }
}
