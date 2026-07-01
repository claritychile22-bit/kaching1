using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kaching.Core.Models;

namespace Kaching.Core.Services;

public sealed class ShopifyService(HttpClient httpClient)
{
    public const string ApiVersion = "2026-07";

    private const string ShopQuery = """
        query ShopConnectionTest {
          shop { name myshopifyDomain }
        }
        """;

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

    public async Task<ShopifyConnectionResult> TestConnectionAsync(
        ShopifySettings settings,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var validationError = settings.Validate();
        if (validationError is not null)
        {
            return ShopifyConnectionResult.Failure(validationError, stopwatch.Elapsed);
        }

        try
        {
            using var document = await ExecuteGraphQlAsync(settings.Normalize(), ShopQuery, null, cancellationToken);
            stopwatch.Stop();
            var shop = document.RootElement.GetProperty("data").GetProperty("shop");
            var name = shop.GetProperty("name").GetString() ?? settings.ShopDomain;
            return ShopifyConnectionResult.Success(name, stopwatch.Elapsed);
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or JsonException or TaskCanceledException)
        {
            stopwatch.Stop();
            return ShopifyConnectionResult.Failure(DescribeConnectionError(ex), stopwatch.Elapsed);
        }
    }

    public async Task<(IReadOnlyList<ShopifyOrder> Orders, TimeSpan ResponseTime)> GetRecentPaidOrdersAsync(
        ShopifySettings settings,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
    {
        var normalized = settings.Normalize();
        var validationError = normalized.Validate();
        if (validationError is not null)
        {
            throw new InvalidOperationException(validationError);
        }

        var stopwatch = Stopwatch.StartNew();
        var shopifyQuery = $"created_at:>={since.UtcDateTime:yyyy-MM-ddTHH:mm:ssZ} status:any";
        using var document = await ExecuteGraphQlAsync(
            normalized,
            OrdersQuery,
            new Dictionary<string, object?> { ["query"] = shopifyQuery },
            cancellationToken);
        stopwatch.Stop();

        var nodes = document.RootElement
            .GetProperty("data")
            .GetProperty("orders")
            .GetProperty("nodes");

        var orders = nodes.EnumerateArray()
            .Select(ParseOrder)
            .Where(order => string.Equals(order.FinancialStatus, "PAID", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(order.FinancialStatus, "AUTHORIZED", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(order => order.CreatedAt)
            .ToList();

        return (orders, stopwatch.Elapsed);
    }

    private async Task<JsonDocument> ExecuteGraphQlAsync(
        ShopifySettings settings,
        string query,
        IReadOnlyDictionary<string, object?>? variables,
        CancellationToken cancellationToken)
    {
        var endpoint = new Uri($"https://{settings.ShopDomain}/admin/api/{ApiVersion}/graphql.json");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-Shopify-Access-Token", settings.AccessToken);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { query, variables }),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds;
            throw new InvalidOperationException(retryAfter is null
                ? "Shopify limito temporalmente las consultas. El monitor reintentara automaticamente."
                : $"Shopify limito temporalmente las consultas. Reintenta en {retryAfter:N0} segundos.");
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException("El token no es valido o no tiene permisos para leer pedidos.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException("No se encontro la tienda Shopify. Revisa el dominio.");
        }

        response.EnsureSuccessStatusCode();

        var document = JsonDocument.Parse(content);
        if (document.RootElement.TryGetProperty("errors", out var errors))
        {
            document.Dispose();
            throw new InvalidOperationException($"Shopify rechazo la consulta: {errors}");
        }

        return document;
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

    private static string DescribeConnectionError(Exception ex) => ex switch
    {
        TaskCanceledException => "La conexion con Shopify expiro. Revisa internet o intenta de nuevo.",
        HttpRequestException => "No se pudo conectar con Shopify. Revisa el dominio y tu conexion.",
        _ => ex.Message
    };
}
