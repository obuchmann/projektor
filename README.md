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

## Builds & Releases

Jeder Push/PR baut und testet via CI auf Windows + Linux (siehe `.github/workflows/ci.yml`).

Ein Git-Tag `v*` löst einen Release-Build aus (`.github/workflows/release.yml`):

- Self-contained, Single-File-Builds für `win-x64` und `linux-x64`
- Paketiert als `projektor-<version>-win-x64.zip` und `projektor-<version>-linux-x64.tar.gz`
- Automatisch als GitHub Release mit den Builds als Assets veröffentlicht

Die Builds lassen sich auch ohne Release über `workflow_dispatch` erzeugen — sie liegen dann
als Workflow-Artifacts zum Download bereit.

### Automatische Versionierung

Bei jedem Merge/Push auf `main` ermittelt `.github/workflows/auto-version.yml` automatisch
die nächste Version und löst den Release-Build aus — manuelles Taggen ist nicht nötig.

Die Bump-Stufe folgt [Conventional Commits](https://www.conventionalcommits.org/) anhand der
Commits seit dem letzten stabilen Tag:

| Commit (seit letztem Tag) | Bump | Beispiel |
|---|---|---|
| `feat: …` | Minor | `0.1.3 → 0.2.0` |
| `fix:` / `refactor:` / sonstiges | Patch | `0.1.3 → 0.1.4` |
| `feat!:` / `BREAKING CHANGE` | Major* | `0.1.3 → 1.0.0` |

\* Solange die Version noch `0.x` ist, zählt ein Breaking Change als **Minor** (SemVer-Konvention
für Pre-1.0).

Den Versions-Tag erstellt anschließend `release.yml` am Merge-Commit. Da GitHub Workflows nicht
durch Tags startet, die mit dem Standard-`GITHUB_TOKEN` gepusht werden, ruft `auto-version.yml`
den Release direkt per `workflow_dispatch` auf — es ist **kein Secret nötig**.

Einen Merge ohne Release veröffentlichen: `[skip release]` in die Commit-Message aufnehmen.

Manuelles Taggen bleibt weiterhin möglich:

```bash
git tag v0.1.0
git push origin v0.1.0
```

## Docs

- [PRD — Product Requirements Document](Docs/PRD.md)
- [Architecture](Docs/Architecture.md)
- [Architecture Decision Records](Docs/adr/README.md)
- [Testing](Docs/Testing.md)
- [Manueller Smoke-Test](Docs/SmokeTest.md)
