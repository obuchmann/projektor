# 6. MVP-Scope: voller Vertical Slice, GUI/Hotkey nur manuell verifizierbar

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Ziel dieses Increments ist ein lauffähiger MVP — der "Erste Build-Slice" aus dem PRD:
Overlay per globalem Hotkey öffnen, Projekte tippen/filtern, eine Action im Working Directory
starten, `Esc`/Fokusverlust schließt. Die Implementierung entstand autonom; offene Punkte werden
wo möglich auf spätere PRs verschoben.

## Entscheidung

Der Increment liefert den **vollständigen Vertical Slice end-to-end**:

- Core-Logik (`ActionResolver`, `ProjectFilter`, `{path}`-Substitution) — voll getestet.
- `TomlConfigStore` (Load/Save/Watcher) + Default-Seeding — getestet.
- `ProcessLauncher` (OS-Shell-Wrapper) — Argumentbau getestet, Start manuell.
- `SharpHookHotkeyService` + `HotkeyParser` — Parser getestet, Hook manuell.
- Avalonia-Overlay (frameless, topmost, zentriert), Tray-Icon, DI-Composition-Root, Logging.

**Verifikation:** Build grün auf Windows + Linux (CI); Unit-/Integrationstests grün. Der
Bootstrap (DI, Config-Seeding, TOML-Roundtrip, Logging, Hotkey-Registrierung, Tray/Overlay) wurde
zusätzlich headless unter `Xvfb` erfolgreich hochgefahren.

## Konsequenzen

- Was sich **nicht** automatisiert verifizieren lässt, bleibt **manueller Smoke-Test** auf
  echtem Desktop (so auch in `Docs/Testing.md` vorgesehen): tatsächlicher globaler Tastendruck,
  Alt+Space-Suppression auf Windows, sichtbares Overlay-Rendering, realer `Process.Start`,
  Wayland-`input`-Gruppe. Siehe `Docs/SmokeTest.md`.
- CI baut die App, führt sie aber nicht aus (kein Display, kein `/dev/input`).

## Bewusst auf spätere PRs verschoben

- Settings-UI zum Verwalten von Projekten/Templates/ScanRoots (v1: TOML von Hand editieren,
  Live-Reload vorhanden).
- Scanner- und Autostart-Bedienung in der UI (Adapter sind implementiert, aber noch nicht
  verdrahtet).
- Fuzzy-Search, "zuletzt/häufig benutzt", Favoriten, Projekt-Metadaten.
- Wayland-Erstart-Check mit Anleitung; plattformabhängiger Default-Hotkey.
