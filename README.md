# EMERGENT DESKTOP v0.3

**Native sci-fi desktop widgets for Windows 11** — built in C# and WPF, targeting **.NET 10** (your installed SDK is `10.0.201`). No Rainmeter, Electron, browser window, third-party NuGet packages, external APIs, or elevated privileges are required by the application itself.

> **SG workplace check:** Being allowed to compile and run C# software does not automatically permit personal desktop customization utilities. Only run this project if that use is authorized by your organization's rules. Do not work around software controls or copy confidential data into the project.

## What you get

| Widget | Function |
| --- | --- |
| Command header | Emergent Lab identity, system status, local date, Exit control |
| System telemetry | Real Windows CPU utilization, physical RAM usage, and aggregate local network-interface byte rates |
| Mission launcher | Eight functional Windows shortcuts, customizable in a JSON file |
| Mission time | Real local clock and date |
| Orbital Earth wallpaper | Separate panoramic PNG intended for Windows' normal wallpaper setting |

The glass panels are transparent, animated, independently draggable, and remember their positions. The application has a tray icon: **double-click to restore**, or right-click for **Show widgets / Hide widgets / Settings / Exit**. No startup entry or scheduled task is created.

## 1. Build and run on your Windows 11 laptop

1. Extract the ZIP to an **approved development folder**.
2. Open PowerShell or a terminal from your normal SG development environment.
3. From the extracted folder, run:

```powershell
dotnet build .\EmergentDesktop\EmergentDesktop.csproj -c Release
dotnet run --project .\EmergentDesktop\EmergentDesktop.csproj -c Release
```

Alternatively open `EmergentDesktop/EmergentDesktop.csproj` in Visual Studio or Rider and start the project normally.

**Expected result:** four separate windows appear: thin header at the top, telemetry on the left, launcher on the right, clock near the bottom center. A tray icon appears in Windows' notification area. The first CPU/network reading may be zero until the second sample.

The project targets `net10.0-windows`. It uses WPF and Windows Forms only for the built-in tray icon. Windows Desktop development support must be available in your SDK/IDE installation.

### If your corporate network blocks NuGet

This project deliberately has **zero third-party package references**. It uses only .NET 10 and built-in Windows libraries. If SDK/targeting-pack resolution is blocked by policy, ask IT for the approved SDK components; don't use an external mirror or bypass corporate restrictions.

## 2. Apply the wallpaper

The bundled `Assets/earth-orbit.png` is a *real wallpaper with no fake data panels*. From Windows Explorer, right-click and **Set as desktop background** (if corporate policy allows it). In Settings > Personalization > Background, choose **Fill** for a typical 16:9 screen, or **Fit** if Windows cuts off parts of the scene.

The application itself **never changes your wallpaper**. This keeps it reversible and avoids altering Windows personalization settings programmatically.

## 3. Use your widgets

- **Drag:** left-click and drag any widget's title band. The new position is saved after the drag.
- **Launch:** click a shortcut from the right panel. Only the clicked target is launched.
- **Configure shortcuts:** select **EDIT LINKS**, edit `settings.json` in Notepad, save, and select **RELOAD**. The initial file is generated in `%LOCALAPPDATA%\EmergentDesktop\settings.json`.
- **Hide / Restore:** tray-menu commands. If you press `Win+D` and a widget disappears, use the tray icon to bring it back.
- **Exit completely:** click **EXIT** in the top bar or **Exit Emergent Desktop** in the tray menu.

Example shortcut entry inside `settings.json`:

```json
{
  "Group": "BUILD / ANALYZE",
  "Label": "NOTEPAD",
  "Target": "notepad.exe",
  "Arguments": ""
}
```

`Group` determines the section heading. `Target` is a Windows executable, approved absolute executable path, or registered URI such as `ms-settings:`. `Arguments` are passed as separate launch arguments. Do not put passwords, internal URLs, tokens, or other company secrets in this file.

If you corrupt the JSON, the app loads safe defaults for that run. You can repair the file through Notepad and reload.

## 4. Optional portable executable

If corporate policy permits running a self-contained custom utility, publish it using:

```powershell
dotnet publish .\EmergentDesktop\EmergentDesktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
```

Find the resulting executable under `EmergentDesktop\bin\Release\net10.0-windows\win-x64\publish\`.

This is an **application executable, not an installer**. Publishing doesn't bypass AppLocker, Defender Application Control, or SG software policy. If IT requires signing or managed distribution, use that process. The development build itself is sufficient for the first test.

## 5. What v0.3 does **not** do

This is an important engineering boundary:

- It provides **native transparent WPF widget windows**, visible on the ordinary desktop. They are **not yet embedded in Explorer's wallpaper/desktop-icon layer** like Rainmeter's special desktop positioning. `Win+D` or window management may hide them, and the tray icon restores them.
- It does not require or attempt undocumented Explorer shell hooks. Those can be fragile across Windows updates and are a poor default for a corporate laptop.
- It does not read GPU sensors, stock quotes, weather feeds, or SG-specific data.
- It doesn't transmit system metrics or make background network requests. Network readings are aggregate local byte counters, and on VPN/virtual-adapter setups can include multiple interfaces.
- It doesn't launch itself on login, create services, install drivers, change registry settings, or require administrator rights.

**Target for v0.4:** after testing the current UI, decide whether an IT-approved desktop hosting mechanism is available for true Rainmeter-like desktop anchoring.

## 6. First-run acceptance checklist

- [ ] `dotnet build` completes without errors.
- [ ] Four panels appear and have readable text at your screen scaling.
- [ ] CPU percentage changes after a few seconds.
- [ ] RAM percentage and used GiB are plausible.
- [ ] Network readings respond to ordinary permitted internet activity (VPN adapters may affect totals).
- [ ] All default launchers open normal Windows utilities.
- [ ] Panels can be dragged; positions persist after restarting.
- [ ] Tray **Hide**, **Show**, and **Exit** work.
- [ ] Normal work applications remain usable; widgets do not force themselves above other windows.

If something fails, send only the **build error message and non-sensitive UI observations**. Do not upload SG source code, internal paths, credentials, telemetry logs, or a screenshot with confidential windows visible.

## 7. Uninstall / remove

1. Choose **Exit Emergent Desktop** from the tray icon.
2. Delete the extracted project/published files only if you no longer want them.
3. Optionally delete `%LOCALAPPDATA%\EmergentDesktop\settings.json` to remove personal layout/shortcut preferences.
4. Restore your original wallpaper through Windows Settings.

No other system settings are changed by this application.

---

**Design language:** midnight navy, cyan telemetry, restrained amber accents, orbital scientific command deck. Built for Mohcine / EMERGENT LAB.
