using System.ComponentModel;
using System.Diagnostics;
using TrayGroups.Core.Configuration;
using TrayGroups.Core.Launching;
using TrayGroups.Host.Icons;
using TrayGroups.Host.Native;

namespace TrayGroups.Host.Tray;

/// <summary>
/// One tray icon with a menu of <c>(icon) App name</c> entries. Both left and
/// right click open the menu.
/// </summary>
internal sealed class GroupTray : IDisposable
{
    private const int MaxTooltipLength = 127;

    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu = new();
    private readonly OwnedIcon _icon;

    public GroupTray(GroupConfig group, Action openSettings, Action exit)
    {
        _icon = GroupIconFactory.Create(group);

        var iconSize = (int)Math.Round(16 * _menu.DeviceDpi / 96.0);
        _menu.ImageScalingSize = new Size(iconSize, iconSize);

        PopulateMenu(group, openSettings, exit);

        _notifyIcon = new NotifyIcon
        {
            Icon = _icon.Icon,
            Text = Truncate(group.Name, MaxTooltipLength),
            Visible = true,
        };
        _notifyIcon.MouseUp += OnMouseUp;
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();

        foreach (ToolStripItem item in _menu.Items)
        {
            item.Image?.Dispose();
        }

        _menu.Dispose();
        _icon.Dispose();
    }

    /// <summary>Shared by the empty-state tray icon, which has no group.</summary>
    public static void AddAppItems(ContextMenuStrip menu, Action openSettings, Action exit)
    {
        var app = new ToolStripMenuItem("TrayGroups");
        app.DropDownItems.Add("Settings…", image: null, (_, _) => openSettings());
        app.DropDownItems.Add(new ToolStripSeparator());
        app.DropDownItems.Add("Exit", image: null, (_, _) => exit());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(app);
    }

    private void PopulateMenu(GroupConfig group, Action openSettings, Action exit)
    {
        if (group.Entries.Count == 0)
        {
            _menu.Items.Add(new ToolStripMenuItem("(no apps in this group)") { Enabled = false });
        }

        foreach (var entry in group.Entries)
        {
            var item = new ToolStripMenuItem(entry.Name, IconLoader.Load(entry.IconPath, entry.Target));
            item.Click += (_, _) => Launch(entry);
            _menu.Items.Add(item);
        }

        AddAppItems(_menu, openSettings, exit);
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button is not (MouseButtons.Left or MouseButtons.Right))
        {
            return;
        }

        // Without foreground focus the menu would not dismiss when clicking elsewhere.
        NativeMethods.SetForegroundWindow(_menu.Handle);
        _menu.Show(Cursor.Position);
    }

    private void Launch(EntryConfig entry)
    {
        try
        {
            Process.Start(LaunchPlanner.Plan(entry))?.Dispose();
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            _notifyIcon.ShowBalloonTip(4000, $"Couldn't start {entry.Name}", ex.Message, ToolTipIcon.Error);
        }
    }

    private static string Truncate(string text, int length) =>
        text.Length <= length ? text : text[..length];
}
