using Atlas.Domain.Common;
using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.AddPatentFavorite;

internal sealed class AddPatentFavoriteHandler(
    IPatentFavoriteRepository favorites,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<AddPatentFavoriteCommand, Result>
{
    public async Task<Result> Handle(AddPatentFavoriteCommand request, CancellationToken cancellationToken)
    {
        Result<PublicationNumber> parsed = PublicationNumber.Create(request.PublicationNumber);
        if (parsed.IsFailure)
        {
            return Result.Fail(PatentFavoriteErrors.InvalidPublicationNumber(request.PublicationNumber ?? string.Empty));
        }

        var userId = new UserId(request.UserId);
        PatentFavorite? existing = await favorites.GetByUserAndNumberAsync(userId, parsed.Value, cancellationToken);
        if (existing is not null)
        {
            return Result.Fail(PatentFavoriteErrors.AlreadyFavorite(parsed.Value.Value));
        }

        var favorite = PatentFavorite.Mark(userId, parsed.Value, request.Title, clock.UtcNow);
        await favorites.AddAsync(favorite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
