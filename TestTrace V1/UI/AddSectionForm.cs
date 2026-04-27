namespace TestTrace_V1.UI;

public sealed class AddSectionForm : Form
{
    private readonly TextBox titleTextBox = new();
    private readonly TextBox descriptionTextBox = new();
    private readonly TextBox approverTextBox = new();

    public string SectionTitle => titleTextBox.Text.Trim();
    public string? Description => string.IsNullOrWhiteSpace(descriptionTextBox.Text) ? null : descriptionTextBox.Text.Trim();
    public string? SectionApprover => string.IsNullOrWhiteSpace(approverTextBox.Text) ? null : approverTextBox.Text.Trim();

    public AddSectionForm(string? defaultApprover)
    {
        Text = "Add Section";
        MinimumSize = new Size(560, 360);
        StartPosition = FormStartPosition.CenterParent;
        approverTextBox.Text = defaultApprover ?? string.Empty;
                InitializeLayout();
        AppTheme.Apply(this);
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddLabel(layout, "Title", 0);
        titleTextBox.Dock = DockStyle.Fill;
        titleTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(titleTextBox, 1, 0);

        AddLabel(layout, "Description", 1);
        descriptionTextBox.Dock = DockStyle.Fill;
        descriptionTextBox.Multiline = true;
        descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        descriptionTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(descriptionTextBox, 1, 1);

        AddLabel(layout, "Approver", 2);
        approverTextBox.Dock = DockStyle.Fill;
        approverTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(approverTextBox, 1, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var addButton = new Button { Text = "Add Section", AutoSize = true };
        addButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(addButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 3);
        layout.SetColumnSpan(actions, 2);

        Controls.Add(layout);
    }

    private void Accept()
    {
        if (string.IsNullOrWhiteSpace(titleTextBox.Text))
        {
            MessageBox.Show(this, "Section title is required.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
