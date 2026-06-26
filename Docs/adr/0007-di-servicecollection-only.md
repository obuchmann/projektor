# 7. DI nur via ServiceCollection (kein Generic Host)

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Die App braucht eine Composition Root, um Ports↔Adapter zu verdrahten. Avalonia hat keinen
eingebauten DI-Container. Der .NET Generic Host (`IHost`/`IHostedService`) bietet Lifecycle-
und Hosting-Features, die Projektor aktuell nicht benötigt.

## Entscheidung

Wir verwenden ausschließlich `Microsoft.Extensions.DependencyInjection.ServiceCollection`
(`AppServices.Build()` in `Projektor.App`). Kein Generic Host, kein `IHostedService`.

- Domänenservices und Adapter werden als Singletons registriert.
- `TomlConfigStore` wird als konkreter Typ registriert (App nutzt `FilePath`/`Exists` für
  Seeding) und zusätzlich als `IConfigStore` exponiert.
- Logging via `AddLogging` + Serilog-File-Sink.

## Konsequenzen

- Minimaler Overhead, einfache und nachvollziehbare Startsequenz.
- App-Lifecycle (Tray, Hotkey-Dispose) wird manuell in `App` über `ShutdownRequested` gehandhabt.
- Sollte später echtes Hosting nötig werden (Background-Services, Health-Checks), kann auf den
  Generic Host migriert werden.
