namespace TestTrace_V1.UI;

public sealed class AttachEvidenceForm : Form
{
    private readonly TextBox filePathTextBox = new();
    private readonly TextBox descriptionTextBox = new();

    public string SourceFilePath => filePathTextBox.Text.Trim();
    public string? Description => string.IsNullOrWhiteSpace(descriptionTextBox.Text) ? null : descriptionTextBox.Text.Trim();

    public AttachEvidenceForm(string testReference, string testTitle)
    {
        Text = $"Attach Evidence - {testReference}";
        MinimumSize = new Size(700, 360);
        StartPosition = FormStartPosition.CenterParent;
                InitializeLayout(testReference, testTitle);
        AppTheme.Apply(this);
    }

    private void InitializeLayout(string testReference, string testTitle)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4,
            Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label
        {
            Text = $"{testReference} - {testTitle}",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12)
        };
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 3);

        AddLabel(layout, "Evidence file", 1);
        filePathTextBox.Dock = DockStyle.Fill;
        filePathTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(filePathTextBox, 1, 1);
        var browseButton = new Button { Text = "Browse", AutoSize = true, Margin = new Padding(8, 0, 0, 8) };
        browseButton.Click += (_, _) => BrowseFile();
        layout.Controls.Add(browseButton, 2, 1);

        AddLabel(layout, "Description", 2);
        descriptionTextBox.Dock = DockStyle.Fill;
        descriptionTextBox.Multiline = true;
        descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        descriptionTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(descriptionTextBox, 1, 2);
        layout.SetColumnSpan(descriptionTextBox, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var attachButton = new Button { Text = "Attach Evidence", AutoSize = true };
        attachButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(attachButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 3);
        layout.SetColumnSpan(actions, 3);

        Controls.Add(layout);
    }

    private void BrowseFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Choose evidence file",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            filePathTextBox.Text = dialog.FileName;
        }
    }

    private void Accept()
    {
        if (string.IsNullOrWhiteSpace(filePathTextBox.Text) || !File.Exists(filePathTextBox.Text.Trim()))
        {
            MessageBox.Show(this, "Choose an existing evidence file.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private static void AddLabel(TableLayoutPanel layout, string text, int row)
    {
        layout.Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 12, 8)
        }, 0, row);
    }
}
