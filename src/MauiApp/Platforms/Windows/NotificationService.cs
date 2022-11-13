using Fluxinator.Shared;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Fluxinator.Platforms.Windows;

public class NotificationService : INotificationService
{
    public void SendNotification(string message)
    {
        var builder = new ToastContentBuilder();
        builder.AddText(message)
        .Show(toast =>
        {
            toast.ExpirationTime = DateTime.Now.AddMinutes(10);
        });
    }
}
