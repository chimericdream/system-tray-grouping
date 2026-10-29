# TrayGroups

Group multiple apps under a single Windows system tray icon.

Instead of a tray full of icons you can't tell apart, create named groups
("Security", "Utilities", "AI", ...). Each group gets one tray icon. Click it
to see a menu of `(icon) App name` entries, and click an entry to launch or
focus that app.

> **Status:** planning. Nothing is implemented yet. See [docs/PLAN.md](docs/PLAN.md).

## What it does not do

Windows has no supported API for moving or merging another app's tray icon,
so TrayGroups does not literally absorb existing icons. It is a grouped
launcher. To declutter the tray, hide the original icons with Windows'
built-in toggle (Settings > Personalization > Taskbar > Other system tray
icons). See the plan for details and limitations.

## Development

Tool versions are managed with [mise](https://mise.jdx.dev/):

```sh
mise install
```

## License

MIT. See [LICENSE](LICENSE).
