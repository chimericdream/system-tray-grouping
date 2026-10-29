using System.Diagnostics;
using TrayGroups.Core.Configuration;
using TrayGroups.Core.Discovery;
using TrayGroups.Core.Launching;
using TrayGroups.Host.Icons;
using TrayGroups.Host.Startup;

namespace TrayGroups.Host.Settings;

/// <summary>Edits groups and their entries. Changes apply when Save is pressed.</summary>
internal sealed class SettingsForm : Form
{
    private readonly Action<TrayGroupsConfig> _onSave;
    private readonly List<GroupModel> _models;
    private readonly bool _autostartAtOpen = AutostartManager.IsEnabled();

    private readonly ListBox _groupList = new() { Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly TextBox _iconPath = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly ListView _entryList = new()
    {
        View = View.Details,
        FullRowSelect = true,
        HideSelection = false,
        MultiSelect = false,
        AllowDrop = true,
        Dock = DockStyle.Fill,
    };
    private readonly ImageList _entryImages = new() { ImageSize = new Size(24, 24), ColorDepth = ColorDepth.Depth32Bit };
    private readonly CheckBox _autostart = new() { Text = "Start TrayGroups when I sign in to Windows", AutoSize = true };
    private readonly Panel _detail = new() { Dock = DockStyle.Fill, Enabled = false };

    public SettingsForm(TrayGroupsConfig config, Action<TrayGroupsConfig> onSave)
    {
        _onSave = onSave;
        _models = [.. config.Groups.Select(GroupModel.From)];

        Text = "TrayGroups settings";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(760, 460);
        ClientSize = new Size(860, 540);
        AllowDrop = false;

        _autostart.Checked = _autostartAtOpen;

        _entryList.SmallImageList = _entryImages;
        _entryList.Columns.Add("Name", 200);
        _entryList.Columns.Add("Target", 380);

        BuildLayout();
        WireEvents();
        RefreshGroups(selectIndex: _models.Count > 0 ? 0 : -1);
    }

    private GroupModel? Current => _groupList.SelectedIndex >= 0 ? _models[_groupList.SelectedIndex] : null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _entryImages.Dispose();
        }

