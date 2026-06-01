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

    public ProfilPage()
    {
        this.InitializeComponent();
        this.Loaded += async (_, _) =>
        {
            Result<Siren> siren = Siren.Create(SampleSiren);
            SirenField.FieldContent = siren.IsSuccess
                ? $"{siren.Value} ✓ (Luhn vérifié côté domaine)"
                : siren.Error?.Message;

            await RefreshStatusAsync();
        };
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
