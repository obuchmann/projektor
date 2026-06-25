using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Hotkey;

public sealed class SharpHookHotkeyService : IGlobalHotkeyService
{
    // CS0067 suppressed via explicit add/remove — event will be fired from SimpleGlobalHook handler in implementation
    public event EventHandler? HotkeyPressed { add { } remove { } }

    public void Register(string hotkey) => throw new NotImplementedException();

    public void Dispose() { }
}
