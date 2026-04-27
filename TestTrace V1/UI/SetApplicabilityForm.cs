using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class SetApplicabilityForm : Form
{
    private readonly TextBox reasonTextBox = new();
    private readonly TextBox validationTextBox = new();
    private readonly ApplicabilityState targetState;

    public string? Reason => string.IsNullOrWhiteSpace(reasonTextBox.Text) ? null : reasonTextBox.Text.Trim();

    public SetApplicabilityForm(string targetLabel, ApplicabilityState targetState)
    {
        this.targetState = targetState;
        Text = targetState == ApplicabilityState.NotApplicable ? "Mark Not Applicable" : "Return to Scope";
        MinimumSize = new Size(600, targetState == ApplicabilityState.NotApplicable ? 380 : 250);
        StartPosition = FormStartPosition.CenterParent;
        InitializeLayout(targetLabel);
        AppTheme.Apply(this);
    }

    private void InitializeLayout(string targetLabel)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(16)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label
        {
            Text = targetLabel,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 10)
        };
        layout.Controls.Add(heading, 0, 0);

        var message = new Label
        {
            Text = targetState == ApplicabilityState.NotApplicable
                ? "Explain why this item is not relevant to this machine/project scope."
                : "This will return the item to the applicable execution scope.",
            AutoSize = true,
            MaximumSize = new Size(540, 0),
            Margin = new Padding(0, 0, 0, 10)
        };
        layout.Controls.Add(message, 0, 1);

        if (targetState == ApplicabilityState.NotApplicable)
        {
            reasonTextBox.Dock = DockStyle.Fill;
            reasonTextBox.Multiline = true;
            reasonTextBox.ScrollBars = ScrollBars.Vertical;
            reasonTextBox.Margin = new Padding(0, 0, 0, 10);
            layout.Controls.Add(reasonTextBox, 0, 2);
        }

        validationTextBox.Dock = DockStyle.Fill;
        validationTextBox.Multiline = true;
        validationTextBox.ReadOnly = true;
        validationTextBox.BackColor = AppTheme.Current.InputReadOnlyBackground;
        validationTextBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(validationTextBox, 0, 3);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var saveButton = new Button
        {
            Text = targetState == ApplicabilityState.NotApplicable ? "Mark Not Applicable" : "Return to Scope",
            AutoSize = true
        };
        saveButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(saveButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 4);

        Controls.Add(layout);
    }

    private void Accept()
    {
        validationTextBox.Clear();
        if (targetState == ApplicabilityState.NotApplicable && string.IsNullOrWhiteSpace(reasonTextBox.Text))
        {
            validationTextBox.Text = "A reason is required when marking an item not applicable.";
            return;
        }

        DialogResult = DialogResult.OK;
    }
}
