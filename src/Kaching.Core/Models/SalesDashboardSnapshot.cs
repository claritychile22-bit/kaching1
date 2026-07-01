namespace Kaching.Core.Models;

public sealed record SalesDashboardSnapshot(
    int OrdersToday,
    decimal SalesToday,
    decimal TotalSold,
    string CurrencyCode,
    ShopifyOrder? LatestOrder,
    IReadOnlyList<ShopifyOrder> RecentSales);
