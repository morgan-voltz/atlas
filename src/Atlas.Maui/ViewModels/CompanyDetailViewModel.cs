using Atlas.Maui.Models;
using Atlas.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Atlas.Maui.ViewModels;

[QueryProperty(nameof(Siren), "siren")]
public partial class CompanyDetailViewModel(IAtlasApiClient api) : BaseViewModel
{
    [ObservableProperty]
    private string? _siren;

    [ObservableProperty]
    private CompanyResponse? _company;

    partial void OnSirenChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadAsync(value);
        }
    }

    private async Task LoadAsync(string siren)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            Company = await api.GetCompanyBySirenAsync(siren);
            if (Company is null)
            {
                ErrorMessage = "Entreprise introuvable.";
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
