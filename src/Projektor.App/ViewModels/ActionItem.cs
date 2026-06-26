using Projektor.Core.Models;

namespace Projektor.App.ViewModels;

/// <summary>
/// A resolved action plus its 1-based position, used to render the numbered action list and
/// drive the <c>Alt+N</c> shortcuts in the overlay.
/// </summary>
public sealed class ActionItem(int number, ProjectAction action)
{
    public int Number { get; } = number;
    public ProjectAction Action { get; } = action;

    public string Name => Action.Name;
    public string Command => Action.Command;

    /// <summary>Keyboard hint shown next to the action ("Alt+1"…"Alt+9"); empty beyond 9.</summary>
    public string ShortcutLabel => Number <= 9 ? $"Alt+{Number}" : string.Empty;
}
