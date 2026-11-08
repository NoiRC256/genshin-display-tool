# Genshin Display Tool

A small Windows utility for viewing and changing Genshin Impact's display settings. It can update the resolution and switch between fullscreen and windowed mode.

## How it works

The tool reads and writes Windows Registry values under:

`HKCU\Software\miHoYo\原神`

It updates these `REG_DWORD` values:

- `Screenmanager Resolution Width_h182942802`
- `Screenmanager Resolution Height_h2627697771`
- `Screenmanager Is Fullscreen mode_h3981298716`

The fullscreen value is `1` for fullscreen and `0` for windowed mode. The available registry values do not reliably support borderless mode, so the app does not offer it.

The existing registry key is never created by the app. If it is missing, start Genshin Impact once, close it, and try again.

## Safety and limitations

This tool does not work for global version at the moment.

Close Genshin Impact before applying changes. The app reads the values back after writing them and attempts to restore the previous values if a write or verification fails.

The game may overwrite these settings when it exits, and future game versions may change or ignore them. 

The tool does not modify game files and does not require administrator access.

## Requirements

- Windows 10 or 11, 64-bit
- The .NET 10 Windows Desktop Runtime to run a published build
- The .NET 10 SDK to build from source

The app only changes settings for the current Windows user and does not require administrator access.

## Build and run

Run these commands from the repository root:

```powershell
dotnet build .\GenshinDisplayTool.sln -c Release
dotnet run --project .\src\GenshinDisplayTool\GenshinDisplayTool.csproj
```

To create a framework-dependent Windows x64 build:

```powershell
dotnet publish .\src\GenshinDisplayTool\GenshinDisplayTool.csproj `
  -c Release -r win-x64 --self-contained false
```

Install the .NET 10 Windows Desktop Runtime on the target computer and copy the complete publish folder. Do not copy only the executable.

## Create a package

The repository pins the .NET SDK in [`global.json`](global.json) and uses deterministic build settings from [`Directory.Build.props`](Directory.Build.props).

Create a framework-dependent package:

```powershell
.\scripts\package.ps1 -Version 1.0.0
```

Create a self-contained package that does not need the .NET runtime:

```powershell
.\scripts\package.ps1 -Version 1.0.0 -SelfContained
```

Packages are written to `artifacts\package`. Each package includes the complete application, a build manifest, and a SHA-256 checksum.

## Releases

Releases are created manually from GitHub:

1. Open **Actions → Release → Run workflow**.
2. Choose `patch`, `minor`, or `major`.
3. The workflow uses `main`, finds the highest stable `vMAJOR.MINOR.PATCH` tag, and increments the selected part.
4. It creates and pushes the next annotated tag, then publishes the release packages.

If no release tag exists, the first release is `v0.1.0`. The workflow publishes both framework-dependent and self-contained Windows x64 packages, together with SHA-256 checksums and generated release notes.

For example, if the latest release is `v1.4.2`, choosing `patch` creates `v1.4.3`, `minor` creates `v1.5.0`, and `major` creates `v2.0.0`. The workflow creates stable releases only. A protected GitHub environment named `release` can be configured to require approval before publishing.

The build workflow is also manual-only. Use **Actions → Build → Run workflow** to build `main` or another branch, tag, or commit.
