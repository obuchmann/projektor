namespace Projektor.Core.Models;

/// <summary>
/// A resolved, executable action — command is already OS-selected and {path}-substituted.
/// <paramref name="Terminal"/> requests the command run inside a visible terminal window
/// (for interactive CLI tools) instead of the hidden shell wrapper.
/// </summary>
public sealed record ProjectAction(
    string Id,
    string Name,
    string Command,
    bool Terminal = false);
