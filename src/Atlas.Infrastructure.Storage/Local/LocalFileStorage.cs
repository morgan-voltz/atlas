using Atlas.Domain.Storage;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Storage.Local;

/// <summary>
/// Implémentation filesystem locale d'<see cref="IFileStorage"/> (F-014). Les blobs sont écrits
/// dans <c>RootPath/&lt;safe-key&gt;</c>. Convient pour dev et déploiement single-node ; un
/// adapter S3 compatible sera ajouté ultérieurement pour la prod multi-node.
/// </summary>
internal sealed class LocalFileStorage(IOptions<LocalFileStorageOptions> options) : IFileStorage
{
    private readonly string _rootPath = EnsureDirectory(ResolveRoot(options.Value.RootPath));

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(content);

        string path = PathForKey(key);
        await using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(file, ct);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken ct = default)
    {
        string path = PathForKey(key);
        if (!File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        string path = PathForKey(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        return Task.CompletedTask;
    }

    private string PathForKey(string key)
    {
        // Sanitize : on remplace les séparateurs de chemin pour éviter les traversées (../).
        string safe = key
            .Replace('/', '_')
            .Replace('\\', '_')
            .Replace("..", "__", StringComparison.Ordinal);
        return Path.Combine(_rootPath, safe);
    }

    // Une config absente ou vide (ex. "RootPath": null dans appsettings) ne doit jamais faire
    // planter le stockage à chaque requête : on retombe sur le répertoire par défaut.
    private static string ResolveRoot(string? configured) =>
        string.IsNullOrWhiteSpace(configured) ? LocalFileStorageOptions.DefaultRootPath : configured;

    private static string EnsureDirectory(string root)
    {
        Directory.CreateDirectory(root);
        return root;
    }
}
