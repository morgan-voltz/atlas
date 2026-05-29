using Atlas.Application.Downloads;

namespace Atlas.Api.Downloads;

/// <summary>
/// Fallback de <see cref="IBulkDownloadEnqueuer"/> utilisé quand Hangfire est désactivé
/// (`BackgroundJobs:Enabled=false`, typiquement les tests d'intégration). Le job reste
/// en base au statut <c>Pending</c> : c'est conforme à l'attendu du POST `/downloads/bulk`
/// (202 + jobId) sans exécuter le job réel.
/// </summary>
internal sealed class LoggingBulkDownloadEnqueuer(ILogger<LoggingBulkDownloadEnqueuer> logger)
    : IBulkDownloadEnqueuer
{
    public void Enqueue(Guid jobId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Bulk download : job {JobId} créé mais Hangfire est désactivé — il restera Pending.",
                jobId);
        }
    }
}
