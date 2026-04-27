namespace TestTrace_V1.UI;

public sealed class ReleaseProjectForm : Form
{
    private readonly TextBox releasedByTextBox = new();
    private readonly TextBox declarationTextBox = new();

    public string ReleasedBy => releasedByTextBox.Text.Trim();
    public string Declaration => declarationTextBox.Text.Trim();

    public ReleaseProjectForm(string projectTitle, string releaseAuthority)
    {
        Text = "Release Project";
        MinimumSize = new Size(680, 420);
        StartPosition = FormStartPosition.CenterParent;
        releasedByTextBox.Text = releaseAuthority;
        declarationTextBox.Text = "I confirm this TestTrace project is complete and ready for release.";
                InitializeLayout(projectTitle);
        AppTheme.Apply(this);
    }

    private void InitializeLayout(string projectTitle)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(18)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label
        {
            Text = projectTitle,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 10)
        };
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);

        var warning = new Label
        {
            Text = "Release closes the project. The releaser must match the contract release authority.",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 16)
        };
        layout.Controls.Add(warning, 0, 1);
        layout.SetColumnSpan(warning, 2);

        AddLabel(layout, "Released by", 2);
        releasedByTextBox.Dock = DockStyle.Fill;
        releasedByTextBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(releasedByTextBox, 1, 2);

        AddLabel(layout, "Declaration", 3);
        declarationTextBox.Dock = DockStyle.Fill;
        declarationTextBox.Multiline = true;
        declarationTextBox.ScrollBars = ScrollBars.Vertical;
        declarationTextBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(declarationTextBox, 1, 3);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var releaseButton = new Button { Text = "Release Project", AutoSize = true };
        releaseButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(releaseButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 4);
        layout.SetColumnSpan(actions, 2);

        Controls.Add(layout);
    }

    private void Accept()
    {
        if (string.IsNullOrWhiteSpace(releasedByTextBox.Text))
        {
            MessageBox.Show(this, "Released by is required.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(declarationTextBox.Text))
        {
            MessageBox.Show(this, "Release declaration is required.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            Margin = new Padding(0, 4, 12, 10)
        }, 0, row);
    }
}
