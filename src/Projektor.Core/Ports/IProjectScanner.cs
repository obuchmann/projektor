using Projektor.Core.Models;

namespace Projektor.Core.Ports;

public interface IProjectScanner
{
    IReadOnlyList<Project> Scan(ScanRoot root);
}
