using System.IO;
using System.Net.Http;
using System.Windows;
using Kaching.Core.Services;
using Kaching.Windows.Services;
using Kaching.Windows.ViewModels;

namespace Kaching.Windows;

public partial class App : Application
{
    private TrayIconService? trayIconService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kaching Windows");

        var store = new JsonAppStateStore(Path.Combine(appDataPath, "shopify-monitor.json"));
        var shopifyService = new ShopifyService(new HttpClient());
        var monitorService = new OrderMonitorService(shopifyService);
        var soundService = new KachingSoundService(appDataPath);
        var autoStartService = new AutoStartService();

        trayIconService = new TrayIconService();
        var notificationService = new NotificationService(trayIconService);

        var viewModel = new MainViewModel(
            store,
            monitorService,
            new SalesAnalytics(),
            soundService,
            notificationService,
            autoStartService);

        await viewModel.InitializeAsync();

        if (MainWindow is not null)
        {
            MainWindow.DataContext = viewModel;
            trayIconService.Attach(MainWindow);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        trayIconService?.Dispose();
        base.OnExit(e);
    }
}
