namespace Projektor.Infrastructure.Config;

internal static class ConfigPathResolver
{
    private const string FileName = "projektor.toml";
    private const string AppName = "Projektor";

    /// <summary>
    /// Portable mode: use projektor.toml next to the executable.
    /// Otherwise: platform-specific user config directory.
    /// </summary>
    public static string Resolve()
    {
        var portablePath = Path.Combine(AppContext.BaseDirectory, FileName);
        if (File.Exists(portablePath))
            return portablePath;

        var configDir = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "projektor");

        Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, FileName);
    }
}
