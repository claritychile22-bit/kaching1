using System.Globalization;
using Kaching.Core.Models;

namespace Kaching.Core.Services;

public sealed class SalesLogService(string logFilePath)
{
    public async Task LogSaleAsync(ShopifyOrder order, CancellationToken cancellationToken = default)
    {
        var line = string.Join(',',
            Escape(order.CreatedAt.LocalDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            Escape(order.CreatedAt.LocalDateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
            Escape(order.Name),
            Escape(order.DisplayCustomer),
            Escape(order.TotalAmount.ToString(CultureInfo.InvariantCulture)),
            Escape(order.CurrencyCode),
            Escape(order.FinancialStatus));

        await AppendAsync(line, cancellationToken);
    }

    public async Task LogErrorAsync(string message, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var line = string.Join(',',
            Escape(now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            Escape(now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
            Escape("ERROR"),
            Escape(message));

        await AppendAsync(line, cancellationToken);
    }

    private async Task AppendAsync(string line, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var needsHeader = !File.Exists(logFilePath);
        await using var stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        await using var writer = new StreamWriter(stream);
        if (needsHeader)
        {
            await writer.WriteLineAsync("Date,Time,Order,Customer,Amount,Currency,PaymentStatus");
        }

        await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
    }

    private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
