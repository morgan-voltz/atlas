using System.Threading.Tasks;
using Atlas.App.Services;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Création de compte (doc 12 §10 ; M7). <c>POST /auth/register</c> → enchaîne sur l'écran d'attente de
/// vérification d'email (<see cref="VerifierEmailPage"/>). Règle de mot de passe visible (≥ 12 caractères,
/// alignée sur <c>RegisterUserValidator</c>). Aucun token n'est posé ici.
/// </summary>
public sealed partial class InscriptionPage : Page
{
    private const int MinPasswordLength = 12;
    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();

    public InscriptionPage()
    {
        this.InitializeComponent();
    }

    private async void OnSubmitClick(object sender, RoutedEventArgs e) => await SubmitAsync();

    private async Task SubmitAsync()
    {
        string email = EmailBox.Text.Trim();
        string password = PasswordBox.Password;

        if (email.Length == 0 || !email.Contains('@'))
        {
            ShowError("Saisissez une adresse e-mail valide.");
            return;
        }

        if (password.Length < MinPasswordLength)
        {
            ShowError($"Le mot de passe doit contenir au moins {MinPasswordLength} caractères.");
            return;
        }

        SetBusy(true);
        try
        {
            Result result = await _api.RegisterAsync(email, password);
            if (result.IsFailure)
            {
                ShowError(result.Error?.Message ?? "La création de compte a échoué.");
                return;
            }

            // Parcours annoncé : on enchaîne sur l'écran d'attente de vérification d'email.
            this.Frame.Navigate(typeof(VerifierEmailPage), new VerifyEmailArgs(email, null, null));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnGoToLogin(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(LoginPage));

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

        SubmitButton.IsEnabled = !busy;
        Busy.IsActive = busy;
        Busy.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
    }
}
