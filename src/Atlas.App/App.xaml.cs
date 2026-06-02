using System;
using Microsoft.Extensions.Logging;
using Uno.Resizetizer;

namespace Atlas.App;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        // Composition root (U4.1) : DI + HttpClientFactory.
        Atlas.App.Services.AppServices.Initialize();
    }

    protected Window? MainWindow { get; private set; }

    /// <summary>Fenêtre applicative (exposée pour l'init des sélecteurs de fichiers natifs — RGPD export).</summary>
    internal static Window? WindowInstance { get; private set; }

    /// <summary>Frame racine (héberge LoginPage puis MainPage). Sert à revenir au login après déconnexion/suppression.</summary>
    internal static Frame? RootFrame { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window();
        WindowInstance = MainWindow;
#if DEBUG
        MainWindow.UseStudio();
#endif


        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (MainWindow.Content is not Frame rootFrame)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            // Place the frame in the current Window
            MainWindow.Content = rootFrame;

            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        RootFrame = rootFrame;

        if (rootFrame.Content == null)
        {
            // Démarrage sur la connexion ; après login, la page bascule sur la coque (MainPage).
            // Sur la tête WebAssembly, un lien profond pré-auth (activation d'e-mail, réinitialisation
            // de mot de passe) route directement vers l'écran d'onboarding adéquat (doc 14 §4).
            (Type page, object? parameter) = ResolveLaunchNavigation(args.Arguments);
            rootFrame.Navigate(page, parameter);
        }

        MainWindow.SetWindowIcon();
        // Ensure the current window is active
        MainWindow.Activate();
    }

    // Résout la page de démarrage (et son paramètre) depuis l'URL pré-auth sur WebAssembly. Les liens
    // d'email (activation, réinitialisation) sont des liens profonds : ils doivent ouvrir l'écran cible
    // sans détour par la connexion. Les destinations du rail (accueil, recherche…) exigent l'auth : on
    // démarre alors sur la connexion et MainPage relit le hash après login. No-op hors WASM.
    private static (Type Page, object? Parameter) ResolveLaunchNavigation(object? defaultArguments)
    {
        var login = (typeof(Atlas.App.Presentation.Pages.LoginPage), defaultArguments);

        if (Atlas.App.Presentation.WasmRouting.GetTag() is not { } tag)
        {
            return login;
        }

        return tag switch
        {
            "inscription" => (typeof(Atlas.App.Presentation.Pages.InscriptionPage), null),
            "mot-de-passe-oublie" => (typeof(Atlas.App.Presentation.Pages.MotDePasseOubliePage), null),
            "verifier-email" => (
                typeof(Atlas.App.Presentation.Pages.VerifierEmailPage),
                new Atlas.App.Presentation.Pages.VerifyEmailArgs(
                    Atlas.App.Presentation.WasmRouting.GetQueryValue("email"),
                    Atlas.App.Presentation.WasmRouting.GetQueryValue("userId"),
                    Atlas.App.Presentation.WasmRouting.GetQueryValue("token"))),
            "reinitialiser-mot-de-passe" => (
                typeof(Atlas.App.Presentation.Pages.ReinitialiserMotDePassePage),
                new Atlas.App.Presentation.Pages.ResetPasswordArgs(
                    Atlas.App.Presentation.WasmRouting.GetQueryValue("userId"),
                    Atlas.App.Presentation.WasmRouting.GetQueryValue("token"))),
            _ => login,
        };
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
    }

    /// <summary>
    /// Configures global Uno Platform logging
    /// </summary>
    public static void InitializeLogging()
    {
#if DEBUG
        // Logging is disabled by default for release builds, as it incurs a significant
        // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
        // is a concern for your application, keep this disabled. If you're running on the web or
        // desktop targets, you can use URL or command line parameters to enable it.
        //
        // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

        var factory = LoggerFactory.Create(builder =>
        {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());

            // Log to the Visual Studio Debug console
            builder.AddConsole();
#else
            builder.AddConsole();
#endif

            // Exclude logs below this level
            builder.SetMinimumLevel(LogLevel.Information);

            // Default filters for Uno Platform namespaces
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);

            // Generic Xaml events
            // builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.VisualStateGroup", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.StateTriggerBase", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace );

            // Layouter specific messages
            // builder.AddFilter("Microsoft.UI.Xaml.Controls", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug );

            // builder.AddFilter("Windows.Storage", LogLevel.Debug );

            // Binding related messages
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );

            // Binder memory references tracking
            // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

            // DevServer and HotReload related
            // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

            // Debug JS interop
            // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
