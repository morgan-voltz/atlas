using System.Text.Json;

namespace Atlas.DevEye;

/// <summary>Descripteur d'une commande logique exposee (decouverte).</summary>
public sealed record CommandDescriptor(string Name, string Description);

/// <summary>
/// Registre des commandes logiques invocables par DevEye (miroir de
/// <c>deveye_sdk::expose_route</c> cote Rust). L'hote enregistre ses actions
/// via <see cref="Register"/> au demarrage ; DevEye les decouvre
/// (<c>GET /devtools/commands</c>) et les declenche
/// (<c>POST /devtools/command</c>).
/// </summary>
public interface IDevEyeCommandRegistry
{
    void Register(string name, string description, Func<JsonElement, object?> handler);

    IReadOnlyList<CommandDescriptor> List();

    /// <summary>
    /// Invoque la commande <paramref name="name"/>. Retourne <c>false</c> si
    /// inconnue (le serveur repond alors 404). <paramref name="result"/> est la
    /// valeur JSON brute renvoyee par le handler (sans enveloppe).
    /// </summary>
    bool TryInvoke(string name, JsonElement payload, out object? result);
}

/// <summary>Implementation thread-safe (ecriture rare au demarrage).</summary>
public sealed class DevEyeCommandRegistry : IDevEyeCommandRegistry, IDisposable
{
    private readonly Dictionary<string, (string Description, Func<JsonElement, object?> Handler)> _routes = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public void Register(string name, string description, Func<JsonElement, object?> handler)
    {
        _lock.EnterWriteLock();
        try { _routes[name] = (description, handler); }
        finally { _lock.ExitWriteLock(); }
    }

    public IReadOnlyList<CommandDescriptor> List()
    {
        _lock.EnterReadLock();
        try
        {
            return _routes
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => new CommandDescriptor(kv.Key, kv.Value.Description))
                .ToList();
        }
        finally { _lock.ExitReadLock(); }
    }

    public bool TryInvoke(string name, JsonElement payload, out object? result)
    {
        Func<JsonElement, object?>? handler = null;
        _lock.EnterReadLock();
        try
        {
            if (_routes.TryGetValue(name, out (string Description, Func<JsonElement, object?> Handler) entry))
            {
                handler = entry.Handler;
            }
        }
        finally { _lock.ExitReadLock(); }

        // Handler execute HORS verrou (il pourrait lui-meme toucher le registre).
        if (handler is null)
        {
            result = null;
            return false;
        }
        result = handler(payload);
        return true;
    }

    public void Dispose() => _lock.Dispose();
}
