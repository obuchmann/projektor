# Projektor — Dev Quick Launcher
## Product Requirements Document v1.0

> **Status:** Konzept vollständig. Alle Kern-Entscheidungen getroffen, erster Build-Slice definiert.

---

## Problem / Motivation

Als Entwickler arbeite ich ständig in wechselnden Projekten — und ein Projekt ist im Kern immer **ein Ordner** plus **immer wieder dieselben Tools**, die ich in genau diesem Ordner-Kontext öffne (IDE, Editor, Terminal, Build-/Run-Commands). Heute heißt das: Ordner suchen, IDE öffnen, Terminal aufmachen, in den Ordner `cd`en, Command tippen. Reibung bei jedem Kontextwechsel.

**Projektor** ist ein Quick Launcher, bei dem **jeder Start immer einen Projekt-Kontext trägt**: Projekt (Ordner) auswählen → zugeordnete Executables/Commands werden direkt im Working Directory dieses Projekts ausgeführt.

---

## Vision

Ein Quick Launcher, bei dem jede Aktion immer im Kontext eines Projekt-Ordners passiert — als GUI-Overlay, das per globalem Hotkey überall verfügbar ist.

---

## Zielgruppe

Software-Entwickler. Initialer Scope: Single-User / eigener Workflow.

---

## Kern-Konzept

- **Projekt** = ein Ordner im Dateisystem (Working Directory).
- **Action / Executable** = ein Programm oder Command (IDE, Editor, Terminal, beliebiger Shell-Command), das **im Working Directory des Projekts** ausgeführt wird.
- **Launcher** = GUI-Overlay, per globalem Hotkey (Default `Alt+Space`) aufgerufen. Tippen → Projekt filtern → Action(s) starten. Stil-Referenz: Spotlight / Raycast / PowerToys Run — aber **projekt-kontext-first** statt generischer App-Launcher.

---

## Entscheidungen

| Thema | Entscheidung |
|---|---|
| **Name** | Projektor (Projekt + "projector") |
| **Plattform** | Windows + Linux (cross-platform); macOS bewusst out of scope für v1 |
| **Architektur** | Standalone-App — volle Kontrolle, keine Bindung an einen Host-Launcher |
| **Form-Faktor** | GUI-Overlay, global per Hotkey (`Alt+Space`) |
| **Tech-Stack** | .NET (Core), Avalonia UI, SharpHook (libuiohook), `System.Diagnostics.Process`, Solution im `.slnx`-Format |
| **Action-Modell** | Mischung — globale Action-Templates + pro-Projekt-Overrides/Zusätze |
| **Projekt-Erfassung** | Manuelles Hinzufügen + optionaler Auto-Scan eines Root-Ordners nach `.git` |
| **Config-Format** | TOML (via Tomlyn); Linux: `~/.config/projektor/`; Windows: `%APPDATA%\Projektor\`; optionaler portabler Modus |

---

## Datenmodell

- **ActionTemplate** — `id`, `name`, `command`/`exe`, `args`, optional `icon`. Gilt global für alle Projekte, sofern nicht projekt-spezifisch deaktiviert.
- **Project** — `name`, `path`, `source` (`manual` | `scanned`), Liste deaktivierter Templates, Liste projekt-spezifischer Actions (Overrides + Zusätze).
- **ScanRoots** — Liste von Root-Ordnern, die für die Auto-Discovery nach `.git` gescannt werden.
- **Effektive Actions eines Projekts** = (globale Templates − deaktivierte) + projekt-spezifische Overrides/Zusätze.

---

## Features — MVP

- Overlay per globalem Hotkey öffnen, per `Esc` schließen
- Projekte manuell hinzufügen, umbenennen, entfernen
- Globale Action-Templates definieren ("IDE öffnen", "Terminal hier", …)
- Pro-Projekt-Overrides / Zusatz-Actions
- Tippen filtert Projekte → Action im Projekt-Ordner ausführen (Working Directory = Projektpfad)

## Features — Backlog (später)

- Auto-Scan eines Roots nach `.git` (Projekt-Auto-Discovery) — gewünscht, daher früh
- Fuzzy-Search über Projekte/Actions
- Zuletzt/Häufig benutzt, Favoriten
- Pro-Projekt-Metadaten (Git-Status, Branch, README-Preview)

## Out of Scope (v1)

- macOS
- Multi-User / Sync über Geräte
- Vollwertiges Projekt-/Task-Management
- Generischer App-Launcher / Spotlight-Ersatz

---

## Technische Überlegungen & Risiken

### Command-Ausführung (OS-Weiche)

Shell unterscheidet sich (Windows `cmd.exe`/`powershell.exe` vs. Linux `/bin/bash`) — zur Laufzeit per `RuntimeInformation.IsOSPlatform` entscheiden:

```csharp
var psi = new ProcessStartInfo { WorkingDirectory = projektPfad, UseShellExecute = false, CreateNoWindow = true };
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { psi.FileName = "cmd.exe"; psi.Arguments = $"/c \"{command}\""; }
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { psi.FileName = "/bin/bash"; psi.Arguments = $"-c \"{command}\""; }
using var p = Process.Start(psi);
```

### Globaler Hotkey (SharpHook)

`SimpleGlobalHook`, identischer C#-Code für Win+Linux, im Hintergrund-Task gestartet damit das UI nicht blockiert:

```csharp
var hook = new SimpleGlobalHook();
hook.KeyPressed += (s, e) => { /* Hotkey prüfen → Overlay zeigen */ };
Task.Run(() => hook.Start());
```

### Risiko: `Alt+Space`-Konflikt unter Windows

`Alt+Space` ist unter Windows das **Fenster-Systemmenü**. Wird der Default-Hotkey verwendet, muss das Event unterdrückt werden (SharpHook `SuppressEvent` / suppressing hook), sonst poppt zusätzlich das Systemmenü. PowerToys Run nutzt ebenfalls `Alt+Space` — also machbar, aber Suppression ist Pflicht. Alternative: anderen Default wählen.

### Risiko: Wayland-Restriktion (Linux)

Unter **Wayland** sind globale Tastatur-Hooks aus Sicherheitsgründen stark eingeschränkt. SharpHook umgeht das über die Event-Devices `/dev/input/` — dafür muss der User in der Gruppe **`input`** sein, sonst bleibt der Hook stumm. Unter X11 unproblematisch. → Hauptrisiko der Linux-Unterstützung.

### Autostart

- Linux: `.desktop`-Datei unter `~/.config/autostart/`
- Windows: Verknüpfung (`.lnk`) im Startup-Ordner (`Environment.GetFolderPath(Environment.SpecialFolder.Startup)`)

---

## Erster Build-Slice (Vertical Slice)

Ziel: ein End-to-End-Pfad auf Windows + Linux, alles andere danach.

1. Avalonia-Projekt aufsetzen (`.slnx`-Solution), frameless, topmost Overlay-Window (zentriert, Fokus-Grab)
2. SharpHook integrieren: Hotkey → Overlay toggeln; **Windows-Suppression** für `Alt+Space` gleich mitlösen
3. TOML-Config laden (Tomlyn): minimal `Projects` + `ActionTemplates`
4. Tippen filtert Projekt-Liste → ausgewählte Action via `Process` (OS-Weiche) im Working Directory starten
5. **Slice-Abschluss:** 1 Projekt, 1 Template ("Terminal hier") läuft auf beiden OS
