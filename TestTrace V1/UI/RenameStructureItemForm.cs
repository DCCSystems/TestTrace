namespace TestTrace_V1.UI;

public sealed class RenameStructureItemForm : Form
{
    private readonly TextBox primaryTextBox = new();
    private readonly TextBox secondaryTextBox = new();
    private readonly TextBox validationTextBox = new();
    private readonly bool hasSecondaryValue;

    public string PrimaryValue => primaryTextBox.Text.Trim();
    public string SecondaryValue => secondaryTextBox.Text.Trim();

    public RenameStructureItemForm(
        string title,
        string primaryLabel,
        string primaryValue,
        string? secondaryLabel = null,
        string? secondaryValue = null,
        string? note = null)
    {
        Text = title;
        MinimumSize = new Size(520, secondaryLabel is null ? 250 : 320);
        StartPosition = FormStartPosition.CenterParent;
        hasSecondaryValue = !string.IsNullOrWhiteSpace(secondaryLabel);

                InitializeLayout(primaryLabel, primaryValue, secondaryLabel, secondaryValue, note);
        AppTheme.Apply(this);
    }

    private void InitializeLayout(
        string primaryLabel,
        string primaryValue,
        string? secondaryLabel,
        string? secondaryValue,
        string? note)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(14)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddTextRow(layout, 0, primaryLabel, primaryTextBox, primaryValue);

        if (hasSecondaryValue)
        {
            AddTextRow(layout, 1, secondaryLabel ?? string.Empty, secondaryTextBox, secondaryValue ?? string.Empty);
        }

        var noteLabel = new Label
        {
            Text = string.IsNullOrWhiteSpace(note) ? "This change will be recorded in the project audit trail." : note,
            AutoSize = true,
            MaximumSize = new Size(460, 0),
            Margin = new Padding(0, 10, 0, 8)
        };
        layout.Controls.Add(noteLabel, 0, 2);
        layout.SetColumnSpan(noteLabel, 2);

        validationTextBox.Dock = DockStyle.Fill;
        validationTextBox.Multiline = true;
        validationTextBox.ReadOnly = true;
        validationTextBox.BackColor = AppTheme.Current.InputReadOnlyBackground;
        validationTextBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(validationTextBox, 0, 3);
        layout.SetColumnSpan(validationTextBox, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };

        var saveButton = new Button { Text = "Rename", AutoSize = true };
        saveButton.Click += (_, _) => TryAccept();

        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        actions.Controls.Add(saveButton);
        actions.Controls.Add(cancelButton);

        layout.Controls.Add(actions, 0, 4);
        layout.SetColumnSpan(actions, 2);

        Controls.Add(layout);
    }

    private static void AddTextRow(TableLayoutPanel layout, int row, string labelText, TextBox textBox, string value)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 12, 10)
        };

        textBox.Text = value;
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 0, 0, 10);

        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(textBox, 1, row);
    }

    private void TryAccept()
    {
        validationTextBox.Clear();
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(primaryTextBox.Text))
        {
            issues.Add("The first field is required.");
        }

        if (hasSecondaryValue && string.IsNullOrWhiteSpace(secondaryTextBox.Text))
        {
            issues.Add("The second field is required.");
        }

        if (issues.Count > 0)
        {
            validationTextBox.Text = string.Join(Environment.NewLine, issues);
            return;
        }

        DialogResult = DialogResult.OK;
    }
}
