using System.Collections.ObjectModel;
using Kaching.Core.Models;
using Kaching.Core.Services;

namespace Kaching.Windows.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IWorkspaceStore workspaceStore;
    private readonly FinanceCalculator financeCalculator;
    private FinanceWorkspace workspace = new();
    private DashboardSnapshot? snapshot;
    private string statusMessage = "Preparando Kaching Windows";

    public MainViewModel(IWorkspaceStore workspaceStore, FinanceCalculator financeCalculator)
    {
        this.workspaceStore = workspaceStore;
        this.financeCalculator = financeCalculator;

        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
        ResetDemoCommand = new RelayCommand(async () => await ResetDemoAsync());
        AddSaleCommand = new RelayCommand(async () => await AddSaleAsync(), () => Accounts.Count > 0);
    }

    public ObservableCollection<Account> Accounts { get; } = [];
    public ObservableCollection<FinancialTransaction> RecentTransactions { get; } = [];
    public ObservableCollection<CategorySpend> CategorySpending { get; } = [];

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ResetDemoCommand { get; }
    public RelayCommand AddSaleCommand { get; }

    public DashboardSnapshot? Snapshot
    {
        get => snapshot;
        private set => SetProperty(ref snapshot, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public async Task InitializeAsync()
    {
        workspace = await workspaceStore.LoadAsync();
        if (workspace.IsEmpty)
        {
            workspace = SampleWorkspaceFactory.Create(DateOnly.FromDateTime(DateTime.Today));
            await workspaceStore.SaveAsync(workspace);
        }

        ApplySnapshot("Listo para decidir con mejor informacion");
    }

    private async Task RefreshAsync()
    {
        workspace = await workspaceStore.LoadAsync();
        ApplySnapshot("Datos actualizados");
    }

    private async Task ResetDemoAsync()
    {
        workspace = SampleWorkspaceFactory.Create(DateOnly.FromDateTime(DateTime.Today));
        await workspaceStore.SaveAsync(workspace);
        ApplySnapshot("Espacio de trabajo reiniciado");
    }

    private async Task AddSaleAsync()
    {
        var account = workspace.Accounts.First();
        workspace.Transactions.Insert(0, FinancialTransaction.Create(
            account.Id,
            DateOnly.FromDateTime(DateTime.Today),
            "Venta rapida",
            "Ingresos",
            250000,
            TransactionType.Income));

        await workspaceStore.SaveAsync(workspace);
        ApplySnapshot("Venta rapida agregada");
    }

    private void ApplySnapshot(string message)
    {
        Snapshot = financeCalculator.BuildSnapshot(workspace, DateOnly.FromDateTime(DateTime.Today));

        Accounts.Clear();
        foreach (var account in workspace.Accounts)
        {
            Accounts.Add(account);
        }

        RecentTransactions.Clear();
        foreach (var transaction in Snapshot.RecentTransactions)
        {
            RecentTransactions.Add(transaction);
        }

        CategorySpending.Clear();
        foreach (var category in Snapshot.CategorySpending)
        {
            CategorySpending.Add(category);
        }

        StatusMessage = message;
        AddSaleCommand.RaiseCanExecuteChanged();
    }
}
