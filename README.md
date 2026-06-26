# Projektor

> Ein projekt-kontext-first Quick Launcher für Entwickler — per globalem Hotkey, überall verfügbar.

## Was ist Projektor?

Als Entwickler wechselst du ständig zwischen Projekten. Jedes Mal: Ordner suchen, IDE öffnen, Terminal aufmachen, `cd` rein, Command tippen. Immer dieselbe Reibung.

**Projektor** löst das mit einem einfachen Prinzip: Projekt auswählen → Actions starten — und zwar immer direkt im Working Directory dieses Projekts.

`Alt+Space` → Projekt tippen → Terminal / IDE / Build-Command starten. Fertig.

## Kern-Idee

- **Projekt** = ein Ordner im Dateisystem
- **Action** = IDE, Editor, Terminal oder beliebiger Shell-Command — immer im Projekt-Ordner ausgeführt
- **Launcher** = schlankes GUI-Overlay, per globalem Hotkey aufgerufen (Stil: Spotlight / Raycast / PowerToys Run, aber projekt-kontext-first)

## Tech Stack

| | |
|---|---|
| **Framework** | .NET (Core) |
| **UI** | Avalonia UI |
| **Globale Hotkeys** | SharpHook (libuiohook) |
| **Config** | TOML (Tomlyn) |
| **Plattform** | Windows + Linux |

## Plattform

Windows und Linux. macOS ist bewusst nicht im Scope von v1.

## Status

MVP-Increment (erster Vertical Slice) implementiert: Core-Logik, TOML-Config, Process-Launcher,
globaler Hotkey, Avalonia-Overlay und Tray sind end-to-end verdrahtet. Build & Tests grün auf
Windows + Linux. Hotkey und Overlay erfordern einen manuellen Smoke-Test auf echtem Desktop
(siehe unten).

## Starten

```bash
dotnet run --project src/Projektor.App   # benötigt .NET 10 SDK
dotnet test projektor.slnx               # Unit- & Integrationstests
```

`Alt+Space` öffnet das Overlay. Beim ersten Start wird eine Default-Config angelegt
(`~/.config/projektor/projektor.toml` bzw. `%APPDATA%\Projektor\projektor.toml`).

## Docs

- [PRD — Product Requirements Document](Docs/PRD.md)
- [Architecture](Docs/Architecture.md)
- [Architecture Decision Records](Docs/adr/README.md)
- [Testing](Docs/Testing.md)
- [Manueller Smoke-Test](Docs/SmokeTest.md)
