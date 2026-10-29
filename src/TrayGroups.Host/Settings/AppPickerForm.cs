using TrayGroups.Core.Discovery;
using TrayGroups.Host.Discovery;
using TrayGroups.Host.Icons;

namespace TrayGroups.Host.Settings;

/// <summary>Result of the installed-app picker.</summary>
/// <param name="NewGroupName">Set when the user chose to put the apps in a new group.</param>
internal sealed record AppPickerResult(IReadOnlyList<DiscoveredApp> Apps, string? NewGroupName);

/// <summary>Lets the user tick installed apps (from Start Menu shortcuts) to add to a group.</summary>
internal sealed class AppPickerForm : Form
{
    private const string AllPublishers = "(all publishers)";

    private readonly TextBox _filter = new() { PlaceholderText = "Filter by name", Dock = DockStyle.Fill };
    private readonly ComboBox _publishers = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly ListView _list = new()
    {
        View = View.Details,
        CheckBoxes = true,
        FullRowSelect = true,
        HeaderStyle = ColumnHeaderStyle.Nonclickable,
        Dock = DockStyle.Fill,
    };
    private readonly ImageList _images = new() { ImageSize = new Size(24, 24), ColorDepth = ColorDepth.Depth32Bit };
    private readonly CheckBox _newGroup = new() { Text = "Put them in a new group named", AutoSize = true };
    private readonly TextBox _newGroupName = new() { Dock = DockStyle.Fill, Enabled = false };
    private readonly Label _status = new() { Text = "Scanning installed appsâ€¦", AutoSize = true };
    private readonly Dictionary<string, DiscoveredApp> _checked = new(StringComparer.OrdinalIgnoreCase);

    private IReadOnlyList<DiscoveredApp> _all = [];
    private IReadOnlyList<PublisherSuggestion> _suggestions = [];

    private AppPickerForm()
    {
        Text = "Add from installed apps";
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimumSize = new Size(480, 400);
        ClientSize = new Size(620, 520);

        _list.SmallImageList = _images;
        _list.Columns.Add("Name", 300);
        _list.Columns.Add("Publisher", 240);

        var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 36, ColumnCount = 2, Padding = new Padding(8, 8, 8, 0) };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        top.Controls.Add(_filter, 0, 0);
        top.Controls.Add(_publishers, 1, 0);

        var groupRow = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 34, ColumnCount = 2, Padding = new Padding(8, 4, 8, 0) };
        groupRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        groupRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        groupRow.Controls.Add(_newGroup, 0, 0);
        groupRow.Controls.Add(_newGroupName, 1, 0);

        var ok = new Button { Text = "Add", DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        AcceptButton = ok;
        CancelButton = cancel;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 44,
            Padding = new Padding(8),
        };
        buttons.Controls.AddRange([cancel, ok]);
        _status.Margin = new Padding(8, 8, 0, 0);
        buttons.Controls.Add(_status);

        Controls.Add(_list);
        Controls.Add(top);
        Controls.Add(groupRow);
        Controls.Add(buttons);

        _filter.TextChanged += (_, _) => Repopulate();
        _publishers.SelectedIndexChanged += (_, _) => OnPublisherChanged();
        _list.ItemChecked += OnItemChecked;
        _newGroup.CheckedChanged += (_, _) => _newGroupName.Enabled = _newGroup.Checked;
        Shown += async (_, _) => await LoadAsync();
    }

    /// <summary>Returns null if cancelled or nothing was ticked.</summary>
    public static AppPickerResult? Pick(IWin32Window owner)
    {
        using var form = new AppPickerForm();
        if (form.ShowDialog(owner) != DialogResult.OK || form._checked.Count == 0)
        {
            return null;
        }

        var name = form._newGroup.Checked ? form._newGroupName.Text.Trim() : null;
        return new AppPickerResult([.. form._checked.Values], string.IsNullOrEmpty(name) ? null : name);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _images.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task LoadAsync()
    {
        _all = await StartMenuScanner.ScanAsync();
        _suggestions = AppSuggestions.ByPublisher(_all);

        _publishers.Items.Add(AllPublishers);
        foreach (var suggestion in _suggestions)
        {
            _publishers.Items.Add($"{suggestion.Publisher} ({suggestion.Apps.Count})");
        }

        _publishers.SelectedIndex = 0;
        _status.Text = $"{_all.Count} apps found";
        Repopulate();
    }

    private IEnumerable<DiscoveredApp> VisibleApps()
    {
        IEnumerable<DiscoveredApp> apps = _publishers.SelectedIndex > 0
            ? _suggestions[_publishers.SelectedIndex - 1].Apps
            : _all;

        var filter = _filter.Text.Trim();
        return filter.Length == 0
            ? apps
            : apps.Where(app => app.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase));
    }

    private void Repopulate()
    {
        _list.BeginUpdate();
        _list.ItemChecked -= OnItemChecked;
        _list.Items.Clear();
        _images.Images.Clear();

        foreach (var app in VisibleApps())
        {
            using var bitmap = IconLoader.Load(null, app.Target);
            var imageIndex = -1;
            if (bitmap is not null)
            {
                _images.Images.Add(bitmap);
                imageIndex = _images.Images.Count - 1;
            }

            var item = new ListViewItem(app.Name, imageIndex)
            {
                Tag = app,
                Checked = _checked.ContainsKey(app.Target),
            };
            item.SubItems.Add(app.Publisher ?? string.Empty);
            _list.Items.Add(item);
        }

        _list.ItemChecked += OnItemChecked;
        _list.EndUpdate();
    }

    private void OnItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        if (e.Item.Tag is not DiscoveredApp app)
        {
            return;
        }

        if (e.Item.Checked)
        {
            _checked[app.Target] = app;
        }
        else
        {
            _checked.Remove(app.Target);
        }
    }

    private void OnPublisherChanged()
    {
        if (_publishers.SelectedIndex > 0 && _newGroupName.Text.Length == 0)
        {
            _newGroupName.Text = _suggestions[_publishers.SelectedIndex - 1].Publisher;
        }

        Repopulate();
    }
}

