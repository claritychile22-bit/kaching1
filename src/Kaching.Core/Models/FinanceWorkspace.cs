namespace Kaching.Core.Models;

public sealed class FinanceWorkspace
{
    public List<Account> Accounts { get; init; } = [];
    public List<FinancialTransaction> Transactions { get; init; } = [];
    public List<BudgetCategory> Budgets { get; init; } = [];

    public bool IsEmpty => Accounts.Count == 0 && Transactions.Count == 0 && Budgets.Count == 0;
}
