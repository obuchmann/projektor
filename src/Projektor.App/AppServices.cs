using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Projektor.App.ViewModels;
using Projektor.Core.Ports;
using Projektor.Core.Services;
using Projektor.Infrastructure.Autostart;
using Projektor.Infrastructure.Config;
using Projektor.Infrastructure.Hotkey;
using Projektor.Infrastructure.Process;
using Projektor.Infrastructure.Scanner;
using Serilog;

namespace Projektor.App;

/// <summary>
/// Composition root. Wires Core ports to their Infrastructure adapters using a plain
/// <see cref="ServiceCollection"/> (no Generic Host — see ADR-0007).
/// </summary>
internal static class AppServices
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();

        ConfigureLogging(services);

        // Domain services (pure).
        services.AddSingleton<ActionResolver>();
        services.AddSingleton<ProjectFilter>();

        // Ports → adapters.
        services.AddSingleton<TomlConfigStore>();
        services.AddSingleton<IConfigStore>(sp => sp.GetRequiredService<TomlConfigStore>());
        services.AddSingleton<IProcessLauncher, ProcessLauncher>();
        services.AddSingleton<IGlobalHotkeyService, SharpHookHotkeyService>();
        services.AddSingleton<IAutostartManager, AutostartManager>();
        services.AddSingleton<IProjectScanner, GitProjectScanner>();

        // View models.
        services.AddSingleton<OverlayViewModel>();

        return services.BuildServiceProvider();
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        var logDir = System.IO.Path.GetDirectoryName(ConfigPathResolver.Resolve())!;
        var logPath = System.IO.Path.Combine(logDir, "projektor.log");

        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();

        services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));
    }
}
