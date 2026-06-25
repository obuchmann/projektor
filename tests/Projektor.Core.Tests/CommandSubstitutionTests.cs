using Projektor.Core.Services;

namespace Projektor.Core.Tests;

public sealed class CommandSubstitutionTests
{
    [Fact]
    public void SubstituteCommand_WithPathPlaceholder_ReplacesPath()
    {
        var result = ActionResolver.SubstituteCommand("code {path}", "/home/user/myproject");

        Assert.Equal("code /home/user/myproject", result);
    }

    [Fact]
    public void SubstituteCommand_NoPathPlaceholder_CommandUnchanged()
    {
        var result = ActionResolver.SubstituteCommand("wezterm", "/home/user/myproject");

        Assert.Equal("wezterm", result);
    }

    [Fact]
    public void SubstituteCommand_MultiplePlaceholders_AllReplaced()
    {
        var result = ActionResolver.SubstituteCommand("echo {path} && code {path}", "/home/user/myproject");

        Assert.Equal("echo /home/user/myproject && code /home/user/myproject", result);
    }

    [Fact]
    public void SubstituteCommand_PathWithSpaces_PassedCorrectly()
    {
        var result = ActionResolver.SubstituteCommand("rider {path}", "/home/user/my project");

        Assert.Equal("rider /home/user/my project", result);
    }
}
