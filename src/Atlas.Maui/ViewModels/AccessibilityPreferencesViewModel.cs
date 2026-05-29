using Atlas.Maui.Models;
using Atlas.Maui.Services;
using Atlas.Maui.Theming;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Atlas.Maui.ViewModels;

/// <summary>
/// Écran de réglages d'accessibilité (cf. <c>docs/06-accessibilite.md</c> §6.3). Charge les
/// préférences depuis l'API au démarrage, applique localement via <see cref="ThemeManager"/>,
/// puis pousse les modifications via <c>PUT /user/preferences/accessibility</c>.
/// </summary>
public partial class AccessibilityPreferencesViewModel : BaseViewModel
{
    private readonly IAtlasApiClient _api;
    private readonly ThemeManager _themeManager;

    public AccessibilityPreferencesViewModel(IAtlasApiClient api, ThemeManager themeManager)
    {
        _api = api;
        _themeManager = themeManager;

        HighContrast = themeManager.HighContrast;
        ReduceMotion = themeManager.ReduceMotion;
        SelectedFontPreference = themeManager.FontPreference;
    }

    [ObservableProperty]
    public partial bool HighContrast { get; set; }

    [ObservableProperty]
    public partial bool ReduceMotion { get; set; }

    /// <summary>« Default » | « DyslexiaFriendly » | « HighReadability ».</summary>
    [ObservableProperty]
    public partial string SelectedFontPreference { get; set; }

    public IReadOnlyList<string> FontPreferences { get; } =
        ["Default", "DyslexiaFriendly", "HighReadability"];

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            AccessibilityPreferencesResponse? prefs = await _api.GetAccessibilityPreferencesAsync();
            if (prefs is not null)
            {
                HighContrast = prefs.HighContrast;
                ReduceMotion = prefs.ReduceMotion;
                SelectedFontPreference = prefs.FontPreference;

                // Applique localement (source de vérité = backend, override Preferences est neutralisé).
                _themeManager.SetAccessibility(HighContrast, ReduceMotion, SelectedFontPreference);
            }
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Impossible de charger les préférences. Réessayez plus tard.";
            SemanticScreenReader.Default.Announce(ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            // Application locale immédiate (l'utilisateur voit le résultat avant le round-trip réseau).
            _themeManager.SetAccessibility(HighContrast, ReduceMotion, SelectedFontPreference);

            var payload = new AccessibilityPreferencesResponse(
                HighContrast, ReduceMotion, SelectedFontPreference);
            bool ok = await _api.UpdateAccessibilityPreferencesAsync(payload);
            if (!ok)
            {
                ErrorMessage = "Préférences appliquées localement mais non synchronisées avec le serveur.";
                SemanticScreenReader.Default.Announce(ErrorMessage);
            }
            else
            {
                SemanticScreenReader.Default.Announce("Préférences d'accessibilité enregistrées.");
            }
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Préférences appliquées localement mais service indisponible.";
            SemanticScreenReader.Default.Announce(ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
