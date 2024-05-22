using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace WebTranslator.Services;

public static class ToastService
{
    private static WindowNotificationManager? NotifyManager { get; set; }

    public static void Set(TopLevel? level)
    {
        NotifyManager ??= new WindowNotificationManager(level)
            { MaxItems = 3, Position = NotificationPosition.BottomRight };
    }

    public static void Notify(string title, string content, NotificationType type = NotificationType.Information)
    {
        NotifyManager?.Show(new Notification(title, content, type));
    }
}