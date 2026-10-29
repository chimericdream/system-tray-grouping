using TrayGroups.Core.Configuration;
using TrayGroups.Host.Settings;
using TrayGroups.Host.Tray;

namespace TrayGroups.Host;

/// <summary>Owns the tray icons for the lifetime of the process and rebuilds them when settings change.</summary>
internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly ConfigStore _store;
    private readonly Control _uiThread = new();
    private readonly List<IDisposable> _trays = [];
    private TrayGroupsConfig _config;
    private SettingsForm? _settings;

    public TrayApplicationContext(ConfigStore store, SingleInstance instance)
    {
        _store = store;
        _config = store.LoadOrRecover();

        _ = _uiThread.Handle; // Force handle creation so BeginInvoke works from other threads.
        instance.Listen(() => _uiThread.BeginInvoke(ShowSettings));

        RebuildTrays();

        if (_config.Groups.Count == 0)
        {
            ShowSettings();
        }
    }

    public void ShowSettings()
    {
        if (_settings is { IsDisposed: false })
        {
            _settings.Activate();
            return;
        }

        _settings = new SettingsForm(_config, ApplyConfig);
        _settings.Show();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeTrays();
            _settings?.Dispose();
            _uiThread.Dispose();
        }

        base.Dispose(disposing);
    }

    private void ApplyConfig(TrayGroupsConfig config)
    {
        _store.Save(config);
        _config = config;
        RebuildTrays();
    }

    private void RebuildTrays()
    {
        DisposeTrays();

        if (_config.Groups.Count == 0)
        {
            _trays.Add(new HomeTray(ShowSettings, ExitThread));
            return;
        }

        foreach (var group in _config.Groups)
        {
            _trays.Add(new GroupTray(group, ShowSettings, ExitThread));
        }
    }

    private void DisposeTrays()
    {
        _trays.ForEach(tray => tray.Dispose());
        _trays.Clear();
    }
}
