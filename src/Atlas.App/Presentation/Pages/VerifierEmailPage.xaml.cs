using System.Threading.Tasks;
using Atlas.App.Services;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Vérification d'email (doc 12 §10 ; M7). Deux usages selon <see cref="VerifyEmailArgs"/> :
/// sans token, état d'ATTENTE après inscription (renvoi possible, jamais un cul-de-sac) ; avec
/// <c>userId</c> + <c>token</c> (lien d'activation), on vérifie puis on annonce le palier INPI à venir.
/// « Renvoyer le lien » appelle <c>/auth/resend-verification</c> (réponse uniforme, anti-énumération).
/// </summary>
public sealed partial class VerifierEmailPage : Page
{
    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();
    private string? _email;

    public VerifierEmailPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var args = e.Parameter as VerifyEmailArgs ?? new VerifyEmailArgs(null, null, null);
        _email = args.Email;

        // Lien d'activation (userId + token) → vérification immédiate.
        if (!string.IsNullOrWhiteSpace(args.UserId) && !string.IsNullOrWhiteSpace(args.Token))
        {
            ShowOnly(VerifyingPanel);
            Result result = await _api.VerifyEmailAsync(args.UserId!, args.Token!);
            if (result.IsSuccess)
            {
                ShowOnly(VerifiedPanel);
            }
            else
            {
                ErrorText.Text = result.Error?.Message ?? "Ce lien de vérification est invalide ou a expiré.";
                ShowOnly(ErrorPanel);
            }

            return;
        }

        // État d'attente après inscription.
        bool hasEmail = !string.IsNullOrWhiteSpace(_email);
        PendingDetail.Text = hasEmail
            ? $"Un e-mail de vérification a été envoyé à {_email}. Cliquez le lien qu'il contient pour activer votre compte."
            : "Un e-mail de vérification vous a été envoyé. Cliquez le lien qu'il contient pour activer votre compte.";
        ResendButton.Visibility = hasEmail ? Visibility.Visible : Visibility.Collapsed;
        ShowOnly(PendingPanel);
    }

    private async void OnResendClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_email))
        {
            return;
        }

        ResendButton.IsEnabled = false;
        await _api.ResendVerificationAsync(_email!);

        // Réponse volontairement uniforme (le backend ne révèle pas l'existence du compte).
        ResendMessage.Text = "Si un compte en attente correspond, un nouvel e-mail de vérification a été envoyé.";
        ResendMessage.Visibility = Visibility.Visible;
        ResendButton.IsEnabled = true;
    }

    private void OnGoToLogin(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(LoginPage));

    private void OnGoToInscription(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(InscriptionPage));

    private void ShowOnly(StackPanel panel)
    {
        VerifyingPanel.Visibility = Visibility.Collapsed;
        VerifiedPanel.Visibility = Visibility.Collapsed;
        ErrorPanel.Visibility = Visibility.Collapsed;
        PendingPanel.Visibility = Visibility.Collapsed;
        panel.Visibility = Visibility.Visible;
    }
}
