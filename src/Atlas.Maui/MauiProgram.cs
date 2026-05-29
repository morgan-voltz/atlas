using Atlas.Maui.Services;
using Atlas.Maui.Theming;
using Atlas.Maui.ViewModels;
using Atlas.Maui.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace Atlas.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                // Lot 5d : polices facilitantes (cf. docs/06-accessibilite.md §10.3).
                // SIL OFL 1.1 — assets déposés manuellement dans Resources/Fonts/.
                // Tant qu'un .ttf manque, MAUI logge un warning au build et la font
                // sélectionnée retombe sur OpenSansRegular au runtime (FontFamily inconnue).
                fonts.AddFont("OpenDyslexic-Regular.ttf", "OpenDyslexicRegular");
                fonts.AddFont("AtkinsonHyperlegible-Regular.ttf", "AtkinsonHyperlegibleRegular");
            });

        // BaseUrl résolue par plateforme + override Preferences (cf. AtlasApiOptions).
        builder.Services.AddSingleton<AtlasApiOptions>();

        // HttpClient singleton : conserve le cookie de refresh (httpOnly) entre les appels d'une session.
        builder.Services.AddSingleton(sp =>
            new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<AtlasApiOptions>().BaseUrl) });
        builder.Services.AddSingleton(SecureStorage.Default);
        builder.Services.AddSingleton<ITokenStore, SecureStorageTokenStore>();
        builder.Services.AddSingleton<IAtlasApiClient, AtlasApiClient>();
        builder.Services.AddSingleton<ThemeManager>();
        builder.Services.AddSingleton<IMotionCoordinator, MotionCoordinator>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CompanySearchViewModel>();
        builder.Services.AddTransient<CompanyDetailViewModel>();
        builder.Services.AddTransient<SearchHistoryViewModel>();
        builder.Services.AddTransient<AccessibilityPreferencesViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CompanySearchPage>();
        builder.Services.AddTransient<CompanyDetailPage>();
        builder.Services.AddTransient<SearchHistoryPage>();
        builder.Services.AddTransient<AccessibilityPreferencesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
