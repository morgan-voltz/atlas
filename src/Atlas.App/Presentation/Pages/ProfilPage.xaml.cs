using System;
using System.Globalization;
using System.Threading.Tasks;
using Atlas.App.Models;
using Atlas.App.Services;
using Atlas.Domain.Companies;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Destination Profil (doc 12 §9). U4.1 : connexion INPI fonctionnelle (F-003) — formulaire →
/// <c>POST /inpi/connection</c>, statut <c>GET</c>, déconnexion <c>DELETE</c>. Les identifiants INPI
/// ne sont ni stockés ni journalisés côté client (CLAUDE.md). Conserve la preuve <c>Domain.Siren</c>.
/// </summary>
public sealed partial class ProfilPage : Page
{
    private const string SampleSiren = "552032534";
    private static readonly CultureInfo Culture = new("fr-FR");

    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();
    private string? _userEmail;

    public ProfilPage()
    {
        this.InitializeComponent();
        this.Loaded += async (_, _) =>
        {
            Result<Siren> siren = Siren.Create(SampleSiren);
            SirenField.FieldContent = siren.IsSuccess
                ? $"{siren.Value} ✓ (Luhn vérifié côté domaine)"
                : siren.Error?.Message;

            _userEmail = _api.GetCurrentUserEmail();
            ConfirmPrompt.Text = _userEmail is { Length: > 0 }
                ? $"Pour confirmer, ressaisissez votre e-mail ({_userEmail})."
                : "Pour confirmer, ressaisissez votre e-mail.";

            await RefreshStatusAsync();
        };
    }

    private async void OnLogout(object sender, RoutedEventArgs e)
    {
        LogoutButton.IsEnabled = false;
        await _api.LogoutAsync();
        App.RootFrame?.Navigate(typeof(LoginPage));
    }

    private async void OnExport(object sender, RoutedEventArgs e)
    {
        ExportButton.IsEnabled = false;
        ExportMessage.Visibility = Visibility.Collapsed;
        ExportBusy.IsActive = true;
        ExportBusy.Visibility = Visibility.Visible;

        Result<string> result = await _api.ExportMyDataAsync();

        ExportBusy.IsActive = false;
        ExportBusy.Visibility = Visibility.Collapsed;
        ExportButton.IsEnabled = true;

        if (result.IsFailure)
        {
            ShowExportMessage(result.Error?.Message ?? "L'export a échoué. Réessayez.");
            return;
        }

        bool saved = await DataExportSaver.SaveAsync(result.Value);
        if (saved)
        {
            ShowExportMessage("Export prêt : l'archive a été enregistrée.");
        }
    }

    private void OnRevealDelete(object sender, RoutedEventArgs e)
    {
        DeleteRevealButton.Visibility = Visibility.Collapsed;
        DeleteConfirmPanel.Visibility = Visibility.Visible;
    }

    private void OnCancelDelete(object sender, RoutedEventArgs e)
    {
        ConfirmEmailBox.Text = string.Empty;
        DeleteError.Visibility = Visibility.Collapsed;
        DeleteConfirmPanel.Visibility = Visibility.Collapsed;
        DeleteRevealButton.Visibility = Visibility.Visible;
    }

    private void OnConfirmEmailChanged(object sender, TextChangedEventArgs e) => DeleteButton.IsEnabled = CanDelete();

    private bool CanDelete() =>
        _userEmail is { Length: > 0 }
        && string.Equals(ConfirmEmailBox.Text.Trim(), _userEmail, StringComparison.OrdinalIgnoreCase);

    private async void OnDelete(object sender, RoutedEventArgs e)
    {
        if (!CanDelete())
        {
            return;
        }

        DeleteButton.IsEnabled = false;
        DeleteError.Visibility = Visibility.Collapsed;

        Result result = await _api.DeleteMyAccountAsync();
        if (result.IsSuccess)
        {
            App.RootFrame?.Navigate(typeof(LoginPage));
            return;
        }

        DeleteError.Text = result.Error?.Message ?? "La suppression a échoué. Réessayez dans un moment.";
        DeleteError.Visibility = Visibility.Visible;
        DeleteButton.IsEnabled = true;
    }

    private void ShowExportMessage(string message)
    {
        ExportMessage.Text = message;
        ExportMessage.Visibility = Visibility.Visible;
    }

    private async Task RefreshStatusAsync()
    {
        SetBusy(true);
        ErrorText.Visibility = Visibility.Collapsed;

        Result<InpiConnectionStatusResponse> result = await _api.GetInpiStatusAsync();
        SetBusy(false);

        if (result.IsSuccess && result.Value.Connected)
        {
            ShowConnected(result.Value);
        }
        else
        {
            ShowForm();
            if (result.IsFailure)
            {
                ShowError(result.Error?.Message ?? "Statut INPI indisponible.");
            }
        }
    }

    private async void OnConnect(object sender, RoutedEventArgs e)
    {
        string user = InpiUser.Text.Trim();
        string pass = InpiPass.Password;
        if (user.Length == 0 || pass.Length == 0)
        {
            ShowError("Renseignez votre identifiant et votre mot de passe INPI.");
            return;
        }

        ConnectButton.IsEnabled = false;
        ErrorText.Visibility = Visibility.Collapsed;

        Result result = await _api.ConnectInpiAsync(user, pass);
        ConnectButton.IsEnabled = true;

        if (result.IsFailure)
        {
            ShowError(result.Error?.Message ?? "La connexion INPI a échoué.");
            return;
        }

        // Ne pas conserver les identifiants en mémoire après l'appel.
        InpiPass.Password = string.Empty;
        await RefreshStatusAsync();
    }

    private async void OnDisconnect(object sender, RoutedEventArgs e)
    {
        DisconnectButton.IsEnabled = false;
        Result result = await _api.DisconnectInpiAsync();
        DisconnectButton.IsEnabled = true;

        if (result.IsSuccess)
        {
            await RefreshStatusAsync();
        }
    }

    private void ShowConnected(InpiConnectionStatusResponse status)
    {
        FormPanel.Visibility = Visibility.Collapsed;
        ConnectedPanel.Visibility = Visibility.Visible;
        ConnectedDetail.Text = status.LastTestedAt is { } d
            ? $"Dernier test : {d.ToString("d MMM yyyy", Culture)}"
            : string.Empty;
    }

    private void ShowForm()
    {
        ConnectedPanel.Visibility = Visibility.Collapsed;
        FormPanel.Visibility = Visibility.Visible;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void SetBusy(bool busy)
    {
        StatusBusy.IsActive = busy;
        StatusBusy.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
    }
}
