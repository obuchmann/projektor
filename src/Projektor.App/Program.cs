using Avalonia;
using Projektor.App;

AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    .WithInterFont()
    .StartWithClassicDesktopLifetime(args);
