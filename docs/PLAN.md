# TrayGroups: Plan

## Goal

A Windows tray app that shows one tray icon per user-defined group. Clicking a
group's icon opens a menu of `(icon) App name` entries that launch or focus the
corresponding app.

## Non-goals

- Absorbing, reparenting, or proxying other apps' existing tray icons. Windows
  has no supported API for this, and the undocumented approaches break on
  Windows 11 (XAML-based tray).
- Reproducing an app's own tray context menu or live status badges.

## Open-source constraints

- No machine-specific paths, usernames, or app lists in the repo.
- Per-user config lives under `%APPDATA%\TrayGroups\` and is never committed.
- Ship an `examples/config.example.json` with generic groups only.
- MIT license, conventional commits, CI on GitHub Actions.

## Stack

- C# on .NET 10 (LTS), WinForms `NotifyIcon` for the tray.
- SDK version pinned in `mise.toml`; contributors run `mise install`.
- Config as JSON (`System.Text.Json`), with a settings UI so users never edit it
  by hand.
- Tests with xUnit for everything that isn't UI (config, discovery, icon
  extraction, launch logic).

## Architecture

Solution layout:

```
src/
  TrayGroups.Core/   config model, persistence, app discovery, launching
  TrayGroups.App/    WinForms host: tray icons, menus, settings UI
tests/
  TrayGroups.Core.Tests/
docs/
examples/
```

Core concepts:

- **Group**: id, name, optional icon, ordered list of entries.
- **Entry**: display name, target (exe path, shortcut, or `shell:AppsFolder`
  AUMID for packaged apps), optional arguments, optional icon override.
- **Rule** (later): a group can be populated automatically by publisher or
  folder.

Paths in config should support environment variables (`%ProgramFiles%`, etc.)
so a shared config is portable between machines.

## Milestones

1. **Scaffold**: solution, projects, CI (build + test), editorconfig.
2. **Core**: config model, load/save with schema versioning, tests.
3. **Tray MVP**: one icon per group, menu with extracted icons, launch on
   click, single-instance of TrayGroups itself.
4. **Settings UI**: create/rename/delete groups, add entries by file picker or
   drag-and-drop, reorder.
5. **Discovery**: pick from installed apps (Start Menu shortcuts, packaged
   apps); group-by-publisher suggestions.
6. **Polish**: start with Windows toggle, dark/light icons, high-DPI, left vs
   right click behavior.
7. **Release**: GitHub Actions publish of a self-contained build, README
   screenshots, CONTRIBUTING.

## Known limitations to document

- Background-only apps (tray menu, no window) are awkward to launch from a
  menu. Possible later experiment: simulate a click on the original icon, which
  is fragile on Windows 11.
- Windows' per-icon hide toggle only lists apps that have shown an icon at
  least once, and some system icons can't be hidden.

## Decisions

- Group icon: user-chosen image (auto-composed grid may come later).
- Menu: native `ContextMenuStrip` first; a custom flyout is a possible later
  upgrade for dark-mode styling.
- Distribution: zip from GitHub Releases only for now; winget/scoop later.

## Progress

- [x] 1. Scaffold
- [x] 2. Core config model and serializer (persistence to disk still to do)
