using System.Collections.ObjectModel;
using Kaching.Core.Models;
using Kaching.Core.Services;
using Kaching.Windows.Services;

namespace Kaching.Windows.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IAppStateStore stateStore;
    private readonly OrderMonitorService monitorService;
    private readonly SalesAnalytics salesAnalytics;
    private readonly KachingSoundService soundService;
    private readonly NotificationService notificationService;
    private readonly AutoStartService autoStartService;
    private ShopifyMonitorState state = new();
    private CancellationTokenSource? monitorCancellation;
    private SalesDashboardSnapshot? snapshot;
    private string shopDomain = string.Empty;
    private string accessToken = string.Empty;
    private int pollIntervalSeconds = 30;
    private bool autoStartWithWindows;
    private bool isMonitoring;
    private string statusMessage = "Configura tu tienda Shopify para comenzar";

    public MainViewModel(
        IAppStateStore stateStore,
        OrderMonitorService monitorService,
        SalesAnalytics salesAnalytics,
        KachingSoundService soundService,
        NotificationService notificationService,
        AutoStartService autoStartService)
    {
        this.stateStore = stateStore;
        this.monitorService = monitorService;
        this.salesAnalytics = salesAnalytics;
        this.soundService = soundService;
        this.notificationService = notificationService;
        this.autoStartService = autoStartService;

        SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
        StartMonitoringCommand = new RelayCommand(StartMonitoring, () => !IsMonitoring);
        StopMonitoringCommand = new RelayCommand(StopMonitoring, () => IsMonitoring);
        CheckNowCommand = new RelayCommand(async () => await CheckNowAsync());
    }

    public ObservableCollection<ShopifyOrder> SalesHistory { get; } = [];

    public RelayCommand SaveSettingsCommand { get; }
    public RelayCommand StartMonitoringCommand { get; }
    public RelayCommand StopMonitoringCommand { get; }
    public RelayCommand CheckNowCommand { get; }

    public SalesDashboardSnapshot? Snapshot
    {
        get => snapshot;
        private set => SetProperty(ref snapshot, value);
    }

    public string ShopDomain
    {
        get => shopDomain;
        set => SetProperty(ref shopDomain, value);
    }

    public string AccessToken
    {
        get => accessToken;
        set => SetProperty(ref accessToken, value);
    }

    public int PollIntervalSeconds
    {
        get => pollIntervalSeconds;
        set => SetProperty(ref pollIntervalSeconds, Math.Max(ShopifySettings.MinimumPollIntervalSeconds, value));
    }

    public bool AutoStartWithWindows
    {
        get => autoStartWithWindows;
        set => SetProperty(ref autoStartWithWindows, value);
    }

    public bool IsMonitoring
    {
        get => isMonitoring;
        private set
        {
            if (SetProperty(ref isMonitoring, value))
            {
                StartMonitoringCommand.RaiseCanExecuteChanged();
                StopMonitoringCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public async Task InitializeAsync()
    {
        state = await stateStore.LoadAsync();
        state.Settings = state.Settings.Normalize();

        ShopDomain = state.Settings.ShopDomain;
        AccessToken = state.Settings.AccessToken;
        PollIntervalSeconds = state.Settings.PollIntervalSeconds;
        AutoStartWithWindows = state.Settings.AutoStartWithWindows || autoStartService.IsEnabled();
        ApplySnapshot();
    }

    private async Task SaveSettingsAsync()
    {
        state.Settings = new ShopifySettings(ShopDomain, AccessToken, PollIntervalSeconds, AutoStartWithWindows).Normalize();
        ShopDomain = state.Settings.ShopDomain;
        PollIntervalSeconds = state.Settings.PollIntervalSeconds;
        autoStartService.SetEnabled(state.Settings.AutoStartWithWindows);
        await stateStore.SaveAsync(state);
        StatusMessage = "Configuracion guardada";
    }

    private void StartMonitoring()
    {
        if (IsMonitoring)
        {
            return;
        }

        if (!state.Settings.HasCredentials)
        {
            StatusMessage = "Guarda la tienda y el token antes de iniciar el monitor";
            return;
        }

        monitorCancellation = new CancellationTokenSource();
        IsMonitoring = true;
        StatusMessage = "Monitor Shopify activo";
        _ = MonitorLoopAsync(monitorCancellation.Token);
    }

    private void StopMonitoring()
    {
        monitorCancellation?.Cancel();
        monitorCancellation?.Dispose();
        monitorCancellation = null;
        IsMonitoring = false;
        StatusMessage = "Monitor detenido";
    }

    private async Task CheckNowAsync()
    {
        await SaveSettingsAsync();
        await CheckForSalesAsync(CancellationToken.None);
    }

    private async Task MonitorLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await CheckForSalesAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(state.Settings.PollIntervalSeconds), cancellationToken);
        }
    }

    private async Task CheckForSalesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var newOrders = await monitorService.CheckForNewOrdersAsync(state, cancellationToken);
            await stateStore.SaveAsync(state, cancellationToken);
            ApplySnapshot();

            if (newOrders.Count > 0)
            {
                soundService.Play();
                var total = newOrders.Sum(order => order.TotalAmount);
                notificationService.NotifySale("Kaching! Nueva venta en Shopify", $"{newOrders.Count} pedido(s) por {total:N0} {newOrders.Last().CurrencyCode}");
                StatusMessage = $"{newOrders.Count} venta(s) nuevas detectadas";
            }
            else
            {
                StatusMessage = $"Sin ventas nuevas. Ultima revision {DateTime.Now:t}";
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            state.LastError = ex.Message;
            StatusMessage = $"Error Shopify: {ex.Message}";
            await stateStore.SaveAsync(state, CancellationToken.None);
        }
    }

    private void ApplySnapshot()
    {
        Snapshot = salesAnalytics.BuildSnapshot(state.SalesHistory, DateOnly.FromDateTime(DateTime.Today));
        SalesHistory.Clear();
        foreach (var order in Snapshot.RecentSales)
        {
            SalesHistory.Add(order);
        }
    }
}
