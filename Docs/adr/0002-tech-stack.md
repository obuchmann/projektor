# 2. Tech-Stack (.NET 10, Avalonia 12, SharpHook, Tomlyn)

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Projektor ist eine Cross-Platform-Desktop-App (Windows + Linux) mit globalem Hotkey,
GUI-Overlay und TOML-Konfiguration. macOS ist out of scope für v1.

## Entscheidung

| Baustein | Wahl | Version |
|---|---|---|
| Runtime | .NET | 10 (LTS) |
| UI | Avalonia UI | 12.0.x |
| MVVM | CommunityToolkit.Mvvm | 8.4.x (Source-Generator) |
| Globale Hotkeys | SharpHook (libuiohook) | 7.1.x |
| Config | Tomlyn | 2.3.x |
| DI | Microsoft.Extensions.DependencyInjection | 10.x |
| Logging | Microsoft.Extensions.Logging + Serilog File-Sink | aktuell |
| Tests | xUnit v3 | 3.2.x |
| Solution | `.slnx` (XML) | — |

Versionen werden zentral in `Directory.Packages.props` gepinnt (Central Package Management).
Build-weite Settings (`net10.0`, `Nullable`, `TreatWarningsAsErrors`) in `Directory.Build.props`.

## Konsequenzen

- Ein C#-Codepfad für beide OS; OS-Unterschiede über Runtime-Checks/Adapter.
- `TreatWarningsAsErrors` + `EnforceCodeStyleInBuild` halten den Code sauber, erzwingen aber
  z.B. Plattform-Guards (`OperatingSystem.IsWindows()`) gegen CA1416.
- Bindung an Avalonia 12 / .NET 10 (LTS) als langfristige Basis.

## Hinweise zur Umsetzung

- Tomlyn 2.3.2 nutzt eine neue, `System.Text.Json`-ähnliche API (`TomlSerializer`,
  `TomlSerializerOptions`) statt `Toml.ToModel`/`FromModel` — siehe ADR-0010.
- SharpHook 7: `SimpleGlobalHook` + `EventMask`/`KeyCode` aus `SharpHook.Data` — siehe ADR-0004.
- `Microsoft.Win32.Registry` ist in .NET 10 Teil des Frameworks; kein Extra-Paket nötig
  (NU1510) — siehe ADR-0009.
