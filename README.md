# TrayGroups

Group multiple apps under a single Windows system tray icon.

Instead of a tray full of icons you can't tell apart, create named groups
("Security", "Utilities", "AI", ...). Each group gets one tray icon. Click it
to see a menu of `(icon) App name` entries, and click an entry to launch or
focus that app.

> **Status:** early development (pre-1.0).

## How it works

TrayGroups is a grouped launcher. Windows has no supported API for moving or
merging another app's tray icon, so TrayGroups does not absorb existing icons.
To declutter your tray, hide the originals with Windows' own toggle
(Settings > Personalization > Taskbar > Other system tray icons) and launch
the apps from their group instead.

## Using it

1. Run `TrayGroups.Host.exe`. The settings window opens on first run.
2. Create a group and add apps by dragging programs or shortcuts onto the list,
   picking a file, or choosing **Add from installed...** (which can also
   suggest groups by publisher).
3. Optionally choose a group icon. Without one, TrayGroups shows a grid of the
   first four apps' icons.
4. Save. Each group now has its own tray icon; left- or right-click opens the
   menu. Launching TrayGroups again opens the settings window.

Your config lives at `%APPDATA%\TrayGroups\config.json`. See
[examples/config.example.json](examples/config.example.json) for the format.
Paths may use environment variables such as `%ProgramFiles%`.

## Limitations

- Apps that only exist as a tray icon (no window to launch) are awkward to
  start from a menu.
- Windows only lists an app in the tray-icon toggle after it has shown an icon
  at least once, and some system icons can't be hidden.
- Packaged (Microsoft Store) apps aren't discovered automatically yet. You can
  add one by hand with a `shell:AppsFolder\<AppUserModelID>` target.
- Dark mode uses WinForms' experimental system color mode, so menus may look
  inconsistent.

## Development

See [CONTRIBUTING.md](CONTRIBUTING.md). Tool versions are managed with
[mise](https://mise.jdx.dev/):

```sh
mise install
mise exec -- dotnet test
```

## License

MIT. See [LICENSE](LICENSE).