        base.Dispose(disposing);
    }

    private void BuildLayout()
    {
        var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.Controls.Add(new Label { Text = "Groups", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        left.Controls.Add(_groupList, 0, 1);
        left.Controls.Add(ButtonRow(
            Button("New…", OnNewGroup),
            Button("Rename…", OnRenameGroup),
            Button("Delete", OnDeleteGroup),
            Button("▲", (_, _) => MoveGroup(-1)),
            Button("▼", (_, _) => MoveGroup(1))), 0, 2);

        var iconRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 34, ColumnCount = 4 };
        iconRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        iconRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        iconRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        iconRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        iconRow.Controls.Add(new Label { Text = "Group icon", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 8, 0) }, 0, 0);
        iconRow.Controls.Add(_iconPath, 1, 0);
        iconRow.Controls.Add(Button("Browse…", OnBrowseGroupIcon), 2, 0);
        iconRow.Controls.Add(Button("Clear", OnClearGroupIcon), 3, 0);

        var entryButtons = ButtonRow(
            Button("Add app…", OnAddApp),
            Button("Add from installed…", OnAddInstalled),
            Button("Edit…", OnEditEntry),
            Button("Remove", OnRemoveEntry),
            Button("▲", (_, _) => MoveEntry(-1)),
            Button("▼", (_, _) => MoveEntry(1)));
        entryButtons.Dock = DockStyle.Bottom;

        var hint = new Label
        {
            Text = "Tip: drag programs or shortcuts onto the list to add them. Leave the group icon empty to use a grid of the apps' icons.",
            AutoSize = true,
            Dock = DockStyle.Bottom,
            Padding = new Padding(0, 4, 0, 4),
        };

        _detail.Padding = new Padding(8);
        _detail.Controls.Add(_entryList);
        _detail.Controls.Add(hint);
        _detail.Controls.Add(entryButtons);
        _detail.Controls.Add(iconRow);

        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 240, FixedPanel = FixedPanel.Panel1 };
        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(_detail);

        var save = new Button { Text = "Save", AutoSize = true, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        AcceptButton = save;
        CancelButton = cancel;
        save.Click += (_, _) => Save();

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 46,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
        };
        bottom.Controls.AddRange([cancel, save]);
        _autostart.Margin = new Padding(0, 8, 24, 0);
        bottom.Controls.Add(_autostart);

        Controls.Add(split);
        Controls.Add(bottom);
    }

    private void WireEvents()
    {
        _groupList.SelectedIndexChanged += (_, _) => OnGroupSelected();
        _entryList.DoubleClick += OnEditEntry;
        _entryList.DragEnter += (_, e) =>
            e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
        _entryList.DragDrop += OnDropFiles;
    }

    private static Button Button(string text, EventHandler onClick)
    {
        var button = new Button { Text = text, AutoSize = true, MinimumSize = new Size(32, 0) };
        button.Click += onClick;
        return button;
    }

    private static FlowLayoutPanel ButtonRow(params Button[] buttons)
    {
        var row = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        row.Controls.AddRange(buttons);
        return row;
    }

    // ----- Groups -----

    private void RefreshGroups(int selectIndex)
    {
        _groupList.BeginUpdate();
        _groupList.Items.Clear();
        foreach (var model in _models)
        {
            _groupList.Items.Add(model.Name);
        }

        _groupList.EndUpdate();

        if (selectIndex >= 0 && selectIndex < _models.Count)
        {
            _groupList.SelectedIndex = selectIndex;
        }
        else
        {
            OnGroupSelected();
        }
    }

    private void OnGroupSelected()
    {
        var group = Current;
        _detail.Enabled = group is not null;
        _iconPath.Text = group?.IconPath ?? string.Empty;
        RefreshEntries(selectIndex: -1);
    }

    private void OnNewGroup(object? sender, EventArgs e)
    {
        var name = PromptForm.Ask(this, "New group", "Group name");
        if (name is null)
        {
            return;
        }

        _models.Add(NewModel(name));
        RefreshGroups(_models.Count - 1);
    }

    private void OnRenameGroup(object? sender, EventArgs e)
    {
        if (Current is not { } group)
        {
            return;
        }

        var name = PromptForm.Ask(this, "Rename group", "Group name", group.Name);
        if (name is null)
        {
            return;
        }

        group.Name = name;
        RefreshGroups(_groupList.SelectedIndex);
    }

    private void OnDeleteGroup(object? sender, EventArgs e)
    {
        if (Current is not { } group)
        {
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"Delete the group \"{group.Name}\"? Its apps are not uninstalled.",
            Text,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer == DialogResult.Yes)
        {
            var index = _groupList.SelectedIndex;
            _models.Remove(group);
            RefreshGroups(Math.Min(index, _models.Count - 1));
        }
    }

    private void MoveGroup(int delta)
    {
        var index = _groupList.SelectedIndex;
        var target = index + delta;
        if (index < 0 || target < 0 || target >= _models.Count)
        {
            return;
        }

        (_models[index], _models[target]) = (_models[target], _models[index]);
        RefreshGroups(target);
    }

    private void OnBrowseGroupIcon(object? sender, EventArgs e)
    {
        if (Current is not { } group)
        {
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "Images and icons|*.ico;*.png;*.jpg;*.jpeg;*.bmp;*.exe;*.dll|All files|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            group.IconPath = PathPortability.Collapse(dialog.FileName);
            _iconPath.Text = group.IconPath;
        }
    }

    private void OnClearGroupIcon(object? sender, EventArgs e)
    {
        if (Current is { } group)
        {
            group.IconPath = null;
            _iconPath.Text = string.Empty;
        }
    }

    private GroupModel NewModel(string name) => new()
    {
        Id = GroupIds.Create(name, _models.Select(model => model.Id)),
        Name = name,
    };

    // ----- Entries -----

    private void RefreshEntries(int selectIndex)
    {
        _entryList.BeginUpdate();
        _entryList.Items.Clear();
        _entryImages.Images.Clear();

        foreach (var entry in Current?.Entries ?? [])
        {
            using var bitmap = IconLoader.Load(entry.IconPath, entry.Target);
            var imageIndex = -1;
            if (bitmap is not null)
            {
                _entryImages.Images.Add(bitmap);
                imageIndex = _entryImages.Images.Count - 1;
            }

            var item = new ListViewItem(entry.Name, imageIndex);
            item.SubItems.Add(entry.Target);
            _entryList.Items.Add(item);
        }

        _entryList.EndUpdate();

        if (selectIndex >= 0 && selectIndex < _entryList.Items.Count)
        {
            _entryList.Items[selectIndex].Selected = true;
        }
    }

    private int SelectedEntryIndex => _entryList.SelectedIndices.Count > 0 ? _entryList.SelectedIndices[0] : -1;

    private void OnAddApp(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Programs and shortcuts|*.exe;*.lnk;*.bat;*.cmd|All files|*.*",
            Multiselect = true,
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            AddFiles(dialog.FileNames);
        }
    }

    private void OnDropFiles(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
        {
            AddFiles(files);
        }
    }

    private void AddFiles(IEnumerable<string> paths)
    {
        if (Current is not { } group)
        {
            return;
        }

        foreach (var path in paths.Where(File.Exists))
        {
            group.Entries.Add(new EntryConfig
            {
                Name = DisplayNameFor(path),
                Target = PathPortability.Collapse(path),
            });
        }

        RefreshEntries(group.Entries.Count - 1);
    }

    private void OnAddInstalled(object? sender, EventArgs e)
    {
        if (AppPickerForm.Pick(this) is not { } result)
        {
            return;
        }

        var group = result.NewGroupName is { } name ? AddNewGroup(name) : Current;
        if (group is null)
        {
            return;
        }

        foreach (var app in result.Apps)
        {
            group.Entries.Add(new EntryConfig
            {
                Name = app.Name,
                Target = PathPortability.Collapse(app.Target),
            });
        }

        if (result.NewGroupName is null)
        {
            RefreshEntries(group.Entries.Count - 1);
        }
    }

    private GroupModel AddNewGroup(string name)
    {
        var model = NewModel(name);
        _models.Add(model);
        RefreshGroups(_models.Count - 1);
        return model;
    }

    private void OnEditEntry(object? sender, EventArgs e)
    {
        var index = SelectedEntryIndex;
        if (Current is not { } group || index < 0)
        {
            return;
        }

        if (EntryEditorForm.Edit(this, group.Entries[index]) is { } edited)
        {
            group.Entries[index] = edited;
            RefreshEntries(index);
        }
    }

    private void OnRemoveEntry(object? sender, EventArgs e)
    {
        var index = SelectedEntryIndex;
        if (Current is not { } group || index < 0)
        {
            return;
        }

        group.Entries.RemoveAt(index);
        RefreshEntries(Math.Min(index, group.Entries.Count - 1));
    }

    private void MoveEntry(int delta)
    {
        var index = SelectedEntryIndex;
        var target = index + delta;
        if (Current is not { } group || index < 0 || target < 0 || target >= group.Entries.Count)
        {
            return;
        }

        (group.Entries[index], group.Entries[target]) = (group.Entries[target], group.Entries[index]);
        RefreshEntries(target);
    }

    private static string DisplayNameFor(string path)
    {
        if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            var info = FileVersionInfo.GetVersionInfo(path);
            var name = !string.IsNullOrWhiteSpace(info.ProductName) ? info.ProductName : info.FileDescription;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name.Trim();
            }
        }

        return Path.GetFileNameWithoutExtension(path);
    }

    // ----- Saving -----

    private void Save()
    {
        try
        {
            _onSave(new TrayGroupsConfig { Groups = [.. _models.Select(model => model.ToConfig())] });

            if (_autostart.Checked != _autostartAtOpen)
            {
                AutostartManager.SetEnabled(_autostart.Checked);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Couldn't save settings: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.None;
        }
    }

    private sealed class GroupModel
    {
        public required string Id { get; init; }

        public required string Name { get; set; }

        public string? IconPath { get; set; }

        public List<EntryConfig> Entries { get; } = [];

        public static GroupModel From(GroupConfig config)
        {
            var model = new GroupModel { Id = config.Id, Name = config.Name, IconPath = config.IconPath };
            model.Entries.AddRange(config.Entries);
            return model;
        }

        public GroupConfig ToConfig() => new()
        {
            Id = Id,
            Name = Name,
            IconPath = IconPath,
            Entries = [.. Entries],
        };
    }
}
