# Projektor — Architektur

> Technische Planung zur [PRD](PRD.md). Stand: 2026-06. Status: **Entwurf — in Refinement.**
> Noch kein Code; dieses Dokument legt Struktur, Schichten und die kniffligen technischen Pfade fest.

---

## 1. Leitprinzipien

1. **Projekt-Kontext-first** — der Domänenkern (Projekt + effektive Actions) ist der Mittelpunkt; UI und OS sind austauschbares Drumherum.
2. **Ports & Adapters (Hexagonal)** — `Projektor.Core` definiert Interfaces (Ports) für alles Plattform-/IO-Abhängige. Konkrete Implementierungen (SharpHook, Process, TOML-Datei, Autostart) sind Adapter und am Rand austauschbar/testbar.
3. **Plattform-Unterschiede an genau einer Stelle** — jede OS-Weiche (Command-Shell, Autostart, Hotkey-Suppression) lebt hinter einem Interface, nie verstreut im UI-Code.
4. **Config als Single Source of Truth** — der Laufzeitzustand wird aus TOML aufgebaut; kein verstecktes Binärformat. Git-diffbar, von Hand editierbar.
5. **Schlank starten** — minimale Dependencies, kein Over-Engineering. Reactive/Rx, DI-Hosting etc. nur wo sie echten Nutzen bringen.

---

## 2. Tech-Stack (gepinnt)

| Baustein | Wahl | Version (Stand 2026-06) | Notiz |
|---|---|---|---|
| Runtime | .NET | **10 (LTS)** | Min-Anforderung für Avalonia 12 |
| UI | **Avalonia UI** | **12.0.x** | Cross-Platform Desktop, XAML + MVVM |
| MVVM | **CommunityToolkit.Mvvm** | aktuell | Source-Generator-basiert, leichtgewichtig (Alternative: ReactiveUI — nur falls Rx-Bedarf wächst) |
| Globale Hotkeys | **SharpHook** | **7.1.x** | libuiohook-Binding; siehe §6 für Suppression-Caveats |
| Config | **Tomlyn** | aktuell | TOML lesen/schreiben, kommentar-erhaltend |
| DI | **Microsoft.Extensions.DependencyInjection** | 10.x | Composition Root in `App` |
| Logging | **Microsoft.Extensions.Logging** + Serilog-Sink | aktuell | Datei-Log neben Config |
| Tests | **xUnit** | aktuell | Fokus auf `Core` |
| Solution-Format | **`.slnx`** | — | XML-Solution, von `dotnet`/MSBuild nativ unterstützt |

---

## 3. Solution-Layout (`.slnx`)

```
projektor.slnx
├── src/
│   ├── Projektor.Core/          → Domäne + Ports (Interfaces). Keine UI, kein OS, keine IO-Impl.
│   ├── Projektor.Infrastructure/→ Adapter: TOML-Config, Process-Launcher, Autostart, Hotkey.
│   └── Projektor.App/           → Avalonia-App: Views, ViewModels, Bootstrap/DI, Tray, Window-Mgmt.
└── tests/
    └── Projektor.Core.Tests/    → Unit-Tests der Domänenlogik (effektive Actions, Filter).
```

**Abhängigkeitsrichtung (strikt, nur nach innen):**

```
   App ──► Infrastructure ──► Core
    └──────────────────────────►┘
```

- `Core` kennt niemanden.
- `Infrastructure` kennt nur `Core` (implementiert dessen Ports).
- `App` kennt beide und ist die einzige Composition Root (verdrahtet Ports↔Adapter via DI).

> Bewusste Vereinfachung: `Configuration` und `Platform` sind **nicht** getrennt, sondern liegen zusammen in `Infrastructure`. Erst aufsplitten, wenn es wirklich weh tut.

---

## 4. Schichten & Verantwortlichkeiten

### 4.1 `Projektor.Core` (Domäne + Ports)

**Modelle** (rein, immutable wo möglich — `record`):

- `ActionTemplate` — `Id`, `Name`, `Command`, `Args?`, `Icon?`. Global gültig.
- `Project` — `Name`, `Path`, `Source` (`Manual` | `Scanned`), `DisabledTemplateIds`, `CustomActions`.
- `ProjectAction` — eine konkret ausführbare Action (aus Template oder projekt-spezifisch).
- `ScanRoot` — Root-Pfad für `.git`-Auto-Discovery.
- `ProjektorConfig` — Aggregat: `Projects`, `ActionTemplates`, `ScanRoots`, `Settings` (Hotkey, Theme, …).

**Services / Logik:**

- `ActionResolver` — berechnet **effektive Actions** eines Projekts:
  `(globale Templates − deaktivierte) + projekt-spezifische Overrides/Zusätze`. Override-Regel (Match per `Id`) hier zentral und getestet.
