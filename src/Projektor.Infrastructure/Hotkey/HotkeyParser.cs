using SharpHook.Data;

namespace Projektor.Infrastructure.Hotkey;

/// <summary>
/// Parses a textual hotkey definition such as <c>"Alt+Space"</c> or <c>"Ctrl+Shift+P"</c>
/// into a SharpHook modifier mask plus the target <see cref="KeyCode"/>.
/// The last token is the key; all preceding tokens are modifiers.
/// </summary>
internal static class HotkeyParser
{
    public static bool TryParse(string hotkey, out EventMask modifiers, out KeyCode key)
    {
        modifiers = EventMask.None;
        key = KeyCode.VcUndefined;

        if (string.IsNullOrWhiteSpace(hotkey))
            return false;

        var tokens = hotkey.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
            return false;

        var keyToken = tokens[^1];
        for (var i = 0; i < tokens.Length - 1; i++)
        {
            if (!TryParseModifier(tokens[i], out var mask))
                return false;
            modifiers |= mask;
        }

        return TryParseKey(keyToken, out key);
    }

    private static bool TryParseModifier(string token, out EventMask mask)
    {
        mask = token.ToLowerInvariant() switch
        {
            "alt" or "option" => EventMask.Alt,
            "ctrl" or "control" => EventMask.Ctrl,
            "shift" => EventMask.Shift,
            "meta" or "win" or "super" or "cmd" or "command" => EventMask.Meta,
            _ => EventMask.None,
        };
        return mask != EventMask.None;
    }

    private static bool TryParseKey(string token, out KeyCode key)
    {
        // KeyCode members are prefixed with "Vc" (VcSpace, VcA, VcEnter, VcF1, Vc1, ...).
        var candidate = "Vc" + token.Trim();
        return Enum.TryParse(candidate, ignoreCase: true, out key) && key != KeyCode.VcUndefined;
    }

    /// <summary>
    /// True when the live event mask contains all required modifier flags and no extra
    /// alt/ctrl/shift/meta modifiers beyond what the hotkey requires.
    /// </summary>
    public static bool Matches(EventMask required, EventMask live)
    {
        foreach (var flag in new[] { EventMask.Alt, EventMask.Ctrl, EventMask.Shift, EventMask.Meta })
        {
            var wanted = (required & flag) != EventMask.None;
            var present = (live & flag) != EventMask.None;
            if (wanted != present)
                return false;
        }
        return true;
    }
}
