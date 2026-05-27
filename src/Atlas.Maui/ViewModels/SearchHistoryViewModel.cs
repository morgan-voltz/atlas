using System.Collections.ObjectModel;
using Atlas.Maui.Models;
using Atlas.Maui.Services;
using CommunityToolkit.Mvvm.Input;

namespace Atlas.Maui.ViewModels;

public partial class SearchHistoryViewModel : BaseViewModel
{
    private readonly IAtlasApiClient _api;

    public SearchHistoryViewModel(IAtlasApiClient api) => _api = api;

    public ObservableCollection<SearchHistoryEntryResponse> Entries { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        Entries.Clear();
        try
        {
            foreach (SearchHistoryEntryResponse entry in await _api.GetSearchHistoryAsync())
            {
                Entries.Add(entry);
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
