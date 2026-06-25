using System.Diagnostics;
using Projektor.Core.Models;
using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Process;

public sealed class ProcessLauncher : IProcessLauncher
{
    public void Launch(ProjectAction action, string workingDirectory) =>
        throw new NotImplementedException();

    private static System.Diagnostics.ProcessStartInfo BuildStartInfo(string command, string workingDirectory)
    {
        var psi = new ProcessStartInfo
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        if (OperatingSystem.IsWindows())
        {
            psi.FileName = "cmd.exe";
            psi.Arguments = $"/c \"{command}\"";
        }
        else
        {
            psi.FileName = "/bin/bash";
            psi.Arguments = $"-c \"{command}\"";
        }

        return psi;
    }
}
