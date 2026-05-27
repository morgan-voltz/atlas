using Atlas.Maui.ViewModels;

namespace Atlas.Maui.Views;

public partial class SearchHistoryPage : ContentPage
{
    private readonly SearchHistoryViewModel _viewModel;

    public SearchHistoryPage(SearchHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.LoadCommand.CanExecute(null))
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }
}
