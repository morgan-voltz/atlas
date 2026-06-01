using Atlas.Domain.Companies;
using Atlas.Shared.Result;

namespace Atlas.App;

public sealed partial class MainPage : Page
{
    // SIREN réel de DANONE (cf. harness de test) — value object du domaine.
    private const string SampleSiren = "552032534";

    public MainPage()
    {
        this.InitializeComponent();
        ShowSirenValidation();
    }

    /// <summary>
    /// Preuve ADR-002 / ADR-029 : la tête Uno consomme directement un value object
    /// d'<c>Atlas.Domain</c> (validation Luhn incluse), sans aucune dépendance
    /// vers <c>Atlas.Infrastructure.*</c>. Toute donnée réelle passerait par
    /// <see cref="Services.AtlasApiClient"/> (HTTP uniquement).
    /// </summary>
    private void ShowSirenValidation()
    {
        Result<Siren> result = Siren.Create(SampleSiren);

        SirenResult.Text = result.IsSuccess
            ? $"✓ {result.Value} valide (clé de contrôle Luhn vérifiée côté domaine)."
            : $"✗ {result.Error?.Message}";
    }
}
