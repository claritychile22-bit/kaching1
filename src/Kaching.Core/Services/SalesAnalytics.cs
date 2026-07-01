using Kaching.Core.Models;

namespace Kaching.Core.Services;

public sealed class SalesAnalytics
{
    public SalesDashboardSnapshot BuildSnapshot(IEnumerable<ShopifyOrder> orders, DateOnly today)
    {
        var sales = orders.OrderByDescending(order => order.CreatedAt).ToList();
        var todayOrders = sales.Where(order => DateOnly.FromDateTime(order.CreatedAt.LocalDateTime) == today).ToList();
        var currency = sales.FirstOrDefault()?.CurrencyCode ?? "USD";

        return new SalesDashboardSnapshot(
            todayOrders.Count,
            todayOrders.Sum(order => order.TotalAmount),
            sales.Sum(order => order.TotalAmount),
            currency,
            sales.FirstOrDefault(),
            sales.Take(25).ToList());
    }
}
