using Kaching.Core.Models;

namespace Kaching.Core.Services;

public sealed class FinanceCalculator
{
    public DashboardSnapshot BuildSnapshot(FinanceWorkspace workspace, DateOnly today)
    {
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthTransactions = workspace.Transactions
            .Where(transaction => transaction.Date >= monthStart && transaction.Date <= today)
            .ToList();

        var netWorth = workspace.Accounts.Sum(account => account.OpeningBalance) +
            workspace.Transactions.Sum(transaction => transaction.SignedAmount);

        var income = monthTransactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .Sum(transaction => Math.Abs(transaction.Amount));

        var expenses = monthTransactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .Sum(transaction => Math.Abs(transaction.Amount));

        var categorySpending = workspace.Budgets
            .Select(budget => new CategorySpend(
                budget.Name,
                monthTransactions
                    .Where(transaction => transaction.Type == TransactionType.Expense &&
                                          string.Equals(transaction.Category, budget.Name, StringComparison.OrdinalIgnoreCase))
                    .Sum(transaction => Math.Abs(transaction.Amount)),
                budget.MonthlyLimit,
                budget.AccentColor))
            .OrderByDescending(category => category.Usage)
            .ToList();

        var savingsRate = income <= 0 ? 0 : Math.Round((income - expenses) / income, 2);

        return new DashboardSnapshot(
            netWorth,
            income,
            expenses,
            savingsRate,
            categorySpending,
            workspace.Transactions
                .OrderByDescending(transaction => transaction.Date)
                .ThenBy(transaction => transaction.Description)
                .Take(8)
                .ToList());
    }
}
