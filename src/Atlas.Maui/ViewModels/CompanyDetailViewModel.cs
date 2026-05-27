using Atlas.Maui.Models;
using Atlas.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Atlas.Maui.ViewModels;

[QueryProperty(nameof(Siren), "siren")]
public partial class CompanyDetailViewModel : BaseViewModel
{
    private readonly IAtlasApiClient _api;

    public CompanyDetailViewModel(IAtlasApiClient api) => _api = api;

    [ObservableProperty]
    public partial string? Siren { get; set; }

    [ObservableProperty]
    public partial CompanyResponse? Company { get; set; }

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
            Company = await _api.GetCompanyBySirenAsync(siren);
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
