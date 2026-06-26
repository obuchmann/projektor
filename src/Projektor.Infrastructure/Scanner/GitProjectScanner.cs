using Projektor.Core.Models;
using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Scanner;

/// <summary>
/// Discovers projects under a scan root by locating <c>.git</c> entries. Each repository
/// directory becomes a <see cref="ProjectSource.Scanned"/> project. Does not descend into a
/// repository once found, and skips directories it cannot access.
/// </summary>
public sealed class GitProjectScanner : IProjectScanner
{
    public IReadOnlyList<Project> Scan(ScanRoot root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var start = PathUtil.ExpandHome(root.Path);
        if (!Directory.Exists(start))
            return [];

        var projects = new List<Project>();
        Walk(start, projects);
        return projects;
    }

    private static void Walk(string dir, List<Project> projects)
    {
        if (IsGitRepo(dir))
        {
            projects.Add(new Project(
                Name: Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                Path: dir,
                Source: ProjectSource.Scanned,
                DisabledTemplateIds: [],
                CustomActions: []));
            return; // Don't descend into a repository.
        }

        string[] subdirs;
        try
        {
            subdirs = Directory.GetDirectories(dir);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException or IOException)
        {
            return;
        }

        foreach (var sub in subdirs)
            Walk(sub, projects);
    }

    private static bool IsGitRepo(string dir)
    {
        var git = Path.Combine(dir, ".git");
        // A working tree has a .git directory; worktrees/submodules use a .git file.
        return Directory.Exists(git) || File.Exists(git);
    }
}
