namespace Projektor.Core.Ports;

public interface IGlobalHotkeyService : IDisposable
{
    void Register(string hotkey);
    event EventHandler? HotkeyPressed;
}
