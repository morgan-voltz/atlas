namespace Atlas.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
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