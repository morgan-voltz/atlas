using System.Collections.ObjectModel;
using Atlas.Maui.Models;
using Atlas.Maui.Services;
using Atlas.Shared.Result;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Atlas.Maui.ViewModels;

public partial class CompanySearchViewModel : BaseViewModel
{
    private readonly IAtlasApiClient _api;

    public CompanySearchViewModel(IAtlasApiClient api)
    {
        _api = api;
        Query = string.Empty;
    }

    [ObservableProperty]
    public partial string Query { get; set; }

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

            PagedResult<CompanySummaryResponse>? page = await _api.SearchCompaniesAsync(term, 1, 20);
            if (page is not null)
            {
                foreach (CompanySummaryResponse item in page.Items)
                {
                    Results.Add(item);
                }
            }

            // Annonce pour les lecteurs d'écran (cf. docs/06 §4.2 — l'utilisateur doit savoir
            // que la liste a changé sans avoir à scruter visuellement).
            string announcement = Results.Count switch
            {
                0 => "Aucun résultat trouvé.",
                1 => "1 résultat trouvé.",
                _ => $"{Results.Count} résultats trouvés.",
            };
            SemanticScreenReader.Default.Announce(announcement);
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

    [RelayCommand]
    private static async Task OpenAsync(CompanySummaryResponse? summary)
    {
        if (summary is not null)
        {
            await Shell.Current.GoToAsync($"companydetail?siren={summary.Siren}");
        }
    }
}
