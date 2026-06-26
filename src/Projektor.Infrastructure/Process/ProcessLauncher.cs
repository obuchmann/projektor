using System.Diagnostics;
using Projektor.Core.Models;
using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Process;

public sealed class ProcessLauncher : IProcessLauncher
{
    /// <summary>
    /// Launches the action's command through the platform shell wrapper with the working
    /// directory set to the project path. Throws <see cref="ProcessLaunchException"/> when the
    /// process cannot be started so the UI can surface the failure.
    /// </summary>
    public void Launch(ProjectAction action, string workingDirectory)
    {
        ArgumentNullException.ThrowIfNull(action);

        var psi = BuildStartInfo(action.Command, workingDirectory);
        try
        {
            using var process = System.Diagnostics.Process.Start(psi);
            if (process is null)
                throw new ProcessLaunchException($"Konnte Action '{action.Name}' nicht starten.");
        }
        catch (Exception ex) when (ex is not ProcessLaunchException)
        {
            throw new ProcessLaunchException(
                $"Konnte Action '{action.Name}' nicht starten: {ex.Message}", ex);
        }
    }

    internal static ProcessStartInfo BuildStartInfo(string command, string workingDirectory)
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
