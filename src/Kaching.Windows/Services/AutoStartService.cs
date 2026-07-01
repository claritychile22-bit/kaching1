using Microsoft.Win32;

namespace Kaching.Windows.Services;

public sealed class AutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Kaching Windows";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
        return !string.IsNullOrWhiteSpace(key?.GetValue(AppName) as string);
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, true);

        if (enabled)
        {
            var executable = Environment.ProcessPath ?? string.Empty;
            key.SetValue(AppName, $"\"{executable}\"");
        }
        else
        {
            key.DeleteValue(AppName, false);
        }
    }
}
