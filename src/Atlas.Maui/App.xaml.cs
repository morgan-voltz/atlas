using Atlas.Maui.Theming;

namespace Atlas.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Kit de thèmes Atlas (7 thèmes × 2 modes = 14 palettes, WCAG 2.2 AA, ADR-008).
		// Restaure la préférence persistée et applique le ResourceDictionary correspondant.
		new ThemeManager().Initialize();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());

#if WINDOWS || MACCATALYST
		// Adaptation desktop (F-010) : fenêtre dimensionnée pour grand écran.
		window.Width = 1100;
		window.Height = 800;
		window.MinimumWidth = 800;
		window.MinimumHeight = 600;
#endif

		return window;
	}
}