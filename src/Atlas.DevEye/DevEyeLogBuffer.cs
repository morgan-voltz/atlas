namespace Atlas.DevEye;

/// <summary>
/// Un evenement de log au format du wire protocol DevEye.
/// <paramref name="Level"/> est deja normalise en minuscules
/// (<c>trace|debug|info|warn|error</c>) par l'adaptateur de l'hote.
/// </summary>
public sealed record DevEyeLogEvent(
    string Level,
    string Target,
    string Message,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, object?>? Fields);

/// <summary>
/// Buffer circulaire borne, draine par <c>GET /devtools/logs/poll</c>.
/// Politique drop-oldest, capacite 1024 (miroir de <c>LOG_BUFFER_CAPACITY</c>
/// du SDK Rust). Thread-safe. Alimente par un adaptateur propre a l'hote
/// (provider Microsoft.Extensions.Logging cote Atlas.App, sink Serilog cote
/// Atlas.Api).
/// </summary>
public sealed class DevEyeLogBuffer
{
    private const int Capacity = 1024;
    private readonly Queue<DevEyeLogEvent> _buffer = new(Capacity);
    private readonly object _lock = new();

    public void Push(DevEyeLogEvent ev)
    {
        lock (_lock)
        {
            if (_buffer.Count >= Capacity)
            {
                _buffer.Dequeue(); // drop-oldest
            }
            _buffer.Enqueue(ev);
        }
    }

    /// <summary>Draine le buffer : retourne et supprime tous les evenements.</summary>
    public IReadOnlyList<DevEyeLogEvent> DrainAll()
    {
        lock (_lock)
        {
            DevEyeLogEvent[] events = _buffer.ToArray();
            _buffer.Clear();
            return events;
        }
    }
}
