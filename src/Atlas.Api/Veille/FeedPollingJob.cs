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
    }
}
