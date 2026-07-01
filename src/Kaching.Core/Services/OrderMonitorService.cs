using Kaching.Core.Models;

namespace Kaching.Core.Services;

public sealed class OrderMonitorService(ShopifyService shopifyService)
{
    public async Task<IReadOnlyList<ShopifyOrder>> CheckForNewOrdersAsync(
        ShopifyMonitorState state,
        CancellationToken cancellationToken = default)
    {
        var since = state.SalesHistory.Count == 0
            ? DateTimeOffset.UtcNow.AddDays(-1)
            : state.SalesHistory.Max(order => order.CreatedAt).AddSeconds(1);

        var orders = await shopifyService.GetRecentPaidOrdersAsync(state.Settings, since, cancellationToken);
        var knownIds = state.SalesHistory.Select(order => order.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newOrders = orders.Where(order => !knownIds.Contains(order.Id)).OrderBy(order => order.CreatedAt).ToList();

        if (newOrders.Count > 0)
        {
            state.SalesHistory.InsertRange(0, newOrders.OrderByDescending(order => order.CreatedAt));
            state.SalesHistory = state.SalesHistory
                .OrderByDescending(order => order.CreatedAt)
                .Take(250)
                .ToList();
        }

        state.LastSuccessfulSync = DateTimeOffset.UtcNow;
        state.LastError = string.Empty;
        return newOrders;
    }
}
