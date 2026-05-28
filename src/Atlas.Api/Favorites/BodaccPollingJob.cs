using Atlas.Application.Favorites.PollBodacc;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Favorites;

/// <summary>
/// Job Hangfire récurrent (F-048) : interroge BODACC pour les SIREN favoris, dédupliqués cross-users.
/// Crée des <c>FavoriteEvent</c> de type <c>BodaccPublished</c> qui apparaîtront dans la timeline
/// via la fusion F-047 volet 2.
/// </summary>
public sealed class BodaccPollingJob(ISender sender, ILogger<BodaccPollingJob> logger)
{
    public async Task RunAsync()
    {
        Result<BodaccPollSummary> result = await sender.Send(new PollBodaccForFavoritesCommand());

        if (result.IsSuccess)
        {
            BodaccPollSummary summary = result.Value!;
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "BODACC : {Sirens} SIREN interrogé(s), {Failed} échec(s), {Events} événement(s) créé(s).",
                    summary.SirensQueried,
                    summary.SirensFailed,
                    summary.EventsCreated);
            }
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("BODACC : échec du polling ({Code}).", result.Error!.Code);
        }
    }
}