- `ProjectFilter` — Tipp-Eingabe → gefilterte/gerankte Projektliste (v1: simples `Contains`/Prefix; Fuzzy ist Backlog hinter gleichem Interface).

**Ports (Interfaces, von Infrastructure implementiert):**

```csharp
public interface IConfigStore        // Load(), Save(config), Changed-Event (FileWatcher)
public interface IProcessLauncher    // Launch(ProjectAction, workingDirectory)
public interface IGlobalHotkeyService// Register(hotkey), HotkeyPressed-Event, Suppress-Verhalten
public interface IAutostartManager    // Enable()/Disable()/IsEnabled
public interface IProjectScanner     // Scan(ScanRoot) → erkannte Projekte (.git)
```

### 4.2 `Projektor.Infrastructure` (Adapter)

- `TomlConfigStore : IConfigStore` — Tomlyn-Mapping Domäne↔TOML, Pfad-Resolver (siehe §7), `FileSystemWatcher` für externe Edits.
- `ProcessLauncher : IProcessLauncher` — OS-Weiche für Command-Ausführung (siehe §5).
- `SharpHookHotkeyService : IGlobalHotkeyService` — SharpHook-Wrapper (siehe §6).
- `AutostartManager` — plattformspezifisch: `.desktop` (Linux) / `.lnk` (Windows) (siehe §8).
- `GitProjectScanner : IProjectScanner` — durchsucht ScanRoot nach `.git`-Ordnern.

### 4.3 `Projektor.App` (UI + Composition Root)

- **Bootstrap** — `Program.Main` → Avalonia AppBuilder; DI-Container baut alle Services, lädt Config, startet Hotkey-Service, registriert Tray-Icon.
- **Window-Management** — `OverlayWindow`: frameless, topmost, zentriert, Fokus-Grab; per Hotkey toggeln, `Esc` schließt (versteckt, nicht beendet).
- **ViewModels** — `OverlayViewModel` (Suchfeld, gefilterte Projekte, Action-Auswahl, Enter→Launch), `SettingsViewModel` (Projekte/Templates/ScanRoots verwalten).
- **Tray** — App läuft headless im Hintergrund; Tray-Menü: Anzeigen, Einstellungen, Beenden.

---

## 5. Command-Ausführung (OS-Weiche)

Hinter `IProcessLauncher`. Entscheidung zur Laufzeit per `RuntimeInformation.IsOSPlatform`:

```csharp
var psi = new ProcessStartInfo {
    WorkingDirectory = project.Path,
    UseShellExecute = false,
    CreateNoWindow = true,
};
if (OperatingSystem.IsWindows()) { psi.FileName = "cmd.exe";  psi.Arguments = $"/c \"{command}\""; }
else                             { psi.FileName = "/bin/bash"; psi.Arguments = $"-c \"{command}\""; }
Process.Start(psi);
```

**Offene Detailfragen** (für Refinement):
- GUI-Apps (IDE) vs. Terminal-Commands: Bei GUI-Apps ggf. `UseShellExecute = true` ohne `cmd`-Wrapper sinnvoller. → evtl. Action-Flag ` launchKind: shell | direct`.
- Argument-Quoting / Injection-Sicherheit bei zusammengesetzten Commands.
- Fehler-Feedback an UI, wenn `Process.Start` fehlschlägt (Exe nicht gefunden).

---

## 6. Globaler Hotkey (SharpHook) — kritischer Pfad

SharpHook-Eckdaten (v7), die das Design bestimmen:

- **`SuppressEvent` ist synchron und NUR auf Windows/macOS unterstützt — nicht auf Linux.**
- Suppression erfordert **`SimpleGlobalHook`** (Handler laufen auf dem Hook-Thread). `EventLoopGlobalHook` ignoriert Suppression.
- `hook.Run()` blockiert → muss auf **dediziertem Hintergrund-Thread** laufen.

**Konsequenzen fürs Design:**

1. `SharpHookHotkeyService` nutzt `SimpleGlobalHook`, gestartet via `Task.Run(() => hook.Run())`.
2. Im `KeyPressed`-Handler (Hook-Thread):
   - Hotkey-Match prüfen.
   - **Windows:** `e.SuppressEvent = true` **synchron** setzen → verhindert das System-Fenstermenü bei `Alt+Space`.
   - UI-Toggle an den Avalonia-UI-Thread marshallen: `Dispatcher.UIThread.Post(() => ToggleOverlay())`. Niemals UI direkt vom Hook-Thread.
3. **Linux-Realität:** Keine Suppression möglich → der Tastendruck leckt an die fokussierte App. Bei `Alt+Space` unter Linux i.d.R. unkritisch (kein globales Systemmenü), aber dokumentieren. → Empfehlung: **plattformabhängiger Default-Hotkey** denkbar.

