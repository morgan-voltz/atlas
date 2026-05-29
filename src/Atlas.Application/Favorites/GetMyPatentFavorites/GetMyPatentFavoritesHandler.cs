using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.GetMyPatentFavorites;

internal sealed class GetMyPatentFavoritesHandler(IPatentFavoriteRepository favorites)
    : IRequestHandler<GetMyPatentFavoritesQuery, Result<IReadOnlyList<PatentFavoriteDto>>>
{
    public async Task<Result<IReadOnlyList<PatentFavoriteDto>>> Handle(
        GetMyPatentFavoritesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<PatentFavorite> items =
            await favorites.GetByUserAsync(new UserId(request.UserId), cancellationToken);

        IReadOnlyList<PatentFavoriteDto> dto = items
            .OrderByDescending(f => f.AddedAt)
            .Select(f => new PatentFavoriteDto(f.PublicationNumber.Value, f.TitleSnapshot, f.AddedAt))
            .ToList();

        return Result<IReadOnlyList<PatentFavoriteDto>>.Ok(dto);
    }
}
