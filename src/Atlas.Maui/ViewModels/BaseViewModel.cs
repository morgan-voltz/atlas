using CommunityToolkit.Mvvm.ComponentModel;

namespace Atlas.Maui.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;
}
