namespace Projektor.Core.Models;

public sealed record ProjektorConfig(
    IReadOnlyList<ActionTemplate> ActionTemplates,
    IReadOnlyList<Project> Projects,
    IReadOnlyList<ScanRoot> ScanRoots,
    AppSettings Settings)
{
    public static ProjektorConfig Empty => new(
        ActionTemplates: [],
        Projects: [],
        ScanRoots: [],
        Settings: new AppSettings());
}
