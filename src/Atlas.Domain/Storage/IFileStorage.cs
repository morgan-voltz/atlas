namespace Atlas.Domain.Storage;

/// <summary>
/// Port de stockage binaire temporaire (F-014). Implémentations possibles : filesystem local,
/// S3 compatible (MinIO, Wasabi, AWS S3…). Le caller fournit une <c>key</c> opaque qui sert
/// à retrouver le blob plus tard. Aucune notion de TTL côté port — l'expiration est gérée par
/// l'application (job de nettoyage des entités <c>BulkDownloadJob</c>).
/// </summary>
public interface IFileStorage
{
    /// <summary>Stocke un blob. Écrase si la clé existe déjà.</summary>
    Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>Ouvre un blob en lecture. Renvoie <c>null</c> si la clé n'existe pas.</summary>
    Task<Stream?> OpenReadAsync(string key, CancellationToken ct = default);

    /// <summary>Supprime un blob. No-op si la clé n'existe pas.</summary>
    Task DeleteAsync(string key, CancellationToken ct = default);
}
