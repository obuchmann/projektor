# 10. Eigener TOML-Writer für Block-Style-Output

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Die Config soll git-diffbar und von Hand editierbar sein (ADR-0005). Tomlyn 2.3.2 bietet eine
neue, `System.Text.Json`-ähnliche Serializer-API. Deren High-Level-Pfad emittiert Arrays von
Tabellen jedoch **immer als Inline-Tabellen** auf einer einzigen Zeile —
`action_templates = [{id = "…", …}, {…}]`. Die Optionen `TableArrayStyle.Headers` und
`InlineTablePolicy.Never` ändern das im Reflection-Pfad nicht (verifiziert).

Eine wachsende Projekt-/Template-Liste landet so auf einer einzigen, ständig länger werdenden
Zeile — schlecht diffbar und unangenehm von Hand zu editieren.

## Entscheidung

- **Laden:** weiterhin über Tomlyn (`TomlSerializer.Deserialize<ConfigDto>`) — robustes Parsing,
  akzeptiert sowohl Block- als auch Inline-Form.
- **Schreiben:** über einen kleinen, schemaspezifischen `TomlConfigWriter`, der Header-Blöcke
  erzeugt (`[settings]`, `[[action_templates]]`, `[[projects]]`, `[[projects.actions]]`,
  `[[scan_roots]]`) mit einfachem String-Escaping.

## Konsequenzen

- Gespeicherte Configs sind sauber strukturiert und gut diffbar; jedes Feld auf eigener Zeile.
- Roundtrip (schreiben → laden → Modell) ist getestet, inkl. Disabled-Templates und
  Custom-Actions; ein Test fixiert den Block-Style (kein Inline-`[{`).
- Kleiner eigener Writer als Wartungsfläche. Sollte Tomlyn später Block-Style unterstützen oder
  Kommentar-Erhaltung wichtig werden, kann der Writer ersetzt werden.
