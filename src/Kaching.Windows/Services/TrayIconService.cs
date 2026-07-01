using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace Kaching.Windows.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon notifyIcon;
    private Window? mainWindow;

    public TrayIconService()
    {
        notifyIcon = new Forms.NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Kaching Windows",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    public void Attach(Window window)
    {
        mainWindow = window;
        mainWindow.StateChanged += (_, _) =>
        {
            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.Hide();
            }
        };
    }

    public void ShowBalloon(string title, string message)
    {
        notifyIcon.BalloonTipTitle = title;
        notifyIcon.BalloonTipText = message;
        notifyIcon.BalloonTipIcon = Forms.ToolTipIcon.Info;
        notifyIcon.ShowBalloonTip(5000);
    }

    public void Dispose() => notifyIcon.Dispose();

    private Forms.ContextMenuStrip BuildMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Abrir Kaching", null, (_, _) => ShowMainWindow());
        menu.Items.Add("Salir", null, (_, _) =>
        {
            notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        });
        return menu;
    }

    private void ShowMainWindow()
    {
        if (mainWindow is null)
        {
            return;
        }

        mainWindow.Show();
        mainWindow.WindowState = WindowState.Normal;
        mainWindow.Activate();
    }
}
