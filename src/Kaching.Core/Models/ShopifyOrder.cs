namespace Kaching.Core.Models;

public sealed record ShopifyOrder(
    string Id,
    string Name,
    DateTimeOffset CreatedAt,
    string CustomerName,
    decimal TotalAmount,
    string CurrencyCode,
    string FinancialStatus,
    string FulfillmentStatus)
{
    public string DisplayCustomer => string.IsNullOrWhiteSpace(CustomerName) ? "Cliente sin nombre" : CustomerName;
}
