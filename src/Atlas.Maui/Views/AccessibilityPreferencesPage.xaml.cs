using Atlas.Maui.ViewModels;

namespace Atlas.Maui.Views;

public partial class AccessibilityPreferencesPage : ContentPage
{
    private readonly AccessibilityPreferencesViewModel _viewModel;

    public AccessibilityPreferencesPage(AccessibilityPreferencesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
