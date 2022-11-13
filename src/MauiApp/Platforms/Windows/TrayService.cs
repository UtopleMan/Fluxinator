using Fluxinator.Shared;
using Hardcodet.Wpf.TaskbarNotification.Interop;

namespace Fluxinator.Platforms.Windows;
public class TrayService : ITrayService
{
    WindowsTrayIcon tray;

    public Action ClickHandler { get; set; }
    private static bool showing = true;
    public void Initialize()
    {
        tray = new WindowsTrayIcon("Platforms/Windows/trayicon.ico");
        tray.RightClick = () =>
        {
            // Show menu
        };
        tray.LeftClick = () =>
        {
            if (showing)
            {
                FluxinatorWindowExtensions.MinimizeToTray();
                showing = false;
                return;
            }
            FluxinatorWindowExtensions.BringToFront();
            showing = true;
            ClickHandler?.Invoke();

        };
    }
}
