using Kaching.Core.Models;
using Kaching.Core.Services;
using Xunit;

namespace Kaching.Core.Tests;

public sealed class SalesAnalyticsTests
{
    [Fact]
    public void BuildSnapshot_ComputesTodayAndLifetimeSales()
    {
        var today = new DateOnly(2026, 7, 1);
        var orders = new[]
        {
            new ShopifyOrder("gid://shopify/Order/1", "#1001", new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero), "Ana", 25000, "CLP", "PAID", "UNFULFILLED"),
            new ShopifyOrder("gid://shopify/Order/2", "#1002", new DateTimeOffset(2026, 7, 1, 11, 0, 0, TimeSpan.Zero), "Luis", 45000, "CLP", "PAID", "UNFULFILLED"),
            new ShopifyOrder("gid://shopify/Order/3", "#0999", new DateTimeOffset(2026, 6, 30, 18, 0, 0, TimeSpan.Zero), "Marta", 15000, "CLP", "PAID", "FULFILLED")
        };

        var snapshot = new SalesAnalytics().BuildSnapshot(orders, today);

        Assert.Equal(2, snapshot.OrdersToday);
        Assert.Equal(70000, snapshot.SalesToday);
        Assert.Equal(85000, snapshot.TotalSold);
        Assert.Equal("CLP", snapshot.CurrencyCode);
        Assert.Equal("#1002", snapshot.LatestOrder?.Name);
    }

    [Fact]
    public void ShopifySettings_Normalize_AddsMyShopifyDomainAndMinimumInterval()
    {
        var settings = new ShopifySettings("MiTienda", " token ", 3, true).Normalize();

        Assert.Equal("mitienda.myshopify.com", settings.ShopDomain);
        Assert.Equal("token", settings.AccessToken);
        Assert.Equal(ShopifySettings.MinimumPollIntervalSeconds, settings.PollIntervalSeconds);
    }
}
