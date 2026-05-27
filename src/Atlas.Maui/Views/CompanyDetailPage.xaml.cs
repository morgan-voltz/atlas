using Atlas.Maui.ViewModels;

namespace Atlas.Maui.Views;

public partial class CompanyDetailPage : ContentPage
{
    public CompanyDetailPage(CompanyDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
