using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Autostart;

internal sealed class LinuxAutostartManager : IAutostartManager
{
    // ~/.config/autostart/projektor.desktop
    public void Enable() => throw new NotImplementedException();
    public void Disable() => throw new NotImplementedException();
    public bool IsEnabled() => throw new NotImplementedException();
}
