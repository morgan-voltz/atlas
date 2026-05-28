using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.AddCompanyFavorite;

internal sealed class AddCompanyFavoriteHandler(
    ICompanyFavoriteRepository favorites,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<AddCompanyFavoriteCommand, Result>
{
    public async Task<Result> Handle(AddCompanyFavoriteCommand request, CancellationToken cancellationToken)
    {
        Result<Siren> parsed = Siren.Create(request.SirenValue);
        if (parsed.IsFailure)
        {
            return Result.Fail(CompanyFavoriteErrors.InvalidSiren(request.SirenValue));
        }

        Siren siren = parsed.Value;
        var userId = new UserId(request.UserId);

        CompanyFavorite? existing =
            await favorites.GetByUserAndSirenAsync(userId, siren, cancellationToken);
        if (existing is not null)
        {
            return Result.Fail(CompanyFavoriteErrors.AlreadyFavorite(siren.Value));
        }

        CompanyFavorite favorite = CompanyFavorite.Mark(userId, siren, request.NameSnapshot, clock.UtcNow);
        await favorites.AddAsync(favorite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
