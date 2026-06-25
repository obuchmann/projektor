namespace Projektor.Core.Models;

public sealed record AppSettings(
    string Hotkey = "Alt+Space",
    string Theme = "dark");
