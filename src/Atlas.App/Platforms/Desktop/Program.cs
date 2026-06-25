using Uno.UI.Hosting;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace Atlas.App;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
#if DEBUG
        // DevEye (dev-only) : etat client + logs du client sur 127.0.0.1:31337
        // pour donner « yeux + mains » a Claude via MCP. Tete desktop uniquement
        // (la tete WASM ne peut pas ouvrir de socket TCP).
        var devEyeBuffer = new Atlas.DevEye.DevEyeLogBuffer();
        App.InitializeLogging(loggingBuilder => loggingBuilder.AddProvider(
            new Atlas.App.Services.DevEye.DevEyeLoggerProvider(devEyeBuffer)));
        var devEyeServer = new Atlas.DevEye.DevEyeServer(
            new Atlas.App.Services.DevEye.AtlasDevEyeStateProvider(),
            devEyeBuffer,
            new Atlas.DevEye.DevEyeCommandRegistry(),
            Atlas.DevEye.DevEyeServer.PortFromEnv(31337));
        devEyeServer.Start();
#else
        App.InitializeLogging();
#endif

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseX11()
            .UseLinuxFrameBuffer()
            .UseMacOS()
            .UseWin32()
            .Build();

        host.Run();
    }
}
