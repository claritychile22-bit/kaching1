using System.IO;
using System.Windows;
using Kaching.Core.Services;
using Kaching.Windows.ViewModels;

namespace Kaching.Windows;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kaching Windows",
            "workspace.json");

        var store = new JsonWorkspaceStore(dataPath);
        var viewModel = new MainViewModel(store, new FinanceCalculator());
        await viewModel.InitializeAsync();

        if (MainWindow is not null)
        {
            MainWindow.DataContext = viewModel;
        }
    }
}
