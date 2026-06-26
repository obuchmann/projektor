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
            Assert.Equal("/c \"code .\"", psi.Arguments);
        }
        else
        {
            Assert.Equal("/bin/bash", psi.FileName);
            Assert.Equal("-c \"code .\"", psi.Arguments);
        }
    }
}
