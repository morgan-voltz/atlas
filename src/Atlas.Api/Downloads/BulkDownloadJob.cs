using Atlas.Application.Downloads.RunBulkDownload;
using Atlas.Shared.Result;
using Hangfire;
using MediatR;

namespace Atlas.Api.Downloads;

/// <summary>
/// Job Hangfire (F-014) qui exécute le téléchargement en masse en arrière-plan.
/// Démarre 3 retries automatiques (par défaut Hangfire) en cas d'exception ; le handler
/// `RunBulkDownloadHandler` est idempotent et résistant aux re-exécutions.
/// </summary>
public sealed class BulkDownloadJob(ISender sender, ILogger<BulkDownloadJob> logger)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task RunAsync(Guid jobId)
    {
        Result result = await sender.Send(new RunBulkDownloadCommand(jobId));

        // Le handler capture les échecs métier (INPI down, etc.) et marque l'entité comme Failed
        // → le job retourne Ok dans ces cas. Un Result.Fail ici signale uniquement les erreurs
        // structurelles (job introuvable, etc.).
        if (result.IsFailure && logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Bulk download : job {JobId} non traité ({Code}).",
                jobId,
                result.Error!.Code);
        }
    }
}
