using Atlas.Application.Inpi;
using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Favorites.FavoriteRefresh;

internal sealed class RefreshFavoritesHandler(
    ICompanyFavoriteRepository favorites,
    ICompanyFavoriteSnapshotRepository snapshots,
    IUserRepository users,
    IInpiCredentialsRepository inpiCredentials,
    ICryptoService crypto,
    ICompanyDataProvider companyProvider,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<RefreshFavoritesHandler> logger)
    : IRequestHandler<RefreshFavoritesCommand, Result<FavoriteRefreshSummary>>
{
    public async Task<Result<FavoriteRefreshSummary>> Handle(
        RefreshFavoritesCommand request,
        CancellationToken cancellationToken)
    {
        int usersProcessed = 0;
        int favoritesProcessed = 0;
        int favoritesWithChanges = 0;
        int favoritesFailed = 0;

        IReadOnlyList<UserId> userIds = await snapshots.GetUserIdsWithFavoritesAsync(cancellationToken);

        foreach (UserId userId in userIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Sans compte INPI connecté, on ne peut pas appeler le RNE pour ce user — on passe.
            Result<InpiAccessCredentials> access = await InpiAccessResolver.ResolveAsync(
                inpiCredentials, crypto, userId.Value, cancellationToken);
            if (access.IsFailure)
            {
                continue;
            }

            User? user = await users.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                continue;
            }

            usersProcessed++;

            IReadOnlyList<CompanyFavorite> userFavorites = await favorites.GetByUserAsync(userId, cancellationToken);

            // Précharge tous les snapshots du user en une requête (audit Lot 3, E4b) : évite un
            // GetCurrentAsync par favori (N+1). Au plus un snapshot par siren.
            var snapshotsBySiren =
                (await snapshots.GetByUserAsync(userId, cancellationToken)).ToDictionary(snap => snap.Siren);

            foreach (CompanyFavorite favorite in userFavorites)
            {
                cancellationToken.ThrowIfCancellationRequested();
                favoritesProcessed++;

                Result<UniteLegale> companyResult =
                    await companyProvider.GetBySirenAsync(favorite.Siren, access.Value!, cancellationToken);
                if (companyResult.IsFailure)
                {
                    favoritesFailed++;
                    if (logger.IsEnabled(LogLevel.Warning))
                    {
                        logger.LogWarning(
                            "Favoris : impossible de re-fetcher {Siren} pour l'utilisateur {UserId} ({Code}).",
                            favorite.Siren.Value,
                            userId.Value,
                            companyResult.Error!.Code);
                    }
                    continue;
                }

                UniteLegale current = companyResult.Value!;
                snapshotsBySiren.TryGetValue(favorite.Siren, out CompanyFavoriteSnapshot? previous);

                if (previous is null)
                {
                    // Premier passage : on capture le snapshot initial sans publier d'alerte.
                    var initial = CompanyFavoriteSnapshot.Capture(userId, current, clock.UtcNow);
                    await snapshots.AddAsync(initial, cancellationToken);
                    continue;
                }

                IReadOnlyList<CompanyFavoriteChange> changes = previous.DiffWith(current);
                if (changes.Count == 0)
                {
                    continue;
                }

                favoritesWithChanges++;

                await snapshots.RemoveAsync(previous, cancellationToken);
                var refreshed = CompanyFavoriteSnapshot.Capture(userId, current, clock.UtcNow);
                await snapshots.AddAsync(refreshed, cancellationToken);

                await publisher.Publish(
                    new CompanyFavoriteChangedNotification(
                        userId,
                        user.Email,
                        favorite.Siren.Value,
                        current.Denomination,
                        changes),
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<FavoriteRefreshSummary>.Ok(new FavoriteRefreshSummary(
            usersProcessed,
            favoritesProcessed,
            favoritesWithChanges,
            favoritesFailed));
    }
}
