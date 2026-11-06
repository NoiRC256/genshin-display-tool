# Genshin Display Tool

A small Windows WPF utility for viewing and editing Genshin Impact display settings for the current Windows user. It supports resolution and fullscreen/windowed mode changes.

## Requirements

- Windows 10/11 x64 (the app does not require elevation).
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) to build, or the .NET 10 Desktop Runtime to run a published build.

## Build and run

From this folder:

```powershell
dotnet build .\GenshinDisplayTool.sln -c Release
dotnet run --project .\src\GenshinDisplayTool\GenshinDisplayTool.csproj
```

To publish for Windows x64, use `dotnet publish .\src\GenshinDisplayTool\GenshinDisplayTool.csproj -c Release -r win-x64 --self-contained false`. The published application requires the .NET 10 Windows Desktop Runtime.

The SDK feature band is pinned in `global.json`, and deterministic CI settings are defined in `Directory.Build.props`. To create a release-style archive with a manifest and SHA-256 checksum, run from the repository root:

```powershell
.\scripts\package.ps1 -Version 1.0.0
.\scripts\package.ps1 -Version 1.0.0 -SelfContained
```

Packages are written to `artifacts\package`. The archive contains the complete publish directory and `build-manifest.json`; `SHA256SUMS.txt` is generated beside the archive. Use a protected release environment for code signing rather than storing signing material in this repository. Replace the placeholder `RepositoryUrl` in `Directory.Build.props` before publishing publicly.

## Registry behavior

The tool reads and writes `HKCU\Software\miHoYo\原神` using `Registry.CurrentUser`. Width, height, and fullscreen are written as `REG_DWORD` values named `Screenmanager Resolution Width_h182942802`, `Screenmanager Resolution Height_h2627697771`, and `Screenmanager Is Fullscreen mode_h3981298716`. A fullscreen value of `1` means fullscreen; `0` means windowed. The supplied registry values do not reliably define a borderless mode, so borderless is not presented as supported.

Writes are reread and compared before the success message is shown. The existing HKCU key is never created by Apply: if it is missing, start Genshin Impact once, close it, and try again. Missing keys, wrong value types, invalid dimensions, invalid fullscreen values, and access errors are reported in the status area. The app only uses HKCU and requests `asInvoker`, so administrator rights are not needed. If a write or verification fails, the app attempts to restore the values from immediately before that write.

## Safety and limitations

There is no user-facing backup, restore, import, or export feature. The built-in rollback only applies to a failed write during the current Apply operation; it is not a persistent backup. Apply confirms that the change is persistent and that the game should be closed first. The game can overwrite these values when it exits, and later game versions may change or ignore them. This tool does not modify game files or guarantee that every game version honors the values.
