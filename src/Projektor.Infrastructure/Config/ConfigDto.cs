using Projektor.Core.Models;
using Tomlyn.Serialization;

namespace Projektor.Infrastructure.Config;

// Mutable DTOs for Tomlyn deserialization (loading). TOML keys are pinned explicitly via
// [TomlPropertyName] so the on-disk schema stays stable regardless of Tomlyn's default
// naming policy. These mirror the schema in Docs/Architecture.md §7 and map to the immutable
// Core records in ToModel(). Writing is handled by TomlConfigWriter (block-style output).

internal sealed class ConfigDto
{
    [TomlPropertyName("settings")]
    public SettingsDto Settings { get; set; } = new();

    [TomlPropertyName("action_templates")]
    public List<ActionTemplateDto> ActionTemplates { get; set; } = [];

    [TomlPropertyName("projects")]
    public List<ProjectDto> Projects { get; set; } = [];

    [TomlPropertyName("scan_roots")]
    public List<ScanRootDto> ScanRoots { get; set; } = [];

    public ProjektorConfig ToModel() => new(
        ActionTemplates: ActionTemplates.Select(t => t.ToModel()).ToList(),
        Projects: Projects.Select(p => p.ToModel()).ToList(),
        ScanRoots: ScanRoots.Select(r => new ScanRoot(r.Path)).ToList(),
        Settings: Settings.ToModel());
}

internal sealed class SettingsDto
{
    [TomlPropertyName("hotkey")]
    public string Hotkey { get; set; } = "Alt+Space";

    [TomlPropertyName("theme")]
    public string Theme { get; set; } = "dark";

    public AppSettings ToModel() => new(Hotkey, Theme);
}

internal sealed class ActionTemplateDto
{
    [TomlPropertyName("id")]
    public string Id { get; set; } = "";

    [TomlPropertyName("name")]
    public string Name { get; set; } = "";

    [TomlPropertyName("command_windows")]
    public string? CommandWindows { get; set; }

    [TomlPropertyName("command_linux")]
    public string? CommandLinux { get; set; }

    [TomlPropertyName("icon")]
    public string? Icon { get; set; }

    [TomlPropertyName("terminal")]
    public bool Terminal { get; set; }

    public ActionTemplate ToModel() => new(Id, Name, CommandWindows, CommandLinux, Icon, Terminal);
}

internal sealed class ProjectDto
{
    [TomlPropertyName("name")]
    public string Name { get; set; } = "";

    [TomlPropertyName("path")]
    public string Path { get; set; } = "";

    [TomlPropertyName("source")]
    public string Source { get; set; } = "manual";

    [TomlPropertyName("disabled_templates")]
    public List<string> DisabledTemplates { get; set; } = [];

    [TomlPropertyName("actions")]
    public List<ProjectActionDto> Actions { get; set; } = [];

    public Project ToModel() => new(
        Name,
        Path,
        Enum.TryParse<ProjectSource>(Source, ignoreCase: true, out var src) ? src : ProjectSource.Manual,
        DisabledTemplateIds: DisabledTemplates,
        CustomActions: Actions.Select(a => a.ToModel()).ToList());
}

// Per-project custom actions are single-command in v1 (see ADR-0008), matching the
// resolved Core.ProjectAction. Cross-platform per-project commands are deferred.
internal sealed class ProjectActionDto
{
    [TomlPropertyName("id")]
    public string Id { get; set; } = "";

    [TomlPropertyName("name")]
    public string Name { get; set; } = "";

    [TomlPropertyName("command")]
    public string Command { get; set; } = "";

    [TomlPropertyName("terminal")]
    public bool Terminal { get; set; }

    public ProjectAction ToModel() => new(Id, Name, Command, Terminal);
}

internal sealed class ScanRootDto
{
    [TomlPropertyName("path")]
    public string Path { get; set; } = "";
}
