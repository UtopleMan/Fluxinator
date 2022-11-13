using Microsoft.Fast.Components.FluentUI;
using Fluxinator.ApiClient;

namespace Fluxinator;
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<IDeploymentClient, KubernetesDeploymentClient>();
#if WINDOWS
        builder.Services.AddSingleton<Fluxinator.Shared.INotificationService, Fluxinator.Platforms.Windows.NotificationService>();
        builder.Services.AddSingleton<Fluxinator.Shared.ITrayService, Fluxinator.Platforms.Windows.TrayService>();
#endif
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Services.AddFluentUIComponents();
#endif
        return builder.Build();
	}
}
