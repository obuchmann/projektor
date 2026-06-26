using Projektor.Core.Models;

namespace Projektor.Infrastructure.Config;

/// <summary>
/// Starter configuration written on first run so the app is usable out of the box.
/// Ships the example "Terminal hier" template from the PRD plus common IDE/editor templates
/// and a sample project pointing at the user's home directory.
/// </summary>
public static class DefaultConfig
{
    public static ProjektorConfig Create()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        return new ProjektorConfig(
            ActionTemplates:
            [
                new ActionTemplate("terminal", "Terminal hier", CommandWindows: "wt", CommandLinux: "x-terminal-emulator"),
                new ActionTemplate("editor", "VS Code", CommandWindows: "code {path}", CommandLinux: "code {path}"),
                new ActionTemplate("files", "Dateimanager", CommandWindows: "explorer .", CommandLinux: "xdg-open ."),
            ],
            Projects:
            [
                new Project(
                    Name: "Home",
                    Path: home,
                    Source: ProjectSource.Manual,
                    DisabledTemplateIds: [],
                    CustomActions: []),
            ],
            ScanRoots: [],
            Settings: new AppSettings());
    }
}
