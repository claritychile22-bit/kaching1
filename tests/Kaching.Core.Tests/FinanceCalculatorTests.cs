using Kaching.Core.Models;
using Kaching.Core.Services;
using Xunit;

namespace Kaching.Core.Tests;

public sealed class FinanceCalculatorTests
{
    [Fact]
    public void BuildSnapshot_ComputesMonthlyTotalsAndSavingsRate()
    {
        var today = new DateOnly(2026, 7, 15);
        var account = Account.Create("Main", "Bank", 1000);
        var workspace = new FinanceWorkspace
        {
            Accounts = [account],
            Budgets = [BudgetCategory.Create("Software", 500, "#2563EB")],
            Transactions =
            [
                FinancialTransaction.Create(account.Id, today, "Sale", "Income", 1000, TransactionType.Income),
                FinancialTransaction.Create(account.Id, today, "Tool", "Software", 250, TransactionType.Expense),
                FinancialTransaction.Create(account.Id, today.AddMonths(-1), "Old", "Software", 800, TransactionType.Expense)
            ]
        };

        var snapshot = new FinanceCalculator().BuildSnapshot(workspace, today);

        Assert.Equal(1950, snapshot.NetWorth);
        Assert.Equal(1000, snapshot.MonthlyIncome);
        Assert.Equal(250, snapshot.MonthlyExpenses);
        Assert.Equal(0.75m, snapshot.SavingsRate);
        Assert.Equal(250, snapshot.CategorySpending.Single().Spent);
    }

    [Fact]
    public void BuildSnapshot_ReturnsRecentTransactionsInDescendingDateOrder()
    {
        var today = new DateOnly(2026, 7, 15);
        var account = Account.Create("Main", "Bank", 0);
        var workspace = new FinanceWorkspace
        {
            Accounts = [account],
            Transactions =
            [
                FinancialTransaction.Create(account.Id, today.AddDays(-2), "Older", "Ops", 10, TransactionType.Expense),
                FinancialTransaction.Create(account.Id, today, "Newest", "Ops", 10, TransactionType.Expense)
            ]
        };

        var snapshot = new FinanceCalculator().BuildSnapshot(workspace, today);

        Assert.Equal("Newest", snapshot.RecentTransactions.First().Description);
    }
}
