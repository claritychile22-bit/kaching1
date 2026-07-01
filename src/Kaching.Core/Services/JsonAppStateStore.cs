using System.Text.Json;
using Kaching.Core.Models;

namespace Kaching.Core.Services;

public sealed class JsonAppStateStore(string filePath) : IAppStateStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<ShopifyMonitorState> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return new ShopifyMonitorState();
        }

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<ShopifyMonitorState>(stream, SerializerOptions, cancellationToken)
            ?? new ShopifyMonitorState();
    }

    public async Task SaveAsync(ShopifyMonitorState state, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, state, SerializerOptions, cancellationToken);
    }
}
