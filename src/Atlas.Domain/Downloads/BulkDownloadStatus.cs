namespace Atlas.Domain.Downloads;

/// <summary>État d'un job de téléchargement en masse (F-014).</summary>
public enum BulkDownloadStatus
{
    /// <summary>En attente de prise en charge par le job Hangfire.</summary>
    Pending = 0,

    /// <summary>Job en cours d'exécution (au moins un SIREN traité).</summary>
    Running = 1,

    /// <summary>Archive prête, téléchargement possible jusqu'à <c>ExpiresAt</c>.</summary>
    Ready = 2,

    /// <summary>Échec d'exécution — <c>ErrorMessage</c> contient le détail.</summary>
    Failed = 3,
}
