namespace Atlas.Application.Downloads;

/// <summary>
/// Port d'enfilage d'un job de téléchargement en arrière-plan (F-014). Abstrait le détail
/// de la techno (Hangfire en prod, no-op en tests d'intégration qui désactivent les jobs).
/// </summary>
public interface IBulkDownloadEnqueuer
{
    /// <summary>Enfile le traitement asynchrone d'un job déjà persisté en base.</summary>
    void Enqueue(Guid jobId);
}
