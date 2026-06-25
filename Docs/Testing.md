# Projektor — Teststrategie

> Testing hat oberste Priorität. Jede Domänenlogik, die testbar ist, wird getestet — bevor der erste Build-Slice zusammengesteckt wird.

---

## 1. Prioritäten

**Testen, was wirklich zählt:** Die Domänenlogik in `Projektor.Core` ist der Kern des Produkts und hat keine UI-, OS- oder IO-Abhängigkeiten. Sie ist vollständig unit-testbar und soll es bleiben.

**Nicht testen:** UI-Rendering, Hotkey-Propagation durch SharpHook, das Betriebssystem-Verhalten von `Process.Start`. Diese liegen hinter Ports und werden nicht gemockt, sondern durch manuelle Smoke-Tests auf beiden Plattformen abgedeckt.

```
Hohe Priorität   │ Projektor.Core.Tests (Unit — xUnit)
                 │   ActionResolver, ProjectFilter, CommandSubstitution
─────────────────┼──────────────────────────────────────────────────
Mittlere Priorität│ TomlConfigStore-Roundtrips (Integrationstest, Dateisystem)
                 │   Config schreiben → laden → Modell vergleichen
─────────────────┼──────────────────────────────────────────────────
Manuell / OS     │ Hotkey, Process.Start, Wayland-Restriktion,
                 │ Alt+Space-Suppression auf Windows
```

---

## 2. Was wird getestet (Unit-Tests in `Projektor.Core.Tests`)

### ActionResolver

Die Kernregel: *effektive Actions = (globale Templates − deaktivierte) + projekt-spezifische Overrides/Zusätze.*

- Alle Templates sichtbar, wenn keine deaktiviert.
- Deaktivierte Templates werden korrekt ausgeblendet.
- Projekt-spezifische Action mit gleicher `Id` überschreibt das Template (Override).
- Projekt-spezifische Action mit neuer `Id` wird zusätzlich angehängt.
- Kombination aus Deaktivierung + Override + Extra funktioniert korrekt.
- Leeres Projekt (keine CustomActions, keine Deaktivierungen) → alle globalen Templates.

### ProjectFilter

- Exaktes Präfix-Match → Treffer.
- Substring-Match → Treffer.
- Case-insensitiv.
- Leere Eingabe → alle Projekte zurück.
- Keine Treffer → leere Liste (kein Crash).

### Command-Substitution

- `{path}` in Command wird durch Projektpfad ersetzt.
- Kein `{path}` → Command bleibt unverändert.
- Mehrfaches `{path}` → alle Vorkommen ersetzt.
- Pfad mit Leerzeichen wird korrekt übergeben.

### Config-Deserialisierung (TOML-Roundtrip, Integrationstest)

- Minimale Config (nur `[settings]`) lädt fehlerfrei.
- Volle Config mit Templates + Projekten + ScanRoots → Modell korrekt befüllt.
- Fehlende optionale Felder → Defaults greifen, kein Fehler.
- Geschriebene Config ergibt beim erneuten Laden dasselbe Modell.

---

## 3. Test-Setup

Tests laufen in `tests/Projektor.Core.Tests/` (xUnit). Keine externen Abhängigkeiten, kein Dateisystem außer bei expliziten TOML-Roundtrip-Tests (temporäres Verzeichnis via `Path.GetTempPath()`).

```
dotnet test tests/Projektor.Core.Tests/
```

CI-Ziel (wenn Pipeline aufgebaut): Tests müssen auf Windows und Linux grün sein.

---

## 4. Arbeiten mit Claude Code (claude.ai)

Projektor wird mit Claude Code auf claude.ai entwickelt. Damit Skills und Automationen korrekt funktionieren, hier der Überblick.

### Skills installieren

**Methode 1 — Repository (bevorzugt):**
Skills liegen unter `.claude/skills/<skill-name>/SKILL.md`. Claude Code erkennt sie automatisch ohne Session-Neustart (Live change detection).

**Methode 2 — Web-Upload:**
Settings → Skills → Upload (ZIP-Archiv des Skill-Ordners).

### Skills aktivieren

```
/skill-name          ← manuell per Slash-Command
```

Oder als Freitext-Prompt, wenn die Beschreibung eindeutig passt — Claude wählt den Skill autonom. Bei Unsicherheit: explizit benennen: *„Nutze den Skill [Name], um …"*

### Geplante Projekt-Skills

| Skill | Zweck |
|---|---|
| `test` | `dotnet test` ausführen, Ergebnisse zusammenfassen |
| `verify` | Overlay manuell prüfen (Hotkey, Launch, Esc) |
| `build` | Solution bauen, Warnungen prüfen |
