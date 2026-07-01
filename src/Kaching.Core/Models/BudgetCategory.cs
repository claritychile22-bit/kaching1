namespace Kaching.Core.Models;

public sealed record BudgetCategory(
    Guid Id,
    string Name,
    decimal MonthlyLimit,
    string AccentColor)
{
    public static BudgetCategory Create(string name, decimal monthlyLimit, string accentColor) =>
        new(Guid.NewGuid(), name.Trim(), Math.Max(0, monthlyLimit), accentColor.Trim());
}
