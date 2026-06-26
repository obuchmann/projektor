# Projektor — Manueller Smoke-Test

Was automatisierte Tests **nicht** abdecken (globaler Tastendruck, Overlay-Rendering, realer
Prozess-Start, OS-Eigenheiten), wird hier manuell auf einem echten Desktop geprüft. Hintergrund
und Scope: [ADR-0006](adr/0006-mvp-vertical-slice-scope.md).

## Voraussetzungen

- .NET 10 SDK
- Linux: X11 (unter Wayland muss der User in der Gruppe `input` sein — siehe
  [ADR-0004](adr/0004-global-hotkey-suppression.md))

## Starten

```bash
dotnet run --project src/Projektor.App
```

Beim ersten Start wird eine Default-Config geschrieben:
- Linux: `~/.config/projektor/projektor.toml`
- Windows: `%APPDATA%\Projektor\projektor.toml`

Log: `projektor.log` im selben Ordner.

## Checkliste

| # | Schritt | Erwartung |
|---|---|---|
| 1 | App starten | Kein Fenster sichtbar; Tray-Icon erscheint; Log: "Projektor gestartet" |
| 2 | `Alt+Space` drücken | Overlay erscheint zentriert, **Suchfeld hat Tastaturfokus** |
| 3 | (Windows) Schritt 2 | **Kein** Fenster-Systemmenü poppt auf (Suppression greift) |
| 4 | Projektnamen tippen | Projektliste filtert live (case-insensitive Contains) |
| 5 | Projekt auswählen | Rechts erscheinen die nummerierten Actions (Alt+1, Alt+2, …) |
| 6 | **Klick** auf eine Action | Action startet im Projekt-Ordner; Overlay verschwindet |
| 7 | `Alt+Space` → **Alt+2** | Tool 2 des Projekts startet |
| 8 | `Alt+Space` → **Enter** | Tool 1 startet |
| 9 | `Alt+Space` → **Shift+Enter** | **alle** Tools des Projekts starten |
| 10 | `Alt+Space`, dann `Esc` | Overlay öffnet und schließt wieder |
| 11 | Overlay offen, woanders klicken | Overlay verschwindet (Fokusverlust) |
| 12 | TOML extern editieren (vim) | Änderung wird ohne Neustart übernommen (FileSystemWatcher) |
| 13 | Tray → "Config bearbeiten" | TOML öffnet im Default-Editor |
| 14 | Tray → "Beenden" | Prozess endet sauber, Tray-Icon verschwindet |

> Launch-Modell und Windows-Fokus: siehe [ADR-0011](adr/0011-launch-interaction-model.md).

## Fehlerfälle

- Hotkey reagiert nicht (Linux/Wayland): User in Gruppe `input`? (`sudo usermod -aG input $USER`,
  neu einloggen).
- Action startet nicht (Exe nicht im `PATH`): Overlay zeigt eine Fehlermeldung; Command in der
  TOML prüfen.
