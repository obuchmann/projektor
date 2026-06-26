using Projektor.Core.Models;
using Projektor.Infrastructure.Config;

namespace Projektor.Infrastructure.Tests;

public sealed class TomlConfigStoreRoundtripTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public TomlConfigStoreRoundtripTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    private string TempFile() => Path.Combine(_tempDir, $"{Guid.NewGuid()}.toml");

    [Fact]
    public void Load_MinimalConfig_LoadsWithoutError()
    {
        var path = TempFile();
        File.WriteAllText(path, """
            [settings]
            hotkey = "Alt+Space"
            theme = "dark"
            """);
        var store = new TomlConfigStore(path);

        var config = store.Load();

        Assert.NotNull(config);
        Assert.Equal("Alt+Space", config.Settings.Hotkey);
    }

    [Fact]
    public void Load_FullConfig_ModelCorrectlyPopulated()
    {
        var path = TempFile();
        File.WriteAllText(path, """
            [settings]
            hotkey = "Alt+Space"
            theme = "dark"

            [[action_templates]]
            id = "terminal"
            name = "Terminal hier"
            command_windows = "wt"
            command_linux = "wezterm"

            [[projects]]
            name = "Projektor"
            path = "~/dev/projektor"
            source = "manual"
            disabled_templates = []

            [[scan_roots]]
            path = "~/dev"
            """);
        var store = new TomlConfigStore(path);

        var config = store.Load();

        Assert.Single(config.ActionTemplates);
        Assert.Equal("terminal", config.ActionTemplates[0].Id);
        Assert.Single(config.Projects);
        Assert.Equal("Projektor", config.Projects[0].Name);
        Assert.Single(config.ScanRoots);
    }

    [Fact]
    public void Load_MissingOptionalFields_DefaultsApplied()
    {
        var path = TempFile();
        File.WriteAllText(path, "");
        var store = new TomlConfigStore(path);

        var config = store.Load();

        Assert.NotNull(config);
        Assert.Empty(config.ActionTemplates);
        Assert.Empty(config.Projects);
        Assert.Equal("Alt+Space", config.Settings.Hotkey);
    }

    [Fact]
    public void Save_ThenLoad_ProducesSameModel()
    {
        var path = TempFile();
        var original = new ProjektorConfig(
            ActionTemplates: [new ActionTemplate("ide", "Rider", "rider64 {path}", "rider {path}")],
            Projects: [new Project("MyApp", "/dev/myapp", ProjectSource.Manual, [], [])],
            ScanRoots: [new ScanRoot("/dev")],
            Settings: new AppSettings("Alt+Space", "dark"));

        var store = new TomlConfigStore(path);
        store.Save(original);
        var loaded = store.Load();

        Assert.Equal(original.ActionTemplates[0].Id, loaded.ActionTemplates[0].Id);
        Assert.Equal(original.Projects[0].Name, loaded.Projects[0].Name);
        Assert.Equal(original.ScanRoots[0].Path, loaded.ScanRoots[0].Path);
        Assert.Equal(original.Settings.Hotkey, loaded.Settings.Hotkey);
    }

    [Fact]
    public void Save_ProjectWithDisabledTemplatesAndCustomActions_Roundtrips()
    {
        var path = TempFile();
        var original = new ProjektorConfig(
            ActionTemplates: [new ActionTemplate("ide", "Rider", "rider64 {path}", "rider {path}")],
            Projects:
            [
                new Project("MyApp", "/dev/my app", ProjectSource.Scanned,
                    DisabledTemplateIds: ["ide"],
                    CustomActions: [new ProjectAction("run", "Run", "dotnet run")]),
            ],
            ScanRoots: [],
            Settings: new AppSettings());

        var store = new TomlConfigStore(path);
        store.Save(original);
        var loaded = store.Load();

        var project = Assert.Single(loaded.Projects);
        Assert.Equal(ProjectSource.Scanned, project.Source);
        Assert.Equal("/dev/my app", project.Path);
        Assert.Equal(["ide"], project.DisabledTemplateIds);
        var action = Assert.Single(project.CustomActions);
        Assert.Equal("run", action.Id);
        Assert.Equal("dotnet run", action.Command);
    }

    [Fact]
    public void Save_UsesHeaderBlockStyle_NotInlineTables()
    {
        var path = TempFile();
        var config = new ProjektorConfig(
            ActionTemplates: [new ActionTemplate("ide", "Rider", "rider64 {path}", "rider {path}")],
            Projects: [],
            ScanRoots: [],
            Settings: new AppSettings());

        new TomlConfigStore(path).Save(config);
        var text = File.ReadAllText(path);

        Assert.Contains("[[action_templates]]", text);
        Assert.DoesNotContain("action_templates = [{", text);
    }
}
