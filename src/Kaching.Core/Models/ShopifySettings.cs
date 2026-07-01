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

    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(ShopDomain))
        {
            return "Ingresa el dominio de la tienda Shopify.";
        }

        if (string.IsNullOrWhiteSpace(AccessToken))
        {
            return "Ingresa un Admin API access token.";
        }

        if (PollIntervalSeconds < MinimumPollIntervalSeconds)
        {
            return $"El intervalo minimo es {MinimumPollIntervalSeconds} segundos.";
        }

        var normalized = NormalizeDomain(ShopDomain);
        return Uri.CheckHostName(normalized) == UriHostNameType.Dns
            ? null
            : "El dominio Shopify no tiene un formato valido.";
    }

    public static ShopifySettings Empty => new(string.Empty, string.Empty, 30, false);

    private static string NormalizeDomain(string shopDomain)
    {
        var value = shopDomain.Trim().Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');

        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.EndsWith(".myshopify.com", StringComparison.OrdinalIgnoreCase)
            ? value.ToLowerInvariant()
            : $"{value}.myshopify.com".ToLowerInvariant();
    }
}
