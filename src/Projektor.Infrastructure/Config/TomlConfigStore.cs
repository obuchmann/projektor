using Projektor.Core.Models;
using Projektor.Core.Ports;

namespace Projektor.Infrastructure.Config;

public sealed class TomlConfigStore : IConfigStore, IDisposable
{
    private readonly string _path;
    private FileSystemWatcher? _watcher;

    public event EventHandler? Changed;

    public TomlConfigStore() : this(ConfigPathResolver.Resolve()) { }

    public TomlConfigStore(string path)
    {
        _path = path;
    }

    public ProjektorConfig Load() => throw new NotImplementedException();

    public void Save(ProjektorConfig config) => throw new NotImplementedException();

    public void Dispose() => _watcher?.Dispose();
}
