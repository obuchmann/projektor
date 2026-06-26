using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Autostart;

/// <summary>
/// Linux autostart via an XDG desktop entry in <c>~/.config/autostart/projektor.desktop</c>.
/// </summary>
internal sealed class LinuxAutostartManager : IAutostartManager
{
    private static string AutostartDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config", "autostart");

    private static string DesktopFile => Path.Combine(AutostartDir, "projektor.desktop");

    public void Enable()
    {
        Directory.CreateDirectory(AutostartDir);
        var exec = Environment.ProcessPath ?? "projektor";
        var content = $"""
            [Desktop Entry]
            Type=Application
            Name=Projektor
            Comment=Projekt-Kontext Quick Launcher
            Exec={exec}
            Terminal=false
            X-GNOME-Autostart-enabled=true

            """;
        File.WriteAllText(DesktopFile, content);
    }

    public void Disable()
    {
        if (File.Exists(DesktopFile))
            File.Delete(DesktopFile);
    }

    public bool IsEnabled() => File.Exists(DesktopFile);
}
