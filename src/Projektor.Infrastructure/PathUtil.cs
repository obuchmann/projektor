namespace Projektor.Infrastructure;

/// <summary>
/// Small path helpers shared by infrastructure adapters.
/// </summary>
public static class PathUtil
{
    /// <summary>
    /// Expands a leading <c>~</c> (or <c>~/</c>) to the current user's home directory.
    /// Other paths are returned unchanged.
    /// </summary>
    public static string ExpandHome(string path)
    {
        if (string.IsNullOrEmpty(path) || path[0] != '~')
            return path;

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (path.Length == 1)
            return home;

        if (path[1] == '/' || path[1] == '\\')
            return Path.Combine(home, path[2..]);

        return path;
    }
}
