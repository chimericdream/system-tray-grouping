using TrayGroups.Host.Native;

namespace TrayGroups.Host.Tray;

/// <summary>The icon shown when no groups exist yet, so the app is still reachable.</summary>
internal sealed class HomeTray : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu = new();

    public HomeTray(Action openSettings, Action exit)
    {
        _menu.Items.Add(new ToolStripMenuItem("Create your first group…", image: null, (_, _) => openSettings()));
        GroupTray.AddAppItems(_menu, openSettings, exit);

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "TrayGroups",
            Visible = true,
        };
        _notifyIcon.MouseUp += (_, e) =>
        {
            if (e.Button is MouseButtons.Left or MouseButtons.Right)
            {
                NativeMethods.SetForegroundWindow(_menu.Handle);
                _menu.Show(Cursor.Position);
            }
        };
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
    }
}
