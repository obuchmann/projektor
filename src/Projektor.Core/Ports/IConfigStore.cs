using Projektor.Core.Models;

namespace Projektor.Core.Ports;

public interface IConfigStore
{
    ProjektorConfig Load();
    void Save(ProjektorConfig config);
    event EventHandler? Changed;
}
