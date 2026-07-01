namespace Kaching.Core.Models;

public sealed record Account(
    Guid Id,
    string Name,
    string Institution,
    decimal OpeningBalance,
    string Currency,
    bool IsActive)
{
    public static Account Create(string name, string institution, decimal openingBalance, string currency = "CLP") =>
        new(Guid.NewGuid(), name.Trim(), institution.Trim(), openingBalance, currency.Trim().ToUpperInvariant(), true);
}
