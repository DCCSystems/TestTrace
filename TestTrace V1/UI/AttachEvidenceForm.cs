using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class AttachEvidenceForm : Form
{
    private readonly TextBox filePathTextBox = new();
    private readonly ComboBox evidenceTypeComboBox = new();
    private readonly TextBox descriptionTextBox = new();

    public string SourceFilePath => filePathTextBox.Text.Trim();
    public EvidenceType EvidenceType => evidenceTypeComboBox.SelectedItem is EvidenceType evidenceType
        ? evidenceType
        : EvidenceType.Other;
    public string? Description => string.IsNullOrWhiteSpace(descriptionTextBox.Text) ? null : descriptionTextBox.Text.Trim();

    public AttachEvidenceForm(string testReference, string testTitle, EvidenceRequirements? requirements = null)
    {
        Text = $"Attach Evidence - {testReference}";
        MinimumSize = new Size(700, 360);
        StartPosition = FormStartPosition.CenterParent;
        InitializeLayout(testReference, testTitle, requirements);
        AppTheme.Apply(this);
    }

    private void InitializeLayout(string testReference, string testTitle, EvidenceRequirements? requirements)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 5,
            Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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

        AddLabel(layout, "Evidence type", 2);
        evidenceTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        evidenceTypeComboBox.Items.AddRange(Enum.GetValues<EvidenceType>().Cast<object>().ToArray());
        evidenceTypeComboBox.SelectedItem = SuggestedEvidenceType(requirements);
        evidenceTypeComboBox.Dock = DockStyle.Left;
        evidenceTypeComboBox.Width = 220;
        evidenceTypeComboBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(evidenceTypeComboBox, 1, 2);
        layout.SetColumnSpan(evidenceTypeComboBox, 2);

        AddLabel(layout, "Description", 3);
        descriptionTextBox.Dock = DockStyle.Fill;
        descriptionTextBox.Multiline = true;
        descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        descriptionTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(descriptionTextBox, 1, 3);
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
        layout.Controls.Add(actions, 0, 4);
        layout.SetColumnSpan(actions, 3);

        Controls.Add(layout);
    }

    private static EvidenceType SuggestedEvidenceType(EvidenceRequirements? requirements)
    {
        return requirements?.RequiredEvidenceTypes().FirstOrDefault() ?? EvidenceType.Other;
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
