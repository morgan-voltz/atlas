using Atlas.Domain.Common;
using Atlas.Domain.Users;

namespace Atlas.Domain.Downloads;

/// <summary>
/// Demande de téléchargement en masse d'actes et bilans (F-014). Un utilisateur soumet une
/// liste de SIREN ; un job Hangfire récupère les documents puis produit une archive ZIP
/// stockée via <see cref="IFileStorage"/>. L'archive expire au bout de <see cref="ExpiresAt"/>.
/// </summary>
public sealed class BulkDownloadJob : Entity<BulkDownloadJobId>
{
    public const int MaxSirens = 50;
    public const int MaxErrorMessageLength = 1024;

    /// <summary>Joint des SIREN par virgule, longueur calculée pour 50 SIREN × 9 caractères + 49 virgules.</summary>
    public const int SirensColumnMaxLength = 9 * MaxSirens + (MaxSirens - 1);

    private BulkDownloadJob()
        : base(default)
    {
        // Réhydratation EF Core.
    }

    private BulkDownloadJob(
        BulkDownloadJobId id,
        UserId userId,
        string sirens,
        DateTimeOffset requestedAt,
        DateTimeOffset expiresAt)
        : base(id)
    {
        UserId = userId;
        Sirens = sirens;
        Status = BulkDownloadStatus.Pending;
        RequestedAt = requestedAt;
        ExpiresAt = expiresAt;
    }

    public UserId UserId { get; private set; }

    /// <summary>Liste des SIREN séparés par virgule (sérialisée pour persistance simple).</summary>
    public string Sirens { get; private set; } = null!;

    public BulkDownloadStatus Status { get; private set; }

    /// <summary>Clé dans <see cref="IFileStorage"/> une fois l'archive prête.</summary>
    public string? ArchiveKey { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public IReadOnlyList<string> SirenList =>
        string.IsNullOrEmpty(Sirens) ? [] : Sirens.Split(',', StringSplitOptions.RemoveEmptyEntries);

    public static BulkDownloadJob Request(UserId userId, IReadOnlyList<string> sirens, DateTimeOffset now, TimeSpan ttl)
    {
        ArgumentNullException.ThrowIfNull(sirens);

        if (sirens.Count is 0 or > MaxSirens)
        {
            throw new ArgumentException($"Nombre de SIREN requis entre 1 et {MaxSirens}.", nameof(sirens));
        }

        return new BulkDownloadJob(
            BulkDownloadJobId.New(),
            userId,
            string.Join(',', sirens),
            now,
            now + ttl);
    }

    public void MarkAsRunning() => Status = BulkDownloadStatus.Running;

    public void MarkAsReady(string archiveKey, DateTimeOffset completedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archiveKey);
        Status = BulkDownloadStatus.Ready;
        ArchiveKey = archiveKey;
        CompletedAt = completedAt;
    }

    public void MarkAsFailed(string errorMessage, DateTimeOffset completedAt)
    {
        string normalized = (errorMessage ?? "Erreur inconnue").Trim();
        if (normalized.Length > MaxErrorMessageLength)
        {
            normalized = normalized[..MaxErrorMessageLength];
        }

        Status = BulkDownloadStatus.Failed;
        ErrorMessage = normalized;
        CompletedAt = completedAt;
    }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;
}
