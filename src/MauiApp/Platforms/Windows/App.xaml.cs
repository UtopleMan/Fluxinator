using Microsoft.UI;
using Windows.Graphics;
using Microsoft.UI.Windowing;
using Fluxinator.Platforms.Windows;
using Microsoft.UI.Xaml;

namespace Fluxinator.WinUI;
public partial class App : MauiWinUIApplication
{
    const int WindowWidth = 1000;
    const int WindowHeight = 500;
    public App()
	{
		this.InitializeComponent();
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
            var mauiWindow = handler.VirtualView;
            var nativeWindow = handler.PlatformView;
            nativeWindow.Activate();
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));
        });
    }
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        FluxinatorWindowExtensions.Hwnd = ((MauiWinUIWindow)App.Current.Application.Windows.First().Handler.PlatformView).WindowHandle;
    }
}

