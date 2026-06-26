# 8. Pro-Projekt-Actions sind single-command in v1

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Globale `ActionTemplate`s tragen getrennte `command_windows`/`command_linux` (ADR-0003).
Das bestehende Core-Modell repräsentiert eine projekt-spezifische Action als `ProjectAction`
mit **einem** bereits aufgelösten `Command` (Id, Name, Command). Die ursprüngliche
Architektur-Skizze (`Docs/Architecture.md §7`) zeigte für `[[projects.actions]]` dagegen
`command_windows`/`command_linux` — ein Widerspruch im Skeleton.

`Project.CustomActions` als `IReadOnlyList<ProjectAction>` und die bestehenden Core-Tests fixieren
das single-command-Modell.

## Entscheidung

Pro-Projekt-Actions sind in v1 **single-command**. In der TOML hat `[[projects.actions]]` daher
genau ein Feld `command` (nicht `command_windows`/`command_linux`). Beim Resolve wird nur `{path}`
substituiert (keine OS-Auswahl), da der gespeicherte Command bereits der auszuführende ist.

## Konsequenzen

- Sauberer, verlustfreier TOML-Roundtrip für Pro-Projekt-Actions (getestet).
- Passend zum Single-User-Scope des MVP (ein User, meist ein OS-Workflow pro Projekt).
- Eine Pro-Projekt-Action, die auf Windows und Linux unterschiedliche Commands braucht, ist in
  v1 nicht ausdrückbar.

## Annahme / Folge-PR

- Cross-Platform-Commands für Pro-Projekt-Actions (analog zu Templates) sind ein bewusster
  Folge-PR. Er würde `ProjectAction` bzw. ein neues Config-Modell erweitern und sowohl Core-Tests
  als auch den TOML-Writer/Reader anpassen.
