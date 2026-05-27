using System.Collections.ObjectModel;
using Atlas.Maui.Models;
using Atlas.Maui.Services;
using CommunityToolkit.Mvvm.Input;

namespace Atlas.Maui.ViewModels;

public partial class SearchHistoryViewModel(IAtlasApiClient api) : BaseViewModel
{
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
            foreach (SearchHistoryEntryResponse entry in await api.GetSearchHistoryAsync())
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
