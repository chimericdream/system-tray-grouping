namespace TrayGroups.Host.Settings;

/// <summary>A minimal "enter a line of text" dialog.</summary>
internal sealed class PromptForm : Form
{
    private readonly TextBox _input = new() { Dock = DockStyle.Top };

    private PromptForm(string title, string label, string initial)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(360, 110);

        _input.Text = initial;

        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
        AcceptButton = ok;
        CancelButton = cancel;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 40,
        };
        buttons.Controls.AddRange([cancel, ok]);

        var body = new Padding(12);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = body, RowCount = 2, ColumnCount = 1 };
        layout.Controls.Add(new Label { Text = label, AutoSize = true }, 0, 0);
        layout.Controls.Add(_input, 0, 1);

        Controls.Add(layout);
        Controls.Add(buttons);
    }

    /// <summary>Returns the trimmed text, or null if cancelled or left empty.</summary>
    public static string? Ask(IWin32Window owner, string title, string label, string initial = "")
    {
        using var form = new PromptForm(title, label, initial);
        if (form.ShowDialog(owner) != DialogResult.OK)
        {
            return null;
        }

        var value = form._input.Text.Trim();
        return value.Length == 0 ? null : value;
    }
}
