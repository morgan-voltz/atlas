using Atlas.Maui.Views;

namespace Atlas.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Route de détail accessible via GoToAsync("companydetail?siren=...").
        Routing.RegisterRoute("companydetail", typeof(CompanyDetailPage));
    }
}
