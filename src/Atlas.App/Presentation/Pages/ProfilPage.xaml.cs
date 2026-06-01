using Atlas.Domain.Companies;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>Destination Profil — compte, données/RGPD (doc 12 §9). Scaffold U4.0.</summary>
public sealed partial class ProfilPage : Page
{
    // Preuve ADR-002 / ADR-029 conservée : la tête Uno consomme Atlas.Domain.Siren (Luhn).
    private const string SampleSiren = "552032534";

    public ProfilPage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) =>
        {
            Result<Siren> siren = Siren.Create(SampleSiren);
            SirenField.FieldContent = siren.IsSuccess
                ? $"{siren.Value} ✓ (Luhn vérifié côté domaine)"
                : siren.Error?.Message;
        };
    }
}
