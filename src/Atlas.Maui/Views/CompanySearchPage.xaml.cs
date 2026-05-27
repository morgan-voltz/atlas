using Atlas.Maui.ViewModels;

namespace Atlas.Maui.Views;

public partial class CompanySearchPage : ContentPage
{
    public CompanySearchPage(CompanySearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
