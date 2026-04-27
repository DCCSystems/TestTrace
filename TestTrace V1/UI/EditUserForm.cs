using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class EditUserForm : Form
{
    private readonly TextBox displayNameTextBox = new();
    private readonly TextBox emailTextBox = new();
    private readonly TextBox phoneTextBox = new();
    private readonly TextBox organisationTextBox = new();
    private readonly bool isAdd;

    public string DisplayName => displayNameTextBox.Text.Trim();
    public string? Email => Trim(emailTextBox.Text);
    public string? Phone => Trim(phoneTextBox.Text);
    public string? Organisation => Trim(organisationTextBox.Text);

    public EditUserForm(UserAccount? existing)
    {
        isAdd = existing is null;
        Text = isAdd ? "Add User" : $"Edit User - {existing!.DisplayName}";
        MinimumSize = new Size(520, 360);
        StartPosition = FormStartPosition.CenterParent;

        if (existing is not null)
        {
            displayNameTextBox.Text = existing.DisplayName;
            emailTextBox.Text = existing.Email ?? string.Empty;
            phoneTextBox.Text = existing.Phone ?? string.Empty;
            organisationTextBox.Text = existing.Organisation ?? string.Empty;
        }

        InitializeLayout();
        AppTheme.Apply(this);
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 5; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddLabel(layout, "Display name", 0);
        SetupTextBox(displayNameTextBox);
        layout.Controls.Add(displayNameTextBox, 1, 0);

        AddLabel(layout, "Email", 1);
        SetupTextBox(emailTextBox);
        layout.Controls.Add(emailTextBox, 1, 1);

        AddLabel(layout, "Phone", 2);
        SetupTextBox(phoneTextBox);
        layout.Controls.Add(phoneTextBox, 1, 2);

        AddLabel(layout, "Organisation", 3);
        SetupTextBox(organisationTextBox);
        layout.Controls.Add(organisationTextBox, 1, 3);

        var hint = new Label
        {
            Text = isAdd
                ? "Adds a local TestTrace user. They can then be assigned authority roles such as Witness, Section Approver, or Test Executor."
                : "Updates this user's profile details across the project. The change is recorded in the audit log.",
            AutoSize = true,
            MaximumSize = new Size(360, 0),
            ForeColor = AppTheme.Current.TextSecondary,
            Margin = new Padding(0, 6, 0, 8)
        };
        layout.Controls.Add(hint, 1, 4);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var saveButton = new Button { Text = isAdd ? "Add User" : "Save Changes", AutoSize = true };
        saveButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(saveButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 5);
        layout.SetColumnSpan(actions, 2);

        Controls.Add(layout);
    }

    private void Accept()
    {
        if (string.IsNullOrWhiteSpace(displayNameTextBox.Text))
        {
            MessageBox.Show(this, "Display name is required.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private static void SetupTextBox(TextBox textBox)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 0, 0, 8);
    }

    private static void AddLabel(TableLayoutPanel layout, string text, int row)
    {
        layout.Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 12, 8)
        }, 0, row);
    }

    private static string? Trim(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
