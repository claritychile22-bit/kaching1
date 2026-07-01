namespace Kaching.Core.Models;

public sealed class ShopifyMonitorState
{
    public ShopifySettings Settings { get; set; } = ShopifySettings.Empty;
    public List<ShopifyOrder> SalesHistory { get; set; } = [];
    public DateTimeOffset? LastSuccessfulSync { get; set; }
    public string LastError { get; set; } = string.Empty;
}
