using Projektor.Core.Ports;
using SharpHook;

namespace Projektor.Infrastructure.Hotkey;

public sealed class SharpHookHotkeyService : IGlobalHotkeyService
{
    // Uses SimpleGlobalHook (not EventLoopGlobalHook) because SuppressEvent is
    // synchronous and only works on the hook thread with SimpleGlobalHook.
    private SimpleGlobalHook? _hook;

    public event EventHandler? HotkeyPressed;

    public void Register(string hotkey) => throw new NotImplementedException();

    public void Dispose()
    {
        _hook?.Dispose();
        _hook = null;
    }
}
