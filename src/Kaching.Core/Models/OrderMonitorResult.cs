namespace Kaching.Core.Models;

public sealed record OrderMonitorResult(
    IReadOnlyList<ShopifyOrder> NewOrders,
    bool EstablishedBaseline,
    TimeSpan ApiResponseTime)
{
    public bool HasNewOrders => NewOrders.Count > 0;
}
