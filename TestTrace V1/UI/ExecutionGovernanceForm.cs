using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class ExecutionGovernanceForm : Form
{
    private readonly TextBox witnessTextBox = new();
    private readonly TextBox overrideReasonTextBox = new();
    private readonly TextBox validationTextBox = new();
    private readonly TestItem testItem;
    private readonly TestResult result;
    private readonly string currentActor;

    public string? WitnessedBy => string.IsNullOrWhiteSpace(witnessTextBox.Text) ? null : witnessTextBox.Text.Trim();
    public string? OverrideReason => string.IsNullOrWhiteSpace(overrideReasonTextBox.Text) ? null : overrideReasonTextBox.Text.Trim();

    public static bool IsRequiredFor(TestItem testItem, TestResult result)
    {
        return testItem.BehaviourRules.RequiresWitness ||
            (result == TestResult.Fail && testItem.BehaviourRules.AllowOverrideWithReason);
    }

    public ExecutionGovernanceForm(
        TestItem testItem,
        TestResult result,
        string currentActor,
        string? initialWitness = null,
        string? initialOverrideReason = null)
    {
        this.testItem = testItem;
        this.result = result;
        this.currentActor = currentActor.Trim();

        Text = "Execution Governance";
        MinimumSize = new Size(620, 420);
        StartPosition = FormStartPosition.CenterParent;

        InitializeLayout(initialWitness, initialOverrideReason);
        AppTheme.Apply(this);
    }

    private void InitializeLayout(string? initialWitness, string? initialOverrideReason)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(18)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            Text = $"{testItem.TestReference} - {testItem.TestTitle}",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 13, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 6)
        }, 0, 0);

        layout.Controls.Add(new Label
        {
            Text = result == TestResult.Fail
                ? "This result needs additional governed context before it is recorded."
                : "This test requires witnessed execution before the result is recorded.",
            AutoSize = true,
            ForeColor = AppTheme.Current.TextSecondary,
            MaximumSize = new Size(560, 0),
            Margin = new Padding(0, 0, 0, 14)
        }, 0, 1);

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var row = 0;
        if (testItem.BehaviourRules.RequiresWitness)
        {
            fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            AddField(fields, row++, "Witnessed by *", witnessTextBox, initialWitness);
        }

        if (result == TestResult.Fail && testItem.BehaviourRules.AllowOverrideWithReason)
        {
            fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            AddMultilineField(
                fields,
                row++,
                "Override reason",
                overrideReasonTextBox,
                initialOverrideReason,
                "Optional. If supplied, this failed result is recorded as an accepted override for progression.");
        }

        layout.Controls.Add(fields, 0, 2);

        validationTextBox.Dock = DockStyle.Fill;
        validationTextBox.Multiline = true;
        validationTextBox.ReadOnly = true;
        validationTextBox.BackColor = AppTheme.Current.InputReadOnlyBackground;
        validationTextBox.Margin = new Padding(0, 12, 0, 10);
        layout.Controls.Add(validationTextBox, 0, 4);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var recordButton = new Button { Text = "Continue", AutoSize = true };
        AppTheme.StyleButton(recordButton, ThemeButtonKind.Primary);
        recordButton.Click += (_, _) => TryAccept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(recordButton);
        actions.Controls.Add(cancelButton);

        layout.Controls.Add(actions, 0, 5);
        Controls.Add(layout);
    }

    private static void AddField(TableLayoutPanel layout, int row, string labelText, TextBox textBox, string? value)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 12, 12)
        };
        textBox.Text = value ?? string.Empty;
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 0, 0, 12);
        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(textBox, 1, row);
    }

    private static void AddMultilineField(
        TableLayoutPanel layout,
        int row,
        string labelText,
        TextBox textBox,
        string? value,
        string helpText)
    {
        var labelPanel = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, RowCount = 2 };
        labelPanel.Controls.Add(new Label
        {
            Text = labelText,
            AutoSize = true,
            Margin = new Padding(0, 4, 12, 4)
        }, 0, 0);
        labelPanel.Controls.Add(new Label
        {
            Text = helpText,
            AutoSize = true,
            ForeColor = AppTheme.Current.TextMuted,
            MaximumSize = new Size(145, 0),
            Margin = new Padding(0, 0, 12, 12)
        }, 0, 1);

        textBox.Text = value ?? string.Empty;
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.Vertical;
        textBox.MinimumSize = new Size(0, 110);
        textBox.Margin = new Padding(0, 0, 0, 12);

        layout.Controls.Add(labelPanel, 0, row);
        layout.Controls.Add(textBox, 1, row);
    }

    private void TryAccept()
    {
        validationTextBox.Clear();
        var issues = new List<string>();

        if (testItem.BehaviourRules.RequiresWitness)
        {
            if (string.IsNullOrWhiteSpace(witnessTextBox.Text))
            {
                issues.Add("Witnessed by is required for this test.");
            }
            else if (string.Equals(witnessTextBox.Text.Trim(), currentActor, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add("Witness must be different from the test executor.");
            }
        }

        if (issues.Count > 0)
        {
            validationTextBox.Text = string.Join(Environment.NewLine, issues);
            return;
        }

        DialogResult = DialogResult.OK;
    }
}
