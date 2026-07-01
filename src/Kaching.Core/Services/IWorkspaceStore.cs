using Kaching.Core.Models;

namespace Kaching.Core.Services;

public interface IWorkspaceStore
{
    Task<FinanceWorkspace> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(FinanceWorkspace workspace, CancellationToken cancellationToken = default);
}
