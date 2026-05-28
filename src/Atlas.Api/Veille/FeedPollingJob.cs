using Atlas.Application.Veille.ClusterPendingFeedItems;
using Atlas.Application.Veille.MatchFavoritesInFeedItems;
using Atlas.Application.Veille.PollFeedSources;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Api.Veille;

/// <summary>
/// Job Hangfire récurrent (F-041) : déclenche le polling des sources de veille via MediatR.
/// Public car instancié par Hangfire depuis le conteneur DI.
/// </summary>
public sealed class FeedPollingJob(ISender sender, ILogger<FeedPollingJob> logger)
{
    public async Task PollAsync()
    {
        Result<FeedPollSummary> result = await sender.Send(new PollFeedSourcesCommand());

        if (result.IsSuccess)
        {
            FeedPollSummary summary = result.Value!;
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Veille : {Polled} source(s) pollée(s), {Added} item(s) ajouté(s), {Failed} échec(s).",
                    summary.SourcesPolled,
                    summary.ItemsAdded,
                    summary.SourcesFailed);
            }
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("Veille : échec du polling ({Code}).", result.Error!.Code);
        }

        // Déduplication intelligente (F-045) : clusterise les nouveaux items (et backfille les anciens au 1er run).
        Result<ClusterRunSummary> clustering = await sender.Send(new ClusterPendingFeedItemsCommand());

        if (clustering.IsSuccess)
        {
            ClusterRunSummary summary = clustering.Value!;
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Veille : déduplication — {Processed} item(s) traité(s), {Created} cluster(s) créé(s), {Clustered} regroupé(s).",
                    summary.ItemsProcessed,
                    summary.ClustersCreated,
                    summary.ItemsClustered);
            }
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("Veille : échec de la déduplication ({Code}).", clustering.Error!.Code);
        }

        // Tagging des items avec les favoris des users (F-047, timeline mixte).
        Result<FavoriteMatchSummary> matching = await sender.Send(new MatchFavoritesInFeedItemsCommand());
        if (matching.IsSuccess)
        {
            FavoriteMatchSummary summary = matching.Value!;
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Veille : matching favoris — {Users} user(s) scanné(s), {Items} item(s) examiné(s), {Matches} mention(s) créée(s).",
                    summary.UsersScanned,
                    summary.ItemsScanned,
                    summary.MatchesCreated);
            }
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("Veille : échec du matching favoris ({Code}).", matching.Error!.Code);
        }
    }
}
