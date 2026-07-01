using System.Text.Json;
using Kaching.Core.Models;

namespace Kaching.Core.Services;

public sealed class JsonWorkspaceStore(string filePath) : IWorkspaceStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<FinanceWorkspace> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return new FinanceWorkspace();
        }

        await using var stream = File.OpenRead(filePath);
        var workspace = await JsonSerializer.DeserializeAsync<FinanceWorkspace>(stream, SerializerOptions, cancellationToken);
        return workspace ?? new FinanceWorkspace();
    }

    public async Task SaveAsync(FinanceWorkspace workspace, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, workspace, SerializerOptions, cancellationToken);
    }
}
