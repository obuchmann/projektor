# 3. Command-Modell: pro-OS-Commands, `{path}`, Shell-Wrapper

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Eine Action soll auf Windows und Linux unterschiedliche Executables/Commands ausführen können
(z.B. `wt` vs. `x-terminal-emulator`, `rider64` vs. `rider`). Sowohl GUI-Apps (Ordner als
Argument öffnen) als auch CLI-Tools (im Working Directory laufen) müssen unterstützt werden.

## Entscheidung

- `ActionTemplate` trägt **separate** `command_windows` und `command_linux`. Ein leeres Feld
  bedeutet: Action auf dieser Plattform nicht verfügbar (`ActionResolver` überspringt sie).
- Optionaler Platzhalter **`{path}`** wird zur Laufzeit durch den absoluten Projektpfad ersetzt
  (`ActionResolver.SubstituteCommand`, alle Vorkommen).
- Working Directory ist **immer** der Projektpfad — für CLI-Tools die Kernfunktion, GUI-Apps
  schadet es nicht.
- Ausführung **einheitlich über den Plattform-Shell-Wrapper**: Windows `cmd.exe /c "<cmd>"`,
  Linux `/bin/bash -c "<cmd>"`. Das vermeidet eine `launchKind`-Unterscheidung; Shell findet
  Programme im `PATH`, startet GUI-Apps korrekt und beherrscht `&&`/Pipes.
- `UseShellExecute = false`, `CreateNoWindow = true` (kein aufblitzendes Konsolenfenster).

## Konsequenzen

- `{path}`-Substitution ist reine Domänenlogik in Core, voll unit-getestet.
- Tilde-Expansion (`~`) der Projektpfade passiert am App-/Infrastructure-Rand (`PathUtil`),
  damit Core pfad-/OS-frei bleibt.
- Fehlschlägt `Process.Start` (Exe nicht gefunden o.ä.), wirft der Launcher eine
  `ProcessLaunchException`; die UI zeigt die Meldung im Overlay.

## Annahme / offen

- Komplexes Argument-Quoting (z.B. Pfade mit `"` innerhalb des Commands) ist in v1 simpel
  gehalten. Robustere Quoting-Strategie ggf. später.
