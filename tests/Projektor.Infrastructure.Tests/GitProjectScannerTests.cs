using Projektor.Core.Models;
using Projektor.Infrastructure.Scanner;

namespace Projektor.Infrastructure.Tests;

public sealed class GitProjectScannerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "projektor-scan-" + Guid.NewGuid().ToString("N"));
    private readonly GitProjectScanner _scanner = new();

    public GitProjectScannerTests() => Directory.CreateDirectory(_root);
    public void Dispose() => Directory.Delete(_root, recursive: true);

    private void MakeRepo(string relative)
    {
        var dir = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.Combine(dir, ".git"));
    }

    [Fact]
    public void Scan_FindsGitReposAsScannedProjects()
    {
        MakeRepo("alpha");
        MakeRepo("nested/beta");
        Directory.CreateDirectory(Path.Combine(_root, "plain")); // no .git

        var result = _scanner.Scan(new ScanRoot(_root));

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "alpha");
        Assert.Contains(result, p => p.Name == "beta");
        Assert.All(result, p => Assert.Equal(ProjectSource.Scanned, p.Source));
    }

    [Fact]
    public void Scan_DoesNotDescendIntoRepository()
    {
        MakeRepo("outer");
        MakeRepo("outer/inner"); // should be ignored — outer is already a repo

        var result = _scanner.Scan(new ScanRoot(_root));

        Assert.Single(result);
        Assert.Equal("outer", result[0].Name);
    }

    [Fact]
    public void Scan_NonexistentRoot_ReturnsEmpty()
    {
        var result = _scanner.Scan(new ScanRoot(Path.Combine(_root, "does-not-exist")));

        Assert.Empty(result);
    }
}
