using Projektor.Core.Models;

namespace Projektor.Core.Ports;

public interface IProcessLauncher
{
    void Launch(ProjectAction action, string workingDirectory);
}
