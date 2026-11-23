# GitHub Actions Migration Plan

## Overview
Migrate Logazmic CI/CD from AppVeyor to GitHub Actions. The workflow triggers on push to the `release` branch, builds the WPF app, packages it with Squirrel.Windows, creates a git tag, and publishes a GitHub Release with the installer artifacts.

## Current vs Target

| Aspect | AppVeyor (current) | GitHub Actions (target) |
|--------|-------------------|------------------------|
| Trigger | push to `master` | push to `release` |
| Runner | Visual Studio 2022 | `windows-latest` |
| Version | `yyyy.M.d.{appveyor_build_number}` | `yyyy.M.d.{github.run_number}` |
| Nuspec ver | `yyMM.d.{appveyor_build_number}` | `yyMM.d.{github.run_number}` |
| Auth | Encrypted in YAML | Built-in `GITHUB_TOKEN` |
| Tagging | None (AppVeyor used release provider tag) | Explicit `git tag` step |

## Files

### 1. NEW: `.github/workflows/ci.yml`
- **Trigger**: `push: branches: [release]` + `workflow_dispatch` (manual)
- **Permissions**: `contents: write` (to push tag + create release)
- **Job**: `build-and-deploy` on `windows-latest`
- **Steps**:
  1. `actions/checkout@v4`
  2. `actions/setup-dotnet@v4` (dotnet 8.0.x — builds net472 via SDK-style project)
  3. `dotnet restore src/Logazmic/Logazmic.csproj`
  4. `dotnet publish src/Logazmic/Logazmic.csproj -c Release -r win-x64`
  5. Run `squirrel/build-appveyor.ps1` with `BUILD_NUMBER=${{ github.run_number }}`
  6. Upload workflow artifacts: `squirrel/Releases/*`
  7. Compute version string and push git tag `v{yyyy.M.d}.{run_number}`
  8. `softprops/action-gh-release@v2` — create GitHub Release with Squirrel assets

### 2. MODIFY: `squirrel/build-appveyor.ps1`
- Line 12: Replace `$env:appveyor_build_number` with `$env:BUILD_NUMBER`
- This makes the script CI-agnostic

### 3. MODIFY: `README.md`
- Line 4-5: Replace AppVeyor badge with GitHub Actions badge:
  `https://github.com/ihtfw/Logazmic/actions/workflows/ci.yml/badge.svg`

### 4. DELETE: `appveyor.yml`
- No longer needed after migration

## Workflow Diagram

```
Push to 'release'
      │
      ▼
windows-latest runner
      │
      ├─ checkout
      ├─ setup .NET 8.0 SDK
      ├─ dotnet restore
      ├─ dotnet publish (net472, win-x64)
      ├─ build-appveyor.ps1 (Squirrel packaging)
      │     ├─ stamps nuspec version: yyMM.d.{run}
      │     ├─ nuget.exe pack
      │     └─ squirrel.exe --releasify
      ├─ upload artifacts (squirrel/Releases/*)
      ├─ git tag v{yyyy.M.d}.{run}
      └─ GitHub Release + attach assets
```
