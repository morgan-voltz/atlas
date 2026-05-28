using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.RemoveCompanyFavorite;

internal sealed class RemoveCompanyFavoriteHandler(
    ICompanyFavoriteRepository favorites,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveCompanyFavoriteCommand, Result>
{
    public async Task<Result> Handle(RemoveCompanyFavoriteCommand request, CancellationToken cancellationToken)
    {
        Result<Siren> parsed = Siren.Create(request.SirenValue);
        if (parsed.IsFailure)
        {
            return Result.Fail(CompanyFavoriteErrors.InvalidSiren(request.SirenValue));
        }

        Siren siren = parsed.Value;
        var userId = new UserId(request.UserId);

        CompanyFavorite? favorite = await favorites.GetByUserAndSirenAsync(userId, siren, cancellationToken);
        if (favorite is null)
        {
            return Result.Fail(CompanyFavoriteErrors.NotFavorite(siren.Value));
        }

        await favorites.RemoveAsync(favorite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
