# 1. Hexagonale Architektur (Ports & Adapters)

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Projektor hat einen kleinen, klar abgrenzbaren Domänenkern (Projekt + effektive Actions +
Filter) und drumherum viel Plattform-/IO-Abhängiges: globale Hotkeys, Prozess-Start, TOML-Datei,
Autostart, Git-Scan. Diese OS-/IO-Teile sind schwer automatisiert testbar und unterscheiden sich
zwischen Windows und Linux.

## Entscheidung

Wir verwenden Ports & Adapters (Hexagonal):

- `Projektor.Core` enthält die Domäne (Modelle als `record`, `ActionResolver`, `ProjectFilter`)
  und definiert **Ports** (Interfaces): `IConfigStore`, `IProcessLauncher`,
  `IGlobalHotkeyService`, `IAutostartManager`, `IProjectScanner`. Core kennt niemanden.
- `Projektor.Infrastructure` implementiert die Ports als **Adapter** (Tomlyn, `Process`,
  SharpHook, Registry/`.desktop`, Git-Scan) und kennt nur Core.
- `Projektor.App` ist die einzige Composition Root, kennt beide und verdrahtet Ports↔Adapter.

Abhängigkeitsrichtung strikt nach innen: `App → Infrastructure → Core`.

## Konsequenzen

- Der wertvolle, plattformunabhängige Kern ist vollständig unit-testbar (siehe Teststrategie).
- OS-Weichen leben hinter genau einem Interface, nie verstreut im UI-Code.
- Adapter sind austauschbar (z.B. anderer Hotkey-Provider) ohne Core-Änderung.
- Etwas mehr Boilerplate (Interfaces + DI-Verdrahtung) — bewusst akzeptiert.

## Alternativen

- **Monolithische App ohne Schichten**: schneller initial, aber Domänenlogik nicht isoliert
  testbar und OS-Code verstreut. Verworfen.
