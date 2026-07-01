using Kaching.Core.Models;

namespace Kaching.Core.Services;

public interface IAppStateStore
{
    Task<ShopifyMonitorState> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ShopifyMonitorState state, CancellationToken cancellationToken = default);
}
