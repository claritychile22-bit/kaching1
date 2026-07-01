using Kaching.Core.Models;

namespace Kaching.Core.Services;

public sealed class OrderMonitorService(ShopifyService shopifyService)
{
    private const int MaxHistoryItems = 500;

    public async Task<OrderMonitorResult> CheckForNewOrdersAsync(
        ShopifyMonitorState state,
        CancellationToken cancellationToken = default)
    {
        var since = state.LastProcessedOrderCreatedAt ?? DateTimeOffset.UtcNow.AddDays(-7);
        var (orders, responseTime) = await shopifyService.GetRecentPaidOrdersAsync(state.Settings, since, cancellationToken);
        state.LastApiResponseTime = responseTime;
        state.LastSuccessfulSync = DateTimeOffset.UtcNow;
        state.LastError = string.Empty;

        if (!state.HasProcessedCursor)
        {
            EstablishBaseline(state, orders);
            return new OrderMonitorResult([], true, responseTime);
        }

        var knownIds = state.SalesHistory.Select(order => order.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(state.LastProcessedOrderId))
        {
            knownIds.Add(state.LastProcessedOrderId);
        }

        var newOrders = orders
            .Where(order => IsNewerThanCursor(state, order))
            .Where(order => !knownIds.Contains(order.Id))
            .OrderBy(order => order.CreatedAt)
            .ToList();

        if (newOrders.Count > 0)
        {
            AddOrdersToHistory(state, newOrders);
            SetCursor(state, newOrders.MaxBy(order => order.CreatedAt)!);
        }

        return new OrderMonitorResult(newOrders, false, responseTime);
    }

    public void RecordSimulatedSale(ShopifyMonitorState state, ShopifyOrder order)
    {
        AddOrdersToHistory(state, [order]);
        SetCursor(state, order);
    }

    private static void EstablishBaseline(ShopifyMonitorState state, IReadOnlyList<ShopifyOrder> orders)
    {
        var latest = orders.OrderByDescending(order => order.CreatedAt).FirstOrDefault();
        if (latest is not null)
        {
            SetCursor(state, latest);
        }
        else
        {
            state.LastProcessedOrderId = null;
            state.LastProcessedOrderCreatedAt = DateTimeOffset.UtcNow;
        }
    }

    private static bool IsNewerThanCursor(ShopifyMonitorState state, ShopifyOrder order)
    {
        if (state.LastProcessedOrderCreatedAt is null)
        {
            return false;
        }

        if (order.CreatedAt > state.LastProcessedOrderCreatedAt)
        {
            return true;
        }

        return order.CreatedAt == state.LastProcessedOrderCreatedAt &&
               !string.Equals(order.Id, state.LastProcessedOrderId, StringComparison.OrdinalIgnoreCase);
    }

    private static void AddOrdersToHistory(ShopifyMonitorState state, IReadOnlyList<ShopifyOrder> orders)
    {
        var existing = state.SalesHistory.Select(order => order.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var order in orders.Where(order => existing.Add(order.Id)))
        {
            state.SalesHistory.Insert(0, order);
        }

        state.SalesHistory = state.SalesHistory
            .OrderByDescending(order => order.CreatedAt)
            .Take(MaxHistoryItems)
            .ToList();
    }

    private static void SetCursor(ShopifyMonitorState state, ShopifyOrder order)
    {
        state.LastProcessedOrderId = order.Id;
        state.LastProcessedOrderCreatedAt = order.CreatedAt;
    }
}
