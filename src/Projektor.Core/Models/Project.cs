namespace Projektor.Core.Models;

public sealed record Project(
    string Name,
    string Path,
    ProjectSource Source,
    IReadOnlyList<string> DisabledTemplateIds,
    IReadOnlyList<ProjectAction> CustomActions);
