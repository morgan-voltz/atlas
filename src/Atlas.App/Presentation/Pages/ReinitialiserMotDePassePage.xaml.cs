using System.Threading.Tasks;
using Atlas.App.Services;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Réinitialisation effective du mot de passe (doc 12 §10 ; M7). Atteinte via le lien d'e-mail
/// (<see cref="ResetPasswordArgs"/> : <c>userId</c> + <c>token</c>). Collecte le nouveau mot de passe
/// (règle ≥ 12 visible) puis appelle <c>POST /auth/reset-password</c>. Succès → invitation à se
/// connecter (les anciennes sessions sont révoquées côté serveur).
/// </summary>
public sealed partial class ReinitialiserMotDePassePage : Page
{
    private const int MinPasswordLength = 12;
    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();
    private string? _userId;
    private string? _token;

    public ReinitialiserMotDePassePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var args = e.Parameter as ResetPasswordArgs ?? new ResetPasswordArgs(null, null);
        _userId = args.UserId;
        _token = args.Token;

        bool hasLink = !string.IsNullOrWhiteSpace(_userId) && !string.IsNullOrWhiteSpace(_token);
        FormPanel.Visibility = hasLink ? Visibility.Visible : Visibility.Collapsed;
        InvalidPanel.Visibility = hasLink ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void OnSubmitClick(object sender, RoutedEventArgs e) => await SubmitAsync();

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(_userId) || string.IsNullOrWhiteSpace(_token))
        {
            return;
        }

        string password = PasswordBox.Password;
        if (password.Length < MinPasswordLength)
        {
            ErrorText.Text = $"Le mot de passe doit contenir au moins {MinPasswordLength} caractères.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        SetBusy(true);
        try
        {
            Result result = await _api.ResetPasswordAsync(_userId!, _token!, password);
            if (result.IsFailure)
            {
                ErrorText.Text = result.Error?.Message ?? "La réinitialisation a échoué. Réessayez.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            FormPanel.Visibility = Visibility.Collapsed;
            DonePanel.Visibility = Visibility.Visible;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnGoToLogin(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(LoginPage));

    private void OnGoToForgot(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(MotDePasseOubliePage));

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
