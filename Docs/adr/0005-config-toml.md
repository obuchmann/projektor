# 5. Konfiguration: TOML, Ablageort, Portable-Modus

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Der Laufzeitzustand (Projekte, Templates, ScanRoots, Settings) soll als Single Source of Truth
git-diffbar und von Hand editierbar sein — kein verstecktes Binärformat.

## Entscheidung

- Format: **TOML** (gelesen via Tomlyn, geschrieben via eigenem Block-Style-Writer, siehe
  ADR-0010). Schema in `Docs/Architecture.md §7`.
- **Pfad-Resolver** (Reihenfolge):
  1. Portable: existiert `projektor.toml` neben der Executable → diese nutzen.
  2. Sonst: Linux `~/.config/projektor/projektor.toml`, Windows
     `%APPDATA%\Projektor\projektor.toml`.
- **Erststart:** existiert keine Datei → Default-Config (`DefaultConfig.Create()`) mit
  Beispiel-Templates ("Terminal hier", "VS Code", "Dateimanager") und einem Home-Projekt
  schreiben.
- **Live-Reload:** `FileSystemWatcher` → externe Edits (vim) lösen `Changed` aus; die App lädt
  neu auf dem UI-Thread. Eigene Schreibvorgänge unterdrücken den Watcher kurzzeitig.
- TOML-Keys sind snake_case und in den DTOs explizit via `[TomlPropertyName]` gepinnt, damit das
  Dateiformat unabhängig von Tomlyns Default-Namenskonvention stabil bleibt.

## Konsequenzen

- Config ist editierbar und versionierbar; Änderungen greifen ohne Neustart.
- Das Log liegt neben der Config (`projektor.log`, täglich rollierend, 7 Tage).

## Annahme

- Kommentar-Erhaltung bei UI-getriebenen Schreibvorgängen ist v1 nicht garantiert (der
  Block-Writer erzeugt die Datei neu). Akzeptabel, da Schreiben aktuell selten passiert.
