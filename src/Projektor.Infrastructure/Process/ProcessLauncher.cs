using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Projektor.Core.Models;
using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Process;

public sealed class ProcessLauncher : IProcessLauncher
{
    private readonly ILogger<ProcessLauncher> _logger;

    public ProcessLauncher(ILogger<ProcessLauncher>? logger = null)
    {
        _logger = logger ?? NullLogger<ProcessLauncher>.Instance;
    }

    /// <summary>
    /// Launches the action's command through the platform shell wrapper with the working
    /// directory set to the project path. Throws <see cref="ProcessLaunchException"/> when the
    /// process cannot be started so the UI can surface the failure.
    /// </summary>
    public void Launch(ProjectAction action, string workingDirectory)
    {
        ArgumentNullException.ThrowIfNull(action);

        var psi = BuildStartInfo(action.Command, workingDirectory);

        _logger.LogInformation(
            "Starte Action '{Action}': command={Command} | shell={Shell} {ShellArgs} | cwd={Cwd}",
            action.Name, action.Command, psi.FileName, DescribeArguments(psi), workingDirectory);

        System.Diagnostics.Process? process;
        try
        {
            process = System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex) when (ex is not ProcessLaunchException)
        {
            _logger.LogError(ex,
                "Action '{Action}' konnte nicht gestartet werden: {Message}", action.Name, ex.Message);
            throw new ProcessLaunchException(
                $"Konnte Action '{action.Name}' nicht starten: {ex.Message}", ex);
        }

        if (process is null)
        {
            _logger.LogError(
                "Action '{Action}' lieferte keinen Prozess (Process.Start gab null zurück).", action.Name);
            throw new ProcessLaunchException($"Konnte Action '{action.Name}' nicht starten.");
        }

        var pid = process.Id;
        _logger.LogInformation("Action '{Action}' gestartet (PID {Pid}).", action.Name, pid);

        // Observe the shell's exit code without blocking the caller: a non-zero code
        // (e.g. 127 = command not found) is the usual reason a launch "does nothing".
        WireUpExitLogging(process, action.Name, pid);
    }

    private void WireUpExitLogging(System.Diagnostics.Process process, string actionName, int pid)
    {
        try
        {
            process.EnableRaisingEvents = true;
        }
        catch (Exception ex)
        {
            // Best effort: if we cannot observe the exit, still release the handle.
            _logger.LogDebug(ex, "Exit-Überwachung für Action '{Action}' (PID {Pid}) nicht möglich.", actionName, pid);
            process.Dispose();
            return;
        }

        process.Exited += (_, _) =>
        {
            try
            {
                var code = process.ExitCode;
                if (code == 0)
                    _logger.LogInformation("Action '{Action}' (PID {Pid}) beendet mit Code 0.", actionName, pid);
                else
                    _logger.LogWarning(
                        "Action '{Action}' (PID {Pid}) beendet mit Code {Code} — Command vermutlich fehlgeschlagen.",
                        actionName, pid, code);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Exit-Code von Action '{Action}' (PID {Pid}) nicht lesbar.", actionName, pid);
            }
            finally
            {
                process.Dispose();
            }
        };
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
            // /s forces cmd to strip exactly the outer quotes and treat the rest verbatim,
            // which makes quoting predictable regardless of the command's contents.
            psi.Arguments = $"/s /c \"{command}\"";
        }
        else
        {
            psi.FileName = "/bin/bash";
            // Pass the command as a single, verbatim argv element via ArgumentList so it is not
            // re-parsed/mangled by the platform's argument-string tokenizer (backslashes, quotes).
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add(command);
        }

        return psi;
    }

    private static string DescribeArguments(ProcessStartInfo psi) =>
        psi.ArgumentList.Count > 0 ? string.Join(' ', psi.ArgumentList) : psi.Arguments;
}
