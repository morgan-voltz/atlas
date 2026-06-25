#if DEBUG
using Atlas.DevEye;
using Microsoft.Extensions.Logging;

namespace Atlas.App.Services.DevEye;

/// <summary>
/// Provider Microsoft.Extensions.Logging (dev-only) qui pousse les logs du
/// client vers le buffer DevEye, draine par <c>GET /devtools/logs/poll</c>.
/// Seuil INFO (comme le SDK Rust) pour eviter le bruit de framework.
/// </summary>
internal sealed class DevEyeLoggerProvider(DevEyeLogBuffer buffer) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new BufferLogger(buffer, categoryName);

    public void Dispose()
    {
        // Rien a liberer : le buffer est partage avec le serveur.
    }

    private sealed class BufferLogger(DevEyeLogBuffer buffer, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            buffer.Push(new DevEyeLogEvent(
                MapLevel(logLevel),
                category,
                formatter(state, exception),
                DateTimeOffset.UtcNow,
                null));
        }

        private static string MapLevel(LogLevel level) => level switch
        {
            LogLevel.Trace => "trace",
            LogLevel.Debug => "debug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "error",
            LogLevel.Critical => "error",
            _ => "info",
        };
    }
}
#endif
