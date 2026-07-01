namespace Kaching.Core.Models;

public sealed record DashboardSnapshot(
    decimal NetWorth,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal SavingsRate,
    IReadOnlyList<CategorySpend> CategorySpending,
    IReadOnlyList<FinancialTransaction> RecentTransactions);

public sealed record CategorySpend(string Category, decimal Spent, decimal Limit, string AccentColor)
{
    public decimal Usage => Limit <= 0 ? 0 : Math.Min(1, Spent / Limit);
    public decimal Remaining => Math.Max(0, Limit - Spent);
}
