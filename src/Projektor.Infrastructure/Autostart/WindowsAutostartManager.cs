using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Autostart;

internal sealed class WindowsAutostartManager : IAutostartManager
{
    // .lnk in Environment.SpecialFolder.Startup
    public void Enable() => throw new NotImplementedException();
    public void Disable() => throw new NotImplementedException();
    public bool IsEnabled() => throw new NotImplementedException();
}
