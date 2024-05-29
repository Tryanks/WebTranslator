using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace WebTranslator.Services;

public static class ToastService
{
    private static WindowNotificationManager? NotifyManager { get; set; }

    public static void Set(TopLevel? level)
    {
        NotifyManager ??= new WindowNotificationManager(level)
            { MaxItems = 3, Position = NotificationPosition.BottomLeft };
    }

    public static void Notify(string title, string content, NotificationType type = NotificationType.Information)
    {
        NotifyManager?.Show(new Notification(title, content, type));
    }

    public static void Notify(string content, NotificationType type = NotificationType.Information)
    {
        NotifyManager?.Show(new Notification(type switch
        {
            NotificationType.Information => "提示",
            NotificationType.Success => "成功",
            NotificationType.Warning => "注意",
            NotificationType.Error => "错误",
            _ => "提示"
        }, content, type));
    }
}