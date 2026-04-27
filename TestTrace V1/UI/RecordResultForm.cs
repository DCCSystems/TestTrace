using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class RecordResultForm : Form
{
    private readonly ComboBox resultComboBox = new();
    private readonly TextBox measuredValueTextBox = new();
    private readonly TextBox commentsTextBox = new();

    public TestResult SelectedResult => (TestResult)resultComboBox.SelectedItem!;
    public string? MeasuredValue => string.IsNullOrWhiteSpace(measuredValueTextBox.Text) ? null : measuredValueTextBox.Text.Trim();
    public string? Comments => string.IsNullOrWhiteSpace(commentsTextBox.Text) ? null : commentsTextBox.Text.Trim();

    public RecordResultForm(string testReference, string testTitle)
    {
        Text = $"Record Result - {testReference}";
        MinimumSize = new Size(620, 430);
        StartPosition = FormStartPosition.CenterParent;
        InitializeLayout(testReference, testTitle);
        AppTheme.Apply(this);
        resultComboBox.SelectedItem = TestResult.Pass;
    }

    private void InitializeLayout(string testReference, string testTitle)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(16)
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
            Text = $"{testReference} - {testTitle}",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12)
        };
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);

        AddLabel(layout, "Result", 1);
        resultComboBox.Dock = DockStyle.Left;
        resultComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        resultComboBox.Width = 220;
        resultComboBox.Margin = new Padding(0, 0, 0, 8);
        resultComboBox.Items.AddRange([TestResult.Pass, TestResult.Fail]);
        layout.Controls.Add(resultComboBox, 1, 1);

        AddLabel(layout, "Measured value", 2);
        measuredValueTextBox.Dock = DockStyle.Fill;
        measuredValueTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(measuredValueTextBox, 1, 2);

        AddLabel(layout, "Observation", 3);
        commentsTextBox.Dock = DockStyle.Fill;
        commentsTextBox.Multiline = true;
        commentsTextBox.ScrollBars = ScrollBars.Vertical;
        commentsTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(commentsTextBox, 1, 3);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var saveButton = new Button { Text = "Record Result", AutoSize = true };
        saveButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(saveButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 4);
        layout.SetColumnSpan(actions, 2);

        Controls.Add(layout);
    }

    private void Accept()
    {
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
