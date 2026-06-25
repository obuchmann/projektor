namespace Projektor.Core.Ports;

public interface IAutostartManager
{
    void Enable();
    void Disable();
    bool IsEnabled();
}
