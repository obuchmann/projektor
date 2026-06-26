# 4. Globaler Hotkey, Suppression & Wayland-Risiko

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Das Overlay wird per globalem Hotkey (Default `Alt+Space`) aufgerufen — überall, auch wenn die
App keinen Fokus hat. SharpHook (libuiohook) liefert dafür einen plattformübergreifenden Hook,
hat aber harte Constraints:

- `SuppressEvent` ist synchron und **nur auf Windows/macOS** unterstützt, **nicht auf Linux**.
- Suppression erfordert `SimpleGlobalHook` (Handler laufen auf dem Hook-Thread);
  `EventLoopGlobalHook` ignoriert sie.
- Der Hook-Loop blockiert → muss auf einem Hintergrund-Thread laufen.
- `Alt+Space` ist unter Windows das Fenster-Systemmenü → muss unterdrückt werden.

## Entscheidung

- `SharpHookHotkeyService` nutzt `SimpleGlobalHook(GlobalHookType.Keyboard)`, gestartet via
  `RunAsync()` (Hintergrund-Thread).
- Default-Hotkey **`Alt+Space`** auf beiden Plattformen.
- Im `KeyPressed`-Handler: Match aus `e.Data.KeyCode` + `e.RawEvent.Mask` (über `HotkeyParser`);
  bei Match auf Windows/macOS `e.SuppressEvent = true` **synchron** setzen; danach das
  UI-Toggle via `Dispatcher.UIThread.Post(...)` an den UI-Thread marshallen — nie UI direkt vom
  Hook-Thread.
- Hotkey-String wird tolerant geparst (`HotkeyParser`: `Alt|Ctrl|Shift|Meta` + `Vc*`-KeyCode),
  Parser ist unit-getestet.

## Konsequenzen

- **Linux:** Keine Suppression möglich → der Tastendruck leckt an die fokussierte App. Bei
  `Alt+Space` meist unkritisch (kein globales Systemmenü), aber dokumentiert.
- **Wayland:** Globale Hooks sind stark eingeschränkt; SharpHook nutzt `/dev/input/` → der User
  muss in der Gruppe **`input`** sein, sonst bleibt der Hook stumm. Unter X11 unproblematisch.
  Das ist das **Hauptrisiko der Linux-Unterstützung** — beim Start sollte später eine klare
  Fehlermeldung/Anleitung folgen (Backlog).

## Annahme

- Plattformabhängiger Default-Hotkey wäre denkbar, wird aber vorerst nicht gemacht
  (`Alt+Space` überall). Änderbar über die Config.
