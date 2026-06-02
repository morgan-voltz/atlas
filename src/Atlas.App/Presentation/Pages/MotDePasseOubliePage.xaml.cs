using System.Threading.Tasks;
using Atlas.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Mot de passe oublié (doc 12 §10 ; M7). Demande d'un lien de réinitialisation
/// (<c>POST /auth/forgot-password</c>). Réponse volontairement uniforme (anti-énumération) : on
/// n'indique jamais si l'adresse existe. Le lien arrive par e-mail → <see cref="ReinitialiserMotDePassePage"/>.
/// </summary>
public sealed partial class MotDePasseOubliePage : Page
{
    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();

    public MotDePasseOubliePage()
    {
        this.InitializeComponent();
    }

    private async void OnSubmitClick(object sender, RoutedEventArgs e) => await SubmitAsync();

    private async Task SubmitAsync()
    {
        string email = EmailBox.Text.Trim();
        if (email.Length == 0 || !email.Contains('@'))
        {
            ErrorText.Text = "Saisissez une adresse e-mail valide.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        SetBusy(true);
        try
        {
            await _api.RequestPasswordResetAsync(email);
        }
        finally
        {
            SetBusy(false);
        }

        // Réponse uniforme quel que soit le résultat backend (anti-énumération).
        SubmittedDetail.Text =
            $"Si un compte correspond à {email}, un e-mail contenant un lien de réinitialisation vient d'être envoyé. Le lien expire sous une heure.";
        FormPanel.Visibility = Visibility.Collapsed;
        SubmittedPanel.Visibility = Visibility.Visible;
    }

    private void OnGoToLogin(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(LoginPage));

    private void SetBusy(bool busy)
    {
        if (busy)
        {
            ErrorText.Visibility = Visibility.Collapsed;
        }

        SubmitButton.IsEnabled = !busy;
        Busy.IsActive = busy;
        Busy.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
    }
}
