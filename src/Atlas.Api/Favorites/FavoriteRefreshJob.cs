using Atlas.Application.Favorites.FavoriteRefresh;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Favorites;

/// <summary>
/// Job Hangfire récurrent (F-019) : déclenche le rafraîchissement quotidien des favoris
/// (re-fetch RNE, détection de changements, publication des notifications email + push).
/// Public car instancié par Hangfire depuis le conteneur DI.
/// </summary>
public sealed class FavoriteRefreshJob(ISender sender, ILogger<FavoriteRefreshJob> logger)
{
    public async Task RunAsync()
    {
        Result<FavoriteRefreshSummary> result = await sender.Send(new RefreshFavoritesCommand());

        if (result.IsSuccess)
        {
            FavoriteRefreshSummary summary = result.Value!;
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Favoris : {Users} user(s), {Favorites} favori(s) traité(s), {Changes} changement(s), {Failed} échec(s).",
                    summary.UsersProcessed,
                    summary.FavoritesProcessed,
                    summary.FavoritesWithChanges,
                    summary.FavoritesFailed);
            }
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("Favoris : échec du rafraîchissement ({Code}).", result.Error!.Code);
        }
    }
}
