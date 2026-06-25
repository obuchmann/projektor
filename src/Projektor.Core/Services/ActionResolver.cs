using Projektor.Core.Models;

namespace Projektor.Core.Services;

public sealed class ActionResolver
{
    /// <summary>
    /// Computes effective actions for a project:
    /// (global templates − disabled) + custom actions (override by Id or append).
    /// </summary>
    public IReadOnlyList<ProjectAction> Resolve(ProjektorConfig config, Project project)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Substitutes {path} placeholder in a command string with the given project path.
    /// </summary>
    public static string SubstituteCommand(string command, string projectPath) =>
        throw new NotImplementedException();
}
