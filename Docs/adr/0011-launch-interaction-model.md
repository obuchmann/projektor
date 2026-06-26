# 11. Launch-Interaktionsmodell & Windows-Fokus

- Status: Accepted
- Datum: 2026-06-26
- Kontext: Ergebnis des ersten Windows-Smoke-Tests (siehe [ADR-0006](0006-mvp-vertical-slice-scope.md))

## Kontext

Der erste Windows-Smoke-Test deckte zwei Probleme auf:

1. **Fokus nicht im Fenster:** Das Overlay erschien, erhielt aber keinen Tastaturfokus. Ursache:
   Ein Tray-/Hintergrundprozess darf unter Windows nicht ohne Weiteres das Vordergrundfenster
   übernehmen (Foreground-Lock); ein einfaches `Activate()` reicht nicht.
2. **Kein Launch:** Es gab nur einen Enter-Handler. Ohne Fokus im Fenster kam `Enter` nie an, und
   einen Klick-Handler gab es nicht — also ließ sich keine Action starten.

Zusätzlich wurde ein klareres, schnelleres Launch-Konzept gewünscht.

## Entscheidung

**Launch-Interaktionen** (alle führen über `OverlayViewModel.LaunchOne`):

| Eingabe | Wirkung |
|---|---|
| **Klick** auf eine Action | startet genau diese Action (fokus-unabhängig, robust) |
| **Alt+1 … Alt+9** | startet Tool 1…9 des gewählten Projekts |
| **Enter** | startet Tool 1 |
| **Shift+Enter** | startet **alle** Tools des Projekts |
| **Esc** / Fokusverlust | schließt das Overlay |

Actions werden als nummerierte, klickbare Buttons (`ActionItem` mit `Number`/`ShortcutLabel`)
gerendert. Klick ist der primäre, fokus-unabhängige Pfad.

**Windows-Fokus:** `WindowActivation.ForceForeground` nutzt den Standard-`AttachThreadInput`-
Workaround (Input-Queue temporär an den aktuellen Vordergrund-Thread anhängen, dann
`SetForegroundWindow`/`SetFocus`). Guarded auf Windows; No-op auf Linux. `ShowOverlay` macht
`Show()` → `Activate()` → `ForceForeground` → deferred Fokus auf das Suchfeld; ein
`_activating`-Flag verhindert vorzeitiges Schließen durch das `Deactivated`-Event.

## Konsequenzen

- Launch funktioniert auch ohne perfekten Tastaturfokus (Klick).
- Die Launch-Orchestrierung (`LaunchByIndex`/`LaunchAll`/`LaunchFirst`) ist jetzt unit-getestet
  (`Projektor.App.Tests`, Fake-`IProcessLauncher`).
- Win32-Interop ist Windows-spezifisch, aber cross-platform baubar (P/Invoke + Guard).

## Offen / nächster Smoke-Test

- Ob `AttachThreadInput` den Fokus zuverlässig holt, muss erneut auf echtem Windows geprüft
  werden. Falls nicht ausreichend, ist ein Plan B die Registrierung des Hotkeys über
  `RegisterHotKey` (Win32) statt SharpHook, womit Windows das Fenster legitim in den Vordergrund
  lässt.
