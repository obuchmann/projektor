using Microsoft.Win32;
using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Autostart;

/// <summary>
/// Windows autostart via the per-user Run registry key
/// (<c>HKCU\Software\Microsoft\Windows\CurrentVersion\Run</c>). Chosen over a Startup-folder
/// <c>.lnk</c> because it is silent and needs no COM shell-link interop — see ADR-0009.
/// </summary>
internal sealed class WindowsAutostartManager : IAutostartManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Projektor";

    public void Enable()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        var exec = Environment.ProcessPath;
        if (exec is not null)
            key.SetValue(ValueName, $"\"{exec}\"");
    }

    public void Disable()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    public bool IsEnabled()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        return key?.GetValue(ValueName) is not null;
    }
}
