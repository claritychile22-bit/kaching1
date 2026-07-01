namespace Kaching.Windows.Services;

public sealed class NotificationService(TrayIconService trayIconService)
{
    public void NotifySale(string title, string message)
    {
        trayIconService.ShowBalloon(title, message);
    }
}
