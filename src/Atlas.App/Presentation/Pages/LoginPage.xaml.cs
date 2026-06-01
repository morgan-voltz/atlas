using System;
using System.Threading.Tasks;
using Atlas.App.Services;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Écran de connexion (doc 12 §10 ; M1/M7). Appelle <c>POST /auth/login</c> via <see cref="AtlasApiClient"/> ;
/// en cas de succès, bascule sur la coque applicative (<see cref="MainPage"/>). Auth : access token en
/// mémoire + refresh cookie HttpOnly géré par l'API (ADR-010). Le défi 2FA réel arrive dans une tranche suivante.
/// </summary>
public sealed partial class LoginPage : Page
{
    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();

    public LoginPage()
    {
        this.InitializeComponent();
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e) => await LoginAsync();

    private async Task LoginAsync()
    {
        string email = EmailBox.Text.Trim();
        string password = PasswordBox.Password;

        if (email.Length == 0 || password.Length == 0)
        {
            ShowError("Renseignez votre e-mail et votre mot de passe.");
            return;
        }

        SetBusy(true);
        try
        {
            Result<LoginResult> result = await _api.LoginAsync(email, password);

            if (result.IsFailure)
            {
                ShowError(result.Error?.Message ?? "La connexion a échoué.");
                return;
            }

            if (result.Value.Status == LoginStatus.TwoFactorRequired)
            {
                // Le défi TOTP (sous-vue « Validation en deux étapes ») sera câblé dans une tranche suivante.
                ShowError("Validation en deux étapes requise — écran à venir.");
                return;
            }

            // Connecté : bascule sur la coque applicative.
            this.Frame.Navigate(typeof(MainPage));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void SetBusy(bool busy)
    {
        if (busy)
        {
            ErrorText.Visibility = Visibility.Collapsed;
        }

        LoginButton.IsEnabled = !busy;
        Busy.IsActive = busy;
        Busy.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
    }
}
