namespace Atlas.Infrastructure.Storage.Local;

/// <summary>Configuration du stockage filesystem local (F-014).</summary>
internal sealed class LocalFileStorageOptions
{
    public const string SectionName = "Storage:Local";

    /// <summary>Répertoire racine par défaut, utilisé quand <see cref="RootPath"/> n'est pas configuré.</summary>
    public static string DefaultRootPath { get; } = Path.Combine(Path.GetTempPath(), "atlas-storage");

    /// <summary>Répertoire racine où sont écrits les blobs. Créé au démarrage s'il n'existe pas.</summary>
    public string? RootPath { get; set; } = DefaultRootPath;
}
