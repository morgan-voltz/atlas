using Atlas.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Atlas.Maui.ViewModels;

public partial class LoginViewModel(IAtlasApiClient api) : BaseViewModel
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

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
            if (await api.LoginAsync(Email, Password))
            {
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                ErrorMessage = "Email ou mot de passe incorrect.";
            }
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Service indisponible. Réessayez plus tard.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
