using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Atlas.DevEye;

/// <summary>
/// Mini-serveur HTTP local implementant le wire protocol DevEye sur
/// <c>127.0.0.1:&lt;port&gt;</c> (cf. DevEye/docs/wire-protocol.md). Base sur
/// <see cref="HttpListener"/> (BCL pur). Dev-only.
///
/// En contexte navigateur (tete WASM), <see cref="Start"/> est un no-op :
/// aucun socket TCP n'est disponible.
/// </summary>
public sealed class DevEyeServer : IDisposable
{
    /// <summary>Version du wire protocol, emise dans chaque reponse.</summary>
    public const string ProtocolVersion = "1";

    public const int DefaultPort = 31337;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IDevEyeStateProvider _state;
    private readonly DevEyeLogBuffer _logs;
    private readonly IDevEyeCommandRegistry _commands;
    private readonly int _port;

    private HttpListener? _listener;
    private CancellationTokenSource? _cts;

    public DevEyeServer(
        IDevEyeStateProvider state,
        DevEyeLogBuffer logs,
        IDevEyeCommandRegistry commands,
        int port)
    {
        _state = state;
        _logs = logs;
        _commands = commands;
        _port = port;
    }

    /// <summary>Port depuis <c>DEVEYE_PORT</c>, sinon <paramref name="fallback"/>.</summary>
    public static int PortFromEnv(int fallback = DefaultPort)
        => int.TryParse(Environment.GetEnvironmentVariable("DEVEYE_PORT"), out int p) ? p : fallback;

    /// <summary>
    /// Demarre l'ecoute en tache de fond. No-op dans le navigateur. Silencieux
    /// si le port est deja pris (une autre instance ecoute deja) — comme le SDK
    /// Rust.
    /// </summary>
    public void Start()
    {
        if (OperatingSystem.IsBrowser())
        {
            return; // tete WASM : pas de serveur TCP.
        }
        try
        {
            HttpListener listener = new();
            // Deux prefixes loopback-only : un client peut viser `localhost`
            // (defaut de deveye-server) ou `127.0.0.1`. JAMAIS `+`/`*` (qui
            // ecouterait sur 0.0.0.0). Le filtrage par hote du prefixe agit
            // deja comme garde anti DNS-rebinding ; HostIsLocal la double.
            listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
            listener.Prefixes.Add($"http://localhost:{_port}/");
            listener.Start();
            _listener = listener;
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            _ = Task.Run(() => AcceptLoopAsync(listener, token), token);
        }
        catch
        {
            // Dev-only : port indisponible -> on n'embete pas l'app hote.
        }
    }

    private async Task AcceptLoopAsync(HttpListener listener, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch
            {
                break; // listener arrete.
            }
            _ = Task.Run(() => HandleAsync(ctx), ct);
        }
    }

    private async Task HandleAsync(HttpListenerContext ctx)
    {
        try
        {
            ctx.Response.Headers["X-DevEye-Protocol-Version"] = ProtocolVersion;

            // Garde anti DNS-rebinding (miroir de guard_local_host Rust).
            if (!HostIsLocal(ctx.Request.UserHostName ?? string.Empty))
            {
                await WriteJsonAsync(ctx, 403, new { error = "Host non local" }).ConfigureAwait(false);
                return;
            }

            string path = ctx.Request.Url?.AbsolutePath ?? string.Empty;
            string method = ctx.Request.HttpMethod;

            switch (method, path)
            {
                case ("GET", "/devtools/state"):
                    await WriteJsonAsync(ctx, 200, new { entries = _state.GetCurrentState() }).ConfigureAwait(false);
                    break;

                case ("GET", "/devtools/logs/poll"):
                    await WriteJsonAsync(ctx, 200, new { events = _logs.DrainAll().Select(ToWire) }).ConfigureAwait(false);
                    break;

                case ("GET", "/devtools/commands"):
                    await WriteJsonAsync(ctx, 200,
                        _commands.List().Select(d => new { name = d.Name, description = d.Description })).ConfigureAwait(false);
                    break;

                case ("POST", "/devtools/command"):
                    await HandleCommandAsync(ctx).ConfigureAwait(false);
                    break;

                case ("POST", "/devtools/eval"):
                    // Pas de pompe webview cote .NET (Skia desktop / WASM) : 503,
                    // comme le SDK Rust sans pompe enregistree.
                    await WriteJsonAsync(ctx, 503,
                        new { error = "eval JS indisponible cote .NET (aucune pompe webview)" }).ConfigureAwait(false);
                    break;

                default:
                    await WriteJsonAsync(ctx, 404, new { error = "route inconnue" }).ConfigureAwait(false);
                    break;
            }
        }
        catch
        {
            try
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.Close();
            }
            catch
            {
                // reponse deja partie / connexion fermee.
            }
        }
    }

    private async Task HandleCommandAsync(HttpListenerContext ctx)
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(ctx.Request.InputStream).ConfigureAwait(false);
        JsonElement root = doc.RootElement;
        string route = root.TryGetProperty("route", out JsonElement r) ? r.GetString() ?? string.Empty : string.Empty;
        JsonElement payload = root.TryGetProperty("payload", out JsonElement p) ? p.Clone() : default;

        if (_commands.TryInvoke(route, payload, out object? result))
        {
            await WriteJsonAsync(ctx, 200, result).ConfigureAwait(false);
        }
        else
        {
            await WriteJsonAsync(ctx, 404, new { error = $"commande inconnue : {route}" }).ConfigureAwait(false);
        }
    }

    private static object ToWire(DevEyeLogEvent e) => new
    {
        level = e.Level,
        target = e.Target,
        message = e.Message,
        timestamp = e.Timestamp,
        fields = e.Fields,
    };

    private static async Task WriteJsonAsync(HttpListenerContext ctx, int status, object? body)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(body, JsonOptions);
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
        ctx.Response.Close();
    }

    /// <summary>
    /// Vrai si <paramref name="host"/> (header <c>Host</c>, port optionnel)
    /// designe la machine locale. Miroir de <c>host_is_local</c> Rust.
    /// </summary>
    internal static bool HostIsLocal(string host)
    {
        host = host.Trim();
        if (host.Length == 0)
        {
            return false;
        }
        // IPv6 litteral : [::1] ou [::1]:port.
        if (host.StartsWith('['))
        {
            int end = host.IndexOf(']');
            return end > 0 && host[1..end] == "::1";
        }
        // Sinon, retirer un eventuel :port.
        int colon = host.LastIndexOf(':');
        string hostname = colon >= 0 ? host[..colon] : host;
        return string.Equals(hostname, "localhost", StringComparison.OrdinalIgnoreCase)
            || hostname == "127.0.0.1";
    }

    public void Dispose()
    {
        try
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            _cts?.Dispose();
        }
        catch
        {
            // best-effort.
        }
    }
}
