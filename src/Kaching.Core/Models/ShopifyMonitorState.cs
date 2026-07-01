namespace Kaching.Core.Models;

public sealed class ShopifyMonitorState
{
    public ShopifySettings Settings { get; set; } = ShopifySettings.Empty;
    public List<ShopifyOrder> SalesHistory { get; set; } = [];
    public string? LastProcessedOrderId { get; set; }
    public DateTimeOffset? LastProcessedOrderCreatedAt { get; set; }
    public DateTimeOffset? LastSuccessfulSync { get; set; }
    public TimeSpan? LastApiResponseTime { get; set; }
    public string LastError { get; set; } = string.Empty;

    public bool HasProcessedCursor => LastProcessedOrderCreatedAt is not null;
}