> [!warning] Wayland (Linux)
> Globale Hooks sind unter Wayland stark eingeschränkt; SharpHook nutzt `/dev/input/` → User muss in Gruppe **`input`** sein. Unter X11 unproblematisch. Hauptrisiko der Linux-Unterstützung — beim ersten Start prüfen und dem User eine klare Fehlermeldung/Anleitung geben.

---

## 7. Config-Subsystem

**Ablageort (Pfad-Resolver, Reihenfolge):**
1. **Portable**: existiert `projektor.toml` neben der Executable → diese nutzen.
2. Sonst Standard: Linux `~/.config/projektor/projektor.toml`, Windows `%APPDATA%\Projektor\projektor.toml`.

**Verhalten:**
- Laden beim Start; bei Fehlen → Default-Config mit Beispiel-Template ("Terminal hier") schreiben.
- `FileSystemWatcher` → externe Edits (vim) live übernehmen, `Changed`-Event an die App.
- Schreiben nur bei UI-getriebenen Änderungen (Projekt hinzufügen etc.); Tomlyn erhält Kommentare/Struktur soweit möglich.

**TOML-Skizze:**

```toml
[settings]
hotkey = "Alt+Space"
theme  = "dark"

[[action_templates]]
id      = "terminal"
name    = "Terminal hier"
command = "..."        # OS-spezifisch zur Laufzeit aufgelöst

[[projects]]
name   = "Projektor"
path   = "~/dev/projektor"
source = "manual"
disabled_templates = []

[[scan_roots]]
path = "~/dev"
```

> Offen: OS-spezifische Command-Werte pro Template (z.B. `command.windows` / `command.linux`) vs. ein generischer Command mit Platzhaltern. Im Refinement entscheiden.

---

## 8. Lifecycle, Threading & Autostart

**Threading-Modell (3 relevante Threads):**

| Thread | Rolle |
|---|---|
| UI-Thread (Avalonia Dispatcher) | Alles Sichtbare; einziger erlaubter Zugriff auf Views/VMs |
| Hook-Thread (SharpHook `Run()`) | Empfängt Tastatur-Events; setzt Suppression synchron; postet Toggle an UI-Thread |
| Worker (Task-Pool) | `Process.Start`, ScanRoot-Discovery, Config-IO — nichts UI-Blockierendes |

**App-Lifecycle:**
- Start (ggf. via Autostart) → headless in den Tray, Hook aktiv, Overlay versteckt.
- Hotkey → Overlay zeigen (zentriert, Fokus). `Esc`/Fokusverlust → verstecken (Prozess lebt weiter).
- Action-Enter → launchen, Overlay verstecken.
- Beenden nur über Tray/Settings → Hook sauber `Dispose()`n.

**Autostart** (hinter `IAutostartManager`):
- Linux: `~/.config/autostart/projektor.desktop`.
- Windows: `.lnk` im Startup-Ordner (`Environment.SpecialFolder.Startup`).
- Toggle in den Settings; beim ersten Start optional anbieten.

---

## 9. Build-Slice-Mapping (aus PRD §"Erster Build-Slice")

| PRD-Schritt | Architektur-Touchpoint |
|---|---|
| 1. Avalonia-Overlay (`.slnx`, frameless, topmost) | `App`: `OverlayWindow` + Bootstrap |
| 2. SharpHook Hotkey + Win-Suppression | `Infrastructure`: `SharpHookHotkeyService` (§6) |
| 3. TOML-Config minimal laden | `Infrastructure`: `TomlConfigStore` + `Core`-Modelle (§7) |
| 4. Filtern → Action im WorkingDir starten | `Core`: `ProjectFilter`/`ActionResolver` → `Infrastructure`: `ProcessLauncher` (§5) |
| 5. 1 Projekt, Template "Terminal hier" auf beiden OS | End-to-End-Verdrahtung in `App` |

**Slice-Reihenfolge der Projekte:** `Core` (Modelle + Ports, leer testbar) → `Infrastructure` (Config + Process + Hotkey) → `App` (verdrahten). Bewusst dünn: nur was Schritt 5 braucht.

---

## 10. Offene Entscheidungen (vor Implementierung)

1. **MVVM-Lib:** CommunityToolkit.Mvvm (Empfehlung) vs. ReactiveUI.
2. **Command-Modell:** OS-spezifische Commands pro Template vs. Platzhalter/generisch; `launchKind` (shell vs. direct) ja/nein.
3. **Default-Hotkey:** global `Alt+Space` vs. plattformabhängiger Default (Linux-Suppression-Lücke).
4. **DI-Umfang:** voller Generic Host vs. nur `ServiceCollection`.
5. **Scanner-Timing:** Auto-Scan bei Start / on-demand / im Hintergrund mit Cache.

---

## Related

- [PRD.md](PRD.md) — Produkt-Anforderungen, Entscheidungen, Risiken.
