using Kaching.Core.Models;

namespace Kaching.Core.Services;

public static class SampleWorkspaceFactory
{
    public static FinanceWorkspace Create(DateOnly today)
    {
        var checking = Account.Create("Cuenta Corriente", "Banco Principal", 1250000);
        var savings = Account.Create("Ahorro Operacional", "Kaching Vault", 3400000);
        var credit = Account.Create("Tarjeta Corporativa", "Kaching Credit", -420000);

        return new FinanceWorkspace
        {
            Accounts = [checking, savings, credit],
            Budgets =
            [
                BudgetCategory.Create("Operaciones", 850000, "#2563EB"),
                BudgetCategory.Create("Marketing", 520000, "#7C3AED"),
                BudgetCategory.Create("Software", 380000, "#059669"),
                BudgetCategory.Create("Equipo", 720000, "#DC2626")
            ],
            Transactions =
            [
                FinancialTransaction.Create(checking.Id, today.AddDays(-1), "Venta online", "Ingresos", 1450000, TransactionType.Income),
                FinancialTransaction.Create(checking.Id, today.AddDays(-2), "Suscripciones SaaS", "Software", 179000, TransactionType.Expense, true),
                FinancialTransaction.Create(credit.Id, today.AddDays(-3), "Campana Meta Ads", "Marketing", 260000, TransactionType.Expense),
                FinancialTransaction.Create(checking.Id, today.AddDays(-4), "Arriendo oficina", "Operaciones", 390000, TransactionType.Expense, true),
                FinancialTransaction.Create(savings.Id, today.AddDays(-6), "Reserva mensual", "Ahorro", 500000, TransactionType.Transfer),
                FinancialTransaction.Create(checking.Id, today.AddDays(-8), "Pago cliente enterprise", "Ingresos", 2200000, TransactionType.Income),
                FinancialTransaction.Create(checking.Id, today.AddDays(-10), "Equipamiento ventas", "Equipo", 315000, TransactionType.Expense),
                FinancialTransaction.Create(checking.Id, today.AddDays(-12), "Servicios contables", "Operaciones", 180000, TransactionType.Expense, true)
            ]
        };
    }
}
