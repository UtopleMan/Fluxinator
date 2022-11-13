using Fluxinator.Shared;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Fluxinator;

public partial class MainPage : ContentPage
{
    private static bool isSetup = false;
	public MainPage()
	{
		InitializeComponent();
        if (!isSetup)
        {
            isSetup = true;

            SetupAppActions();
            SetupTrayIcon();
        }
    }
    private void SetupAppActions()
    {
        try
        {
#if WINDOWS
            //AppActions.IconDirectory = Application.Current.On<WindowsConfiguration>().GetImageDirectory();
#endif
            AppActions.SetAsync(
                new AppAction("current_info", "Check Current Weather", icon: "current_info"),
                new AppAction("add_location", "Add a Location", icon: "add_location")
            );
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine("App Actions not supported", ex);
        }
    }

    private void SetupTrayIcon()
    {
        var trayService = Shared.ServiceProvider.GetService<ITrayService>();

        if (trayService != null)
        {
            trayService.Initialize();
            trayService.ClickHandler = () => Debug.WriteLine("Hello Build! 😻 From .NET MAUI", "How's your weather?  It's sunny where we are 🌞");
        }
    }
}
