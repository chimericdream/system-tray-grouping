using TrayGroups.Core.Configuration;
using TrayGroups.Core.Launching;

namespace TrayGroups.Host.Settings;

/// <summary>Edits one menu entry: name, target, arguments, and optional icon override.</summary>
internal sealed class EntryEditorForm : Form
{
    private readonly TextBox _name = new() { Dock = DockStyle.Fill };
    private readonly TextBox _target = new() { Dock = DockStyle.Fill };
    private readonly TextBox _arguments = new() { Dock = DockStyle.Fill };
    private readonly TextBox _icon = new() { Dock = DockStyle.Fill };

    private EntryEditorForm(EntryConfig entry)
    {
        Text = "Edit app";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(520, 210);

        _name.Text = entry.Name;
        _target.Text = entry.Target;
        _arguments.Text = entry.Arguments ?? string.Empty;
        _icon.Text = entry.IconPath ?? string.Empty;

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 3,
            RowCount = 4,
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        AddRow(grid, 0, "Name", _name, null);
        AddRow(grid, 1, "Target", _target, BrowseButton(_target, "Programs and shortcuts|*.exe;*.lnk;*.bat;*.cmd|All files|*.*"));
        AddRow(grid, 2, "Arguments", _arguments, null);
        AddRow(grid, 3, "Icon (optional)", _icon, BrowseButton(_icon, "Images and icons|*.ico;*.png;*.jpg;*.bmp;*.exe|All files|*.*"));

        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        AcceptButton = ok;
        CancelButton = cancel;
        ok.Click += OnOkClick;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 44,
            Padding = new Padding(8),
        };
        buttons.Controls.AddRange([cancel, ok]);

        Controls.Add(grid);
        Controls.Add(buttons);
    }

    public static EntryConfig? Edit(IWin32Window owner, EntryConfig entry)
    {
        using var form = new EntryEditorForm(entry);
        if (form.ShowDialog(owner) != DialogResult.OK)
        {
            return null;
        }

        return entry with
        {
            Name = form._name.Text.Trim(),
            Target = form._target.Text.Trim(),
            Arguments = NullIfBlank(form._arguments.Text),
            IconPath = NullIfBlank(form._icon.Text),
        };
    }

    private static string? NullIfBlank(string text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static void AddRow(TableLayoutPanel grid, int row, string label, Control field, Control? extra)
    {
        grid.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 8, 6) }, 0, row);
        grid.Controls.Add(field, 1, row);
        if (extra is not null)
        {
            grid.Controls.Add(extra, 2, row);
        }
    }

    private Button BrowseButton(TextBox target, string filter)
    {
        var button = new Button { Text = "Browse…", AutoSize = true };
        button.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog { Filter = filter, CheckFileExists = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                target.Text = PathPortability.Collapse(dialog.FileName);
            }
        };
        return button;
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        if (_name.Text.Trim().Length == 0 || _target.Text.Trim().Length == 0)
        {
            MessageBox.Show(this, "Name and target are required.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
    }
}
