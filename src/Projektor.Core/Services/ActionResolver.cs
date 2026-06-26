using Projektor.Core.Models;

namespace Projektor.Core.Services;

public sealed class ActionResolver
{
    /// <summary>
    /// Computes effective actions for a project:
    /// (global templates − disabled) + custom actions (override by Id or append).
    /// Each resulting action carries the command resolved for the current OS and
    /// with the <c>{path}</c> placeholder substituted by the project path.
    /// </summary>
    public IReadOnlyList<ProjectAction> Resolve(ProjektorConfig config, Project project)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(project);

        var disabled = new HashSet<string>(project.DisabledTemplateIds, StringComparer.OrdinalIgnoreCase);
        var result = new List<ProjectAction>();
        var indexById = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Global templates minus disabled ones, resolved for the current OS.
        foreach (var template in config.ActionTemplates)
        {
            if (disabled.Contains(template.Id))
                continue;

            var command = SelectCommand(template);
            if (string.IsNullOrWhiteSpace(command))
                continue; // Action not available on this platform.

            indexById[template.Id] = result.Count;
            result.Add(new ProjectAction(
                template.Id, template.Name, SubstituteCommand(command, project.Path), template.Terminal));
        }

        // Custom actions: override the template with the same Id, otherwise append.
        foreach (var custom in project.CustomActions)
        {
            var resolved = new ProjectAction(
                custom.Id, custom.Name, SubstituteCommand(custom.Command, project.Path), custom.Terminal);
            if (indexById.TryGetValue(custom.Id, out var existing))
                result[existing] = resolved;
            else
            {
                indexById[custom.Id] = result.Count;
                result.Add(resolved);
            }
        }

        return result;
    }

    /// <summary>
    /// Picks the command for the current operating system. Returns <c>null</c> when the
    /// action is not configured for this platform.
    /// </summary>
    private static string? SelectCommand(ActionTemplate template) =>
        OperatingSystem.IsWindows() ? template.CommandWindows : template.CommandLinux;

    /// <summary>
    /// Substitutes the <c>{path}</c> placeholder in a command string with the given project path.
    /// All occurrences are replaced; a command without the placeholder is returned unchanged.
    /// </summary>
    public static string SubstituteCommand(string command, string projectPath) =>
        command.Replace("{path}", projectPath, StringComparison.Ordinal);
}
