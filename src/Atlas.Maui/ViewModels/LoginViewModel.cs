using Atlas.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Atlas.Maui.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAtlasApiClient _api;

    public LoginViewModel(IAtlasApiClient api)
    {
        _api = api;
        Email = string.Empty;
        Password = string.Empty;
    }

    [ObservableProperty]
    public partial string Email { get; set; }

    [ObservableProperty]
    public partial string Password { get; set; }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (await _api.LoginAsync(Email, Password))
            {
                SemanticScreenReader.Default.Announce("Connexion réussie, ouverture de l'application.");
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                ErrorMessage = "Email ou mot de passe incorrect.";
                SemanticScreenReader.Default.Announce(ErrorMessage);
            }
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Service indisponible. Réessayez plus tard.";
            SemanticScreenReader.Default.Announce(ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
