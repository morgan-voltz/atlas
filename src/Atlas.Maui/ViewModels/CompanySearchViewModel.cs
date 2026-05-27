using System.Collections.ObjectModel;
using Atlas.Maui.Models;
using Atlas.Maui.Services;
using Atlas.Shared.Result;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Atlas.Maui.ViewModels;

public partial class CompanySearchViewModel(IAtlasApiClient api) : BaseViewModel
{
    [ObservableProperty]
    private string _query = string.Empty;

    public ObservableCollection<CompanySummaryResponse> Results { get; } = [];

    [RelayCommand]
    private async Task SearchAsync()
    {
        string term = Query.Trim();
        if (IsBusy || term.Length == 0)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        Results.Clear();
        try
        {
            // Saisie d'un SIREN (9 chiffres) → on ouvre directement la fiche.
            if (term.Length == 9 && term.All(char.IsAsciiDigit))
            {
                await Shell.Current.GoToAsync($"companydetail?siren={term}");
                return;
            }

            PagedResult<CompanySummaryResponse>? page = await api.SearchCompaniesAsync(term, 1, 20);
            if (page is not null)
            {
                foreach (CompanySummaryResponse item in page.Items)
                {
                    Results.Add(item);
                }
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

    [RelayCommand]
    private static async Task OpenAsync(CompanySummaryResponse? summary)
    {
        if (summary is not null)
        {
            await Shell.Current.GoToAsync($"companydetail?siren={summary.Siren}");
        }
    }
}
