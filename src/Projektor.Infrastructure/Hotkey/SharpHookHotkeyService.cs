using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Projektor.Core.Ports;
using SharpHook;
using SharpHook.Data;

namespace Projektor.Infrastructure.Hotkey;

/// <summary>
/// SharpHook-based global hotkey. Uses <see cref="SimpleGlobalHook"/> so handlers run on the
/// hook thread, which is required for synchronous event suppression on Windows. The hook loop
/// runs on a background thread; the matched hotkey is reported via <see cref="HotkeyPressed"/>.
/// </summary>
/// <remarks>
/// Suppression of the triggering key press only works on Windows/macOS — on Linux the key press
/// leaks to the focused app (see ADR-0004). Under Wayland the user must be in the <c>input</c>
/// group for the hook to receive events at all.
/// </remarks>
public sealed class SharpHookHotkeyService : IGlobalHotkeyService
{
    private readonly ILogger<SharpHookHotkeyService> _logger;
    private readonly object _gate = new();

    private SimpleGlobalHook? _hook;
    private EventMask _modifiers = EventMask.None;
    private KeyCode _key = KeyCode.VcUndefined;
    private bool _registered;

    public event EventHandler? HotkeyPressed;

    public SharpHookHotkeyService(ILogger<SharpHookHotkeyService>? logger = null)
    {
        _logger = logger ?? NullLogger<SharpHookHotkeyService>.Instance;
    }

    public void Register(string hotkey)
    {
        if (!HotkeyParser.TryParse(hotkey, out var modifiers, out var key))
        {
            _logger.LogWarning("Hotkey '{Hotkey}' konnte nicht geparst werden — Hotkey inaktiv.", hotkey);
            return;
        }

        lock (_gate)
        {
            _modifiers = modifiers;
            _key = key;

            if (_hook is not null)
                return; // Already running; new key/modifiers take effect immediately.

            _hook = new SimpleGlobalHook(GlobalHookType.Keyboard);
            _hook.KeyPressed += OnKeyPressed;
            _hook.RunAsync();
            _registered = true;
            _logger.LogInformation("Globaler Hotkey '{Hotkey}' registriert.", hotkey);
        }
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (!_registered || e.Data.KeyCode != _key)
            return;

        if (!HotkeyParser.Matches(_modifiers, e.RawEvent.Mask))
            return;

        // Suppress the system handling of the key on platforms that support it (Windows/macOS).
        // This prevents e.g. the Alt+Space window system menu on Windows.
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
            e.SuppressEvent = true;

        HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_hook is not null)
            {
                _hook.KeyPressed -= OnKeyPressed;
                _hook.Dispose();
                _hook = null;
            }
            _registered = false;
        }
    }
}
