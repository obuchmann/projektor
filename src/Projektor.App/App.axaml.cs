using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Projektor.App.ViewModels;
using Projektor.App.Views;
using Projektor.Core.Ports;
using Projektor.Infrastructure.Config;

namespace Projektor.App;

public sealed partial class App : Application
{
    private ServiceProvider? _services;
    private TomlConfigStore? _store;
    private IGlobalHotkeyService? _hotkey;
    private OverlayWindow? _overlay;
    private TrayIcon? _tray;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Headless tray app: the overlay hides instead of closing, so the app only
            // exits when the user explicitly quits from the tray.
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.ShutdownRequested += (_, _) => Cleanup();

            Bootstrap();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Bootstrap()
    {
        _services = AppServices.Build();
        var logger = _services.GetRequiredService<ILogger<App>>();

        _store = _services.GetRequiredService<TomlConfigStore>();
        if (!_store.Exists)
        {
            _store.Save(DefaultConfig.Create());
            logger.LogInformation("Standard-Konfiguration nach {Path} geschrieben.", _store.FilePath);
        }

        var config = _store.Load();

        var vm = _services.GetRequiredService<OverlayViewModel>();
        vm.SetConfig(config);
        vm.LaunchRequested += (_, _) => _overlay?.Hide();

        _overlay = new OverlayWindow { DataContext = vm };

        // External edits to the TOML file are picked up live.
        _store.Changed += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            try { vm.SetConfig(_store.Load()); }
            catch (Exception ex) { logger.LogWarning(ex, "Config-Reload fehlgeschlagen."); }
        });

        _hotkey = _services.GetRequiredService<IGlobalHotkeyService>();
        _hotkey.HotkeyPressed += (_, _) => Dispatcher.UIThread.Post(() => _overlay?.ToggleOverlay());
        _hotkey.Register(config.Settings.Hotkey);
        logger.LogInformation("Projektor gestartet. Hotkey: {Hotkey}", config.Settings.Hotkey);

        SetupTray();
    }

    private void SetupTray()
    {
        var menu = new NativeMenu();

        var show = new NativeMenuItem("Anzeigen");
        show.Click += (_, _) => _overlay?.ShowOverlay();
        menu.Add(show);

        var edit = new NativeMenuItem("Config bearbeiten");
        edit.Click += (_, _) => OpenConfigFile();
        menu.Add(edit);

        menu.Add(new NativeMenuItemSeparator());

        var quit = new NativeMenuItem("Beenden");
        quit.Click += (_, _) => Quit();
        menu.Add(quit);

        _tray = new TrayIcon
        {
            ToolTipText = "Projektor",
            Menu = menu,
            Icon = LoadIcon(),
        };
        _tray.Clicked += (_, _) => _overlay?.ShowOverlay();

        TrayIcon.SetIcons(this, new TrayIcons { _tray });
    }

    private static WindowIcon? LoadIcon()
    {
        try
        {
            var uri = new Uri("avares://Projektor.App/Assets/projektor.ico");
            return new WindowIcon(AssetLoader.Open(uri));
        }
        catch
        {
            return null;
        }
    }

    private void OpenConfigFile()
    {
        if (_store is null)
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _store.FilePath,
                UseShellExecute = true,
            });
        }
        catch
        {
            // Opening the editor is best-effort; failure is non-fatal.
        }
    }

    private void Quit()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    private void Cleanup()
    {
        _tray?.Dispose();
        _hotkey?.Dispose();
        _store?.Dispose();
        _services?.Dispose();
    }
}
