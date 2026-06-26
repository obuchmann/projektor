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
| 2 | `Alt+Space` drücken | Overlay erscheint zentriert, Suchfeld hat Fokus |
| 3 | (Windows) Schritt 2 | **Kein** Fenster-Systemmenü poppt auf (Suppression greift) |
| 4 | Projektnamen tippen | Projektliste filtert live (case-insensitive Contains) |
| 5 | Projekt auswählen | Rechts erscheinen die effektiven Actions des Projekts |
| 6 | Action wählen + `Enter` | Action startet im Projekt-Ordner (z.B. Terminal/Editor öffnet) |
| 7 | Nach Launch | Overlay verschwindet |
| 8 | `Alt+Space`, dann `Esc` | Overlay öffnet und schließt wieder |
| 9 | Overlay offen, woanders klicken | Overlay verschwindet (Fokusverlust) |
| 10 | TOML extern editieren (vim) | Änderung wird ohne Neustart übernommen (FileSystemWatcher) |
| 11 | Tray → "Config bearbeiten" | TOML öffnet im Default-Editor |
| 12 | Tray → "Beenden" | Prozess endet sauber, Tray-Icon verschwindet |

## Fehlerfälle

- Hotkey reagiert nicht (Linux/Wayland): User in Gruppe `input`? (`sudo usermod -aG input $USER`,
  neu einloggen).
- Action startet nicht (Exe nicht im `PATH`): Overlay zeigt eine Fehlermeldung; Command in der
  TOML prüfen.
