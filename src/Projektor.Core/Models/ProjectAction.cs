namespace Projektor.Core.Models;

/// <summary>
/// A resolved, executable action — command is already OS-selected and {path}-substituted.
/// </summary>
public sealed record ProjectAction(
    string Id,
    string Name,
    string Command);
