namespace TestTrace_V1.UI;

public sealed class ApproveSectionForm : Form
{
    private readonly TextBox approvedByTextBox = new();
    private readonly TextBox commentsTextBox = new();

    public string ApprovedBy => approvedByTextBox.Text.Trim();
    public string? Comments => string.IsNullOrWhiteSpace(commentsTextBox.Text) ? null : commentsTextBox.Text.Trim();

    public ApproveSectionForm(string sectionTitle, string defaultApprover)
    {
        Text = "Approve Section";
        MinimumSize = new Size(620, 380);
        StartPosition = FormStartPosition.CenterParent;
        approvedByTextBox.Text = defaultApprover;
                InitializeLayout(sectionTitle);
        AppTheme.Apply(this);
    }

    private void InitializeLayout(string sectionTitle)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(18)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label
        {
            Text = sectionTitle,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 16)
        };
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);

        AddLabel(layout, "Approved by", 1);
        approvedByTextBox.Dock = DockStyle.Fill;
        approvedByTextBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(approvedByTextBox, 1, 1);

        AddLabel(layout, "Comments", 2);
        commentsTextBox.Dock = DockStyle.Fill;
        commentsTextBox.Multiline = true;
        commentsTextBox.ScrollBars = ScrollBars.Vertical;
        commentsTextBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(commentsTextBox, 1, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var approveButton = new Button { Text = "Approve Section", AutoSize = true };
        approveButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(approveButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 3);
        layout.SetColumnSpan(actions, 2);

        Controls.Add(layout);
    }

    private void Accept()
    {
        if (string.IsNullOrWhiteSpace(approvedByTextBox.Text))
        {
            MessageBox.Show(this, "Approved by is required.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
