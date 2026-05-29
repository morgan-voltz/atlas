using Atlas.Domain.Common;
using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.RemovePatentFavorite;

internal sealed class RemovePatentFavoriteHandler(
    IPatentFavoriteRepository favorites,
    IUnitOfWork unitOfWork) : IRequestHandler<RemovePatentFavoriteCommand, Result>
{
    public async Task<Result> Handle(RemovePatentFavoriteCommand request, CancellationToken cancellationToken)
    {
        Result<PublicationNumber> parsed = PublicationNumber.Create(request.PublicationNumber);
        if (parsed.IsFailure)
        {
            return Result.Fail(PatentFavoriteErrors.InvalidPublicationNumber(request.PublicationNumber ?? string.Empty));
        }

        var userId = new UserId(request.UserId);
        PatentFavorite? favorite = await favorites.GetByUserAndNumberAsync(userId, parsed.Value, cancellationToken);
        if (favorite is null)
        {
            return Result.Fail(PatentFavoriteErrors.NotFavorite(parsed.Value.Value));
        }

        await favorites.RemoveAsync(favorite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
