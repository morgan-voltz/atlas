using Atlas.Maui.Services;
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
            });

        // HttpClient singleton : conserve le cookie de refresh (httpOnly) entre les appels d'une session.
        builder.Services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri(AtlasApiOptions.BaseUrl) });
        builder.Services.AddSingleton(SecureStorage.Default);
        builder.Services.AddSingleton<ITokenStore, SecureStorageTokenStore>();
        builder.Services.AddSingleton<IAtlasApiClient, AtlasApiClient>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CompanySearchViewModel>();
        builder.Services.AddTransient<CompanyDetailViewModel>();
        builder.Services.AddTransient<SearchHistoryViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CompanySearchPage>();
        builder.Services.AddTransient<CompanyDetailPage>();
        builder.Services.AddTransient<SearchHistoryPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
