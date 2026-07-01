namespace Kaching.Core.Models;

public sealed record ShopifySettings(
    string ShopDomain,
    string AccessToken,
    int PollIntervalSeconds,
    bool AutoStartWithWindows)
{
    public const int MinimumPollIntervalSeconds = 15;

    public bool HasCredentials => !string.IsNullOrWhiteSpace(ShopDomain) && !string.IsNullOrWhiteSpace(AccessToken);

    public ShopifySettings Normalize() => this with
    {
        ShopDomain = NormalizeDomain(ShopDomain),
        AccessToken = AccessToken.Trim(),
        PollIntervalSeconds = Math.Max(MinimumPollIntervalSeconds, PollIntervalSeconds)
    };

    public static ShopifySettings Empty => new(string.Empty, string.Empty, 30, false);

    private static string NormalizeDomain(string shopDomain)
    {
        var value = shopDomain.Trim().Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');

        return value.EndsWith(".myshopify.com", StringComparison.OrdinalIgnoreCase)
            ? value.ToLowerInvariant()
            : $"{value}.myshopify.com".ToLowerInvariant();
    }
}
