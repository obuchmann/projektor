# Architecture Decision Records

Diese ADRs halten die Architektur- und annahmenbasierten Entscheidungen von Projektor fest,
im [MADR](https://adr.github.io/madr/)-Format. Eine Entscheidung wird hier dokumentiert, wenn
sie die Struktur prägt oder auf einer Annahme beruht, die später überprüft werden könnte.

| Nr. | Titel | Status |
|---|---|---|
| [0001](0001-hexagonal-ports-and-adapters.md) | Hexagonale Architektur (Ports & Adapters) | Accepted |
| [0002](0002-tech-stack.md) | Tech-Stack (.NET 10, Avalonia 12, SharpHook, Tomlyn) | Accepted |
| [0003](0003-command-model.md) | Command-Modell: pro-OS-Commands, `{path}`, Shell-Wrapper | Accepted |
| [0004](0004-global-hotkey-suppression.md) | Globaler Hotkey, Suppression & Wayland-Risiko | Accepted |
| [0005](0005-config-toml.md) | Konfiguration: TOML, Ablageort, Portable-Modus | Accepted |
| [0006](0006-mvp-vertical-slice-scope.md) | MVP-Scope: voller Vertical Slice, GUI/Hotkey nur manuell verifizierbar | Accepted |
| [0007](0007-di-servicecollection-only.md) | DI nur via ServiceCollection (kein Generic Host) | Accepted |
| [0008](0008-per-project-actions-single-command.md) | Pro-Projekt-Actions sind single-command in v1 | Accepted |
| [0009](0009-windows-autostart-registry.md) | Windows-Autostart via Registry-Run-Key statt `.lnk` | Accepted |
| [0010](0010-toml-block-style-writer.md) | Eigener TOML-Writer für Block-Style-Output | Accepted |
| [0011](0011-launch-interaction-model.md) | Launch-Interaktionsmodell & Windows-Fokus | Accepted |

## Template

Neue ADRs folgen dem Muster: **Kontext** → **Entscheidung** → **Konsequenzen** (+ ggf. **Alternativen**).
Dateiname: `NNNN-kurz-titel.md`, fortlaufend nummeriert. ADRs werden nicht gelöscht, sondern bei
Bedarf durch einen neuen ADR ersetzt (Status `Superseded by NNNN`).
