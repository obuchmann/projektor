using Projektor.Core.Models;
using Projektor.Core.Ports;
using Tomlyn;

namespace Projektor.Infrastructure.Config;


/// <summary>
/// TOML-backed <see cref="IConfigStore"/>. Maps the on-disk snake_case TOML schema
/// (see Docs/Architecture.md §7) to the immutable Core records via plain DTOs and
/// raises <see cref="Changed"/> when the file is edited externally.
/// </summary>
public sealed class TomlConfigStore : IConfigStore, IDisposable
{
    private readonly string _path;
    private readonly FileSystemWatcher? _watcher;
    private static readonly TomlSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = TomlIgnoreCondition.WhenWritingNull,
    };

    public event EventHandler? Changed;

    /// <summary>The resolved path of the TOML config file backing this store.</summary>
    public string FilePath => _path;

    /// <summary>True when the backing config file already exists on disk.</summary>
    public bool Exists => File.Exists(_path);

    public TomlConfigStore() : this(ConfigPathResolver.Resolve()) { }

    public TomlConfigStore(string path)
    {
        _path = path;

        var dir = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            _watcher = new FileSystemWatcher(dir, Path.GetFileName(_path))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                EnableRaisingEvents = true,
            };
            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += OnFileChanged;
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) =>
        Changed?.Invoke(this, EventArgs.Empty);

    public ProjektorConfig Load()
    {
        if (!File.Exists(_path))
            return ProjektorConfig.Empty;

        var text = File.ReadAllText(_path);
        var dto = TomlSerializer.Deserialize<ConfigDto>(text, Options) ?? new ConfigDto();
        return dto.ToModel();
    }

    public void Save(ProjektorConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var dir = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var text = TomlConfigWriter.Write(config);

        // Avoid the watcher firing on our own write.
        var hadWatcher = _watcher is { EnableRaisingEvents: true };
        if (_watcher is not null)
            _watcher.EnableRaisingEvents = false;
        try
        {
            File.WriteAllText(_path, text);
        }
        finally
        {
            if (_watcher is not null && hadWatcher)
                _watcher.EnableRaisingEvents = true;
        }
    }

    public void Dispose()
    {
        if (_watcher is not null)
        {
            _watcher.Changed -= OnFileChanged;
            _watcher.Created -= OnFileChanged;
            _watcher.Renamed -= OnFileChanged;
            _watcher.Dispose();
        }
    }
}
