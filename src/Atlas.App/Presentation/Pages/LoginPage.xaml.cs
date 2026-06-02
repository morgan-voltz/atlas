using System.Threading.Tasks;
using Atlas.App.Services;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Écran de connexion (doc 12 §10 ; M1/M7). <c>POST /auth/login</c> → soit connecté, soit défi 2FA
/// (sous-vue TOTP, le challenge token restant en mémoire hors URL) → <c>POST /auth/2fa/verify</c>.
/// Auth : access token en mémoire + refresh cookie HttpOnly (ADR-010).
/// </summary>
public sealed partial class LoginPage : Page
{
    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();
    private string? _challengeToken;

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
            ShowError(ErrorText, "Renseignez votre e-mail et votre mot de passe.");
            return;
        }

        SetBusy(true);
        try
        {
            Result<LoginResult> result = await _api.LoginAsync(email, password);
            if (result.IsFailure)
            {
                ShowError(ErrorText, result.Error?.Message ?? "La connexion a échoué.");
                return;
            }

            if (result.Value.Status == LoginStatus.TwoFactorRequired)
            {
                _challengeToken = result.Value.ChallengeToken;
                ShowTwoFactor();
                return;
            }

            this.Frame.Navigate(typeof(MainPage));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnVerifyClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_challengeToken))
        {
            return;
        }

        string code = CodeBox.Text.Trim();
        if (code.Length == 0)
        {
            ShowError(TwoFactorError, "Saisissez le code de validation.");
            return;
        }

        SetBusy(true);
        try
        {
            Result result = await _api.VerifyTwoFactorAsync(_challengeToken, code);
            if (result.IsFailure)
            {
                ShowError(TwoFactorError, result.Error?.Message ?? "Code incorrect.");
                return;
            }

            this.Frame.Navigate(typeof(MainPage));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnGoToRegister(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(InscriptionPage));

    private void OnForgotPassword(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(MotDePasseOubliePage));

    private void OnBackToCredentials(object sender, RoutedEventArgs e)
    {
        _challengeToken = null;
        CodeBox.Text = string.Empty;
        TwoFactorError.Visibility = Visibility.Collapsed;
        TwoFactorPanel.Visibility = Visibility.Collapsed;
        CredentialsPanel.Visibility = Visibility.Visible;
    }

    private void ShowTwoFactor()
    {
        ErrorText.Visibility = Visibility.Collapsed;
        TwoFactorError.Visibility = Visibility.Collapsed;
        CredentialsPanel.Visibility = Visibility.Collapsed;
        TwoFactorPanel.Visibility = Visibility.Visible;
    }

    private static void ShowError(TextBlock target, string message)
    {
        target.Text = message;
        target.Visibility = Visibility.Visible;
    }

    private void SetBusy(bool busy)
    {
        if (busy)
        {
            ErrorText.Visibility = Visibility.Collapsed;
            TwoFactorError.Visibility = Visibility.Collapsed;
        }

        LoginButton.IsEnabled = !busy;
        VerifyButton.IsEnabled = !busy;
        Busy.IsActive = busy;
        Busy.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
    }
}
