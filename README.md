# Genshin Screen Tool

A small Windows 11 WPF utility for safely viewing and editing Genshin Impact display settings for the current Windows user.

## Requirements

- Windows 10/11 x64 (the app does not require elevation).
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) to build, or the .NET 10 Desktop Runtime to run a published build.

## Build, run, and publish

From this folder:

```powershell
dotnet build .\GenshinScreenTool.sln -c Release
dotnet run --project .\src\GenshinScreenTool\GenshinScreenTool.csproj
dotnet clean .\GenshinScreenTool.sln -c Release
dotnet build .\GenshinScreenTool.sln -c Release --no-restore
dotnet publish .\src\GenshinScreenTool\GenshinScreenTool.csproj -c Release -r win-x64 --self-contained false
```

The published artifact directory is `src\GenshinScreenTool\bin\Release\net10.0-windows\win-x64\publish`. With `--self-contained false`, copy the entire directory and install the .NET 10 Windows Desktop Runtime; the executable alone is not a complete deployment. A self-contained alternative is `--self-contained true`, which produces a larger directory.

## Registry behavior

The tool reads and writes `HKCU\Software\miHoYo\原神` using `Registry.CurrentUser`. Width, height, and fullscreen are written as `REG_DWORD` values named `Screenmanager Resolution Width_h182942802`, `Screenmanager Resolution Height_h2627697771`, and `Screenmanager Is Fullscreen mode_h3981298716`. A fullscreen value of `1` means fullscreen; `0` means windowed. The supplied registry values do not reliably define a borderless mode, so borderless is not presented as supported.

Writes are reread and compared before the success message is shown. The existing HKCU key is never created by Apply: if it is missing, start Genshin Impact once, close it, and reload. Missing keys, wrong value types, invalid dimensions, invalid fullscreen values, and access errors are reported in the status area. The app only uses HKCU and requests `asInvoker`, so administrator rights are not needed. Apply preflights the key and attempts to restore the previous values if a write or verification fails.

## Backups and safety

Use **Export backup** before making changes to save the currently read values as JSON. **Import backup** validates the JSON object, exact `Width`, `Height`, and `Fullscreen` fields, shared dimension bounds, and the active aspect-ratio policy; it does not write until **Apply settings** is clicked. Apply confirms that the change is persistent and that the game should be closed first. Launch the game again afterward. The game can overwrite these values when it exits, and later game versions may change or ignore these registry values. This tool does not modify game files or guarantee that every game version honors the values.
