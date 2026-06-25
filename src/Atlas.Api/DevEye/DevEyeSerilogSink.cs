#if DEBUG
using Atlas.DevEye;
using Serilog.Core;
using Serilog.Events;

namespace Atlas.Api.DevEye;

/// <summary>
/// Sink Serilog (dev-only) qui pousse les evenements vers le buffer DevEye,
/// draine par <c>GET /devtools/logs/poll</c>. Niveau normalise au format wire
/// (<c>trace|debug|info|warn|error</c>).
/// </summary>
internal sealed class DevEyeSerilogSink(DevEyeLogBuffer buffer) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        string target = logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? sc)
            ? sc.ToString().Trim('"')
            : "atlas.api";

        IReadOnlyDictionary<string, object?>? fields = logEvent.Properties.Count == 0
            ? null
            : logEvent.Properties.ToDictionary(kv => kv.Key, kv => (object?)kv.Value.ToString());

        buffer.Push(new DevEyeLogEvent(
            MapLevel(logEvent.Level),
            target,
            logEvent.RenderMessage(System.Globalization.CultureInfo.InvariantCulture),
            logEvent.Timestamp,
            fields));
    }

    private static string MapLevel(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => "trace",
        LogEventLevel.Debug => "debug",
        LogEventLevel.Information => "info",
        LogEventLevel.Warning => "warn",
        LogEventLevel.Error => "error",
        LogEventLevel.Fatal => "error",
        _ => "info",
    };
}
#endif
