using Projektor.Core.Models;
using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Scanner;

public sealed class GitProjectScanner : IProjectScanner
{
    public IReadOnlyList<Project> Scan(ScanRoot root) => throw new NotImplementedException();
}
