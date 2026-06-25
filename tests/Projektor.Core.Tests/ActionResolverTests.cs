using Projektor.Core.Models;
using Projektor.Core.Services;

namespace Projektor.Core.Tests;

public sealed class ActionResolverTests
{
    private readonly ActionResolver _resolver = new();

    private static ActionTemplate Template(string id, string name = "Template") =>
        new(id, name, CommandWindows: $"{id}.exe", CommandLinux: id);

    private static ProjectAction CustomAction(string id, string name = "Custom") =>
        new(id, name, Command: $"custom-{id}");

    private static Project EmptyProject(
        string[]? disabled = null,
        ProjectAction[]? custom = null) =>
        new("MyProject", "/projects/my", ProjectSource.Manual,
            DisabledTemplateIds: disabled ?? [],
            CustomActions: custom ?? []);

    [Fact]
    public void Resolve_NoDisabledNoCustom_ReturnsAllTemplates()
    {
        var config = new ProjektorConfig(
            ActionTemplates: [Template("ide"), Template("terminal")],
            Projects: [],
            ScanRoots: [],
            Settings: new AppSettings());
        var project = EmptyProject();

        var result = _resolver.Resolve(config, project);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Resolve_DisabledTemplate_IsExcludedFromResult()
    {
        var config = new ProjektorConfig(
            ActionTemplates: [Template("ide"), Template("terminal")],
            Projects: [],
            ScanRoots: [],
            Settings: new AppSettings());
        var project = EmptyProject(disabled: ["ide"]);

        var result = _resolver.Resolve(config, project);

        Assert.Single(result);
        Assert.Equal("terminal", result[0].Id);
    }

    [Fact]
    public void Resolve_CustomActionWithSameId_OverridesTemplate()
    {
        var config = new ProjektorConfig(
            ActionTemplates: [Template("ide", "Rider")],
            Projects: [],
            ScanRoots: [],
            Settings: new AppSettings());
        var project = EmptyProject(custom: [new ProjectAction("ide", "VS Code Override", "code {path}")]);

        var result = _resolver.Resolve(config, project);

        Assert.Single(result);
        Assert.Equal("VS Code Override", result[0].Name);
    }

    [Fact]
    public void Resolve_CustomActionWithNewId_IsAppended()
    {
        var config = new ProjektorConfig(
            ActionTemplates: [Template("terminal")],
            Projects: [],
            ScanRoots: [],
            Settings: new AppSettings());
        var project = EmptyProject(custom: [CustomAction("run")]);

        var result = _resolver.Resolve(config, project);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.Id == "terminal");
        Assert.Contains(result, a => a.Id == "run");
    }

    [Fact]
    public void Resolve_DisableAndOverrideAndExtra_CombinesCorrectly()
    {
        var config = new ProjektorConfig(
            ActionTemplates: [Template("ide"), Template("terminal"), Template("editor")],
            Projects: [],
            ScanRoots: [],
            Settings: new AppSettings());
        var project = EmptyProject(
            disabled: ["editor"],
            custom: [new ProjectAction("ide", "Override IDE", "rider {path}"), CustomAction("run")]);

        var result = _resolver.Resolve(config, project);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, a => a.Id == "terminal");
        Assert.Contains(result, a => a.Id == "ide" && a.Name == "Override IDE");
        Assert.Contains(result, a => a.Id == "run");
        Assert.DoesNotContain(result, a => a.Id == "editor");
    }

    [Fact]
    public void Resolve_EmptyProject_ReturnsAllGlobalTemplates()
    {
        var config = new ProjektorConfig(
            ActionTemplates: [Template("ide"), Template("terminal")],
            Projects: [],
            ScanRoots: [],
            Settings: new AppSettings());
        var project = EmptyProject();

        var result = _resolver.Resolve(config, project);

        Assert.Equal(2, result.Count);
    }
}
