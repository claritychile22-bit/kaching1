using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kaching.Core.Models;

namespace Kaching.Core.Services;

public sealed class ShopifyService(HttpClient httpClient)
{
    public const string ApiVersion = "2026-07";

    private const string OrdersQuery = """
        query RecentOrders($query: String!) {
          orders(first: 25, sortKey: CREATED_AT, reverse: true, query: $query) {
            nodes {
              id
              name
              createdAt
              displayFinancialStatus
              displayFulfillmentStatus
              totalPriceSet { shopMoney { amount currencyCode } }
              customer { displayName }
            }
          }
        }
        """;

    public async Task<IReadOnlyList<ShopifyOrder>> GetRecentPaidOrdersAsync(
        ShopifySettings settings,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
    {
        var normalized = settings.Normalize();
        if (!normalized.HasCredentials)
        {
            return [];
        }

        var endpoint = new Uri($"https://{normalized.ShopDomain}/admin/api/{ApiVersion}/graphql.json");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-Shopify-Access-Token", normalized.AccessToken);

        var shopifyQuery = $"created_at:>={since.UtcDateTime:yyyy-MM-ddTHH:mm:ssZ} status:any";
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { query = OrdersQuery, variables = new { query = shopifyQuery } }),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(content);
        if (document.RootElement.TryGetProperty("errors", out var errors))
        {
            throw new InvalidOperationException($"Shopify rechazo la consulta: {errors}");
        }

        var nodes = document.RootElement
            .GetProperty("data")
            .GetProperty("orders")
            .GetProperty("nodes");

        return nodes.EnumerateArray()
            .Select(ParseOrder)
            .Where(order => string.Equals(order.FinancialStatus, "PAID", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(order.FinancialStatus, "AUTHORIZED", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(order => order.CreatedAt)
            .ToList();
    }

    private static ShopifyOrder ParseOrder(JsonElement node)
    {
        var money = node.GetProperty("totalPriceSet").GetProperty("shopMoney");
        var amountText = money.GetProperty("amount").GetString() ?? "0";
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);

        var customerName = node.TryGetProperty("customer", out var customer) && customer.ValueKind != JsonValueKind.Null
            ? customer.GetProperty("displayName").GetString() ?? string.Empty
            : string.Empty;

        return new ShopifyOrder(
            node.GetProperty("id").GetString() ?? string.Empty,
            node.GetProperty("name").GetString() ?? string.Empty,
            DateTimeOffset.Parse(node.GetProperty("createdAt").GetString() ?? DateTimeOffset.UtcNow.ToString("O"), CultureInfo.InvariantCulture),
            customerName,
            amount,
            money.GetProperty("currencyCode").GetString() ?? "USD",
            node.GetProperty("displayFinancialStatus").GetString() ?? string.Empty,
            node.GetProperty("displayFulfillmentStatus").GetString() ?? string.Empty);
    }
}
