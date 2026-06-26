using System.Runtime.InteropServices;
using Projektor.Infrastructure.Process;

namespace Projektor.Infrastructure.Tests;

public sealed class ProcessLauncherTests
{
    [Fact]
    public void BuildStartInfo_SetsWorkingDirectoryAndNoWindow()
    {
        var psi = ProcessLauncher.BuildStartInfo("echo hi", "/dev/myapp");

        Assert.Equal("/dev/myapp", psi.WorkingDirectory);
        Assert.False(psi.UseShellExecute);
        Assert.True(psi.CreateNoWindow);
    }

    [Fact]
    public void BuildStartInfo_WrapsCommandInPlatformShell()
    {
        var psi = ProcessLauncher.BuildStartInfo("code .", "/dev/myapp");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Equal("cmd.exe", psi.FileName);
            Assert.Equal("/s /c \"code .\"", psi.Arguments);
        }
        else
        {
            Assert.Equal("/bin/bash", psi.FileName);
            // The command is passed verbatim as a single argv element (no quote mangling).
            Assert.Equal(new[] { "-c", "code ." }, psi.ArgumentList);
        }
    }

    [Fact]
    public void BuildStartInfo_PassesCommandVerbatim_WithoutMangling()
    {
        // A command containing quotes/special chars must survive intact.
        const string command = "git commit -m \"wip\" && code .";
        var psi = ProcessLauncher.BuildStartInfo(command, "/dev/myapp");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Equal($"/s /c \"{command}\"", psi.Arguments);
        }
        else
        {
            Assert.Equal(new[] { "-c", command }, psi.ArgumentList);
        }
    }

    [Fact]
    public void BuildStartInfo_Terminal_RunsCommandInsideVisibleTerminal()
    {
        var psi = ProcessLauncher.BuildStartInfo("npm run dev", "/dev/myapp", terminal: true);

        Assert.Equal("/dev/myapp", psi.WorkingDirectory);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // wt opens a real window; cmd /k keeps it open and resolves .cmd shims (npm, …).
            Assert.Equal("wt.exe", psi.FileName);
            Assert.True(psi.UseShellExecute);
            Assert.False(psi.CreateNoWindow);
            Assert.Equal(new[] { "-d", "/dev/myapp", "cmd", "/k", "npm run dev" }, psi.ArgumentList);
        }
        else
        {
            // The default terminal runs the tool; `exec bash` keeps the window open afterwards.
            Assert.Equal("x-terminal-emulator", psi.FileName);
            Assert.False(psi.UseShellExecute);
            Assert.Equal(new[] { "-e", "bash", "-c", "npm run dev; exec bash" }, psi.ArgumentList);
        }
    }
}
