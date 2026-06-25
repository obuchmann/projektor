using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Autostart;

public sealed class AutostartManager : IAutostartManager
{
    private readonly IAutostartManager _impl = OperatingSystem.IsWindows()
        ? new WindowsAutostartManager()
        : new LinuxAutostartManager();

    public void Enable() => _impl.Enable();
    public void Disable() => _impl.Disable();
    public bool IsEnabled() => _impl.IsEnabled();
}
