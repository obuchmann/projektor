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
}
