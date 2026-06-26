# 9. Windows-Autostart via Registry-Run-Key statt `.lnk`

- Status: Accepted
- Datum: 2026-06-26

## Kontext

Autostart soll pro Plattform umgesetzt werden. Die Architektur-Skizze nannte für Windows eine
`.lnk`-Verknüpfung im Startup-Ordner. Eine `.lnk` programmatisch zu erzeugen erfordert
COM-Interop (`IShellLink`/Windows Script Host), was im Cross-Platform-`net10.0`-Build umständlich
und fehleranfällig ist.

## Entscheidung

- **Linux:** XDG-Desktop-Eintrag `~/.config/autostart/projektor.desktop` (Text-Datei, keine
  Abhängigkeiten).
- **Windows:** per-User Registry-Run-Key
  `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, Wert `Projektor` = `"<exe>"`.
  Silent, ohne COM, ohne Extra-NuGet-Paket (`Microsoft.Win32.Registry` ist in .NET 10 Teil des
  Frameworks). Alle Registry-Zugriffe sind mit `OperatingSystem.IsWindows()` geschützt (CA1416).

## Konsequenzen

- Kein aufblitzendes Konsolenfenster beim Login (im Gegensatz zu einer `.cmd`-Variante).
- Einfacher, testbar-naher Code; beide Implementierungen hinter `IAutostartManager`.
- Verhalten weicht von der ursprünglichen `.lnk`-Skizze ab — bewusst.

## Alternativen

- **`.lnk` via COM**: am ehesten "Windows-idiomatisch", aber COM-Interop-Aufwand. Verworfen für
  v1; bei Bedarf später nachrüstbar (gleiches Interface).
- **`.cmd`/`.bat` im Startup-Ordner**: zero-dependency, aber Konsolen-Flackern. Verworfen.
