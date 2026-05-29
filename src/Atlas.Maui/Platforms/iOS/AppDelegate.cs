using System.Diagnostics.CodeAnalysis;
using Foundation;

namespace Atlas.Maui;

[Register("AppDelegate")]
[SuppressMessage(
    "Naming", "CA1711:Identifiers should not have incorrect suffix",
    Justification = "Apple UIKit convention requires the 'Delegate' suffix on UIApplicationDelegate-derived types; the [Register] attribute binds the Objective-C runtime name.")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
