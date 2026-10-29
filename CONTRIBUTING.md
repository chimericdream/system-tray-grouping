# Contributing

Thanks for your interest in TrayGroups!

## Setup

Tool versions are managed with [mise](https://mise.jdx.dev/):

```sh
mise install
mise exec -- dotnet build
mise exec -- dotnet test
```

TrayGroups is Windows-only (WinForms). The `TrayGroups.Core` library and its
tests are platform-neutral; `TrayGroups.Host` and its tests need Windows.

## Layout

| Path | Purpose |
| --- | --- |
| `src/TrayGroups.Core` | Config model, persistence, launch planning, discovery logic. No UI. |
| `src/TrayGroups.Host` | WinForms host: tray icons, menus, settings UI. |
| `tests/` | xUnit tests for each project. |

Put logic that doesn't need Windows APIs in `Core` so it can be unit tested.

## Conventions

- [Conventional Commits](https://www.conventionalcommits.org/) for commit
  messages (`feat:`, `fix:`, `docs:`, `chore:`, `test:`, `refactor:`).
- Warnings are errors; the build must be clean.
- Don't commit machine-specific paths, usernames, or personal app lists.
  Use environment-variable paths (`%ProgramFiles%`) in examples and tests.
- Add or update tests with behavior changes.

## Debugging tip

Set `TRAYGROUPS_CONFIG` to a file path to run against a scratch config instead
of `%APPDATA%\TrayGroups\config.json`.
