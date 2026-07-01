namespace Kaching.Core.Models;

public sealed record ShopifyConnectionResult(
    bool IsSuccess,
    string Message,
    TimeSpan ResponseTime,
    string? ShopName = null)
{
    public static ShopifyConnectionResult Success(string shopName, TimeSpan responseTime) =>
        new(true, "Conectado correctamente", responseTime, shopName);

    public static ShopifyConnectionResult Failure(string message, TimeSpan responseTime) =>
        new(false, message, responseTime);
}
