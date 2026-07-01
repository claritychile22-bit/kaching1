namespace Kaching.Core.Models;

public sealed record FinancialTransaction(
    Guid Id,
    Guid AccountId,
    DateOnly Date,
    string Description,
    string Category,
    decimal Amount,
    TransactionType Type,
    bool IsRecurring)
{
    public decimal SignedAmount => Type switch
    {
        TransactionType.Expense => -Math.Abs(Amount),
        TransactionType.Income => Math.Abs(Amount),
        _ => Amount
    };

    public static FinancialTransaction Create(
        Guid accountId,
        DateOnly date,
        string description,
        string category,
        decimal amount,
        TransactionType type,
        bool isRecurring = false) =>
        new(Guid.NewGuid(), accountId, date, description.Trim(), category.Trim(), Math.Abs(amount), type, isRecurring);
}
