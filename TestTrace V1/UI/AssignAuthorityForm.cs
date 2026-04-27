using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class AssignAuthorityForm : Form
{
    private readonly TestTraceProject project;
    private readonly ComboBox userCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox roleCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox scopeTypeCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox scopeTargetCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox reasonTextBox = new();
    private readonly Label scopeTargetLabel = new();

    public Guid SelectedUserId { get; private set; }
    public AuthorityRole SelectedRole { get; private set; }
    public AuthorityScopeType SelectedScopeType { get; private set; }
    public Guid SelectedScopeId { get; private set; }
    public string? Reason => string.IsNullOrWhiteSpace(reasonTextBox.Text) ? null : reasonTextBox.Text.Trim();

    public AssignAuthorityForm(TestTraceProject project)
    {
        this.project = project;
        Text = "Assign Authority";
        MinimumSize = new Size(560, 400);
        StartPosition = FormStartPosition.CenterParent;

        InitializeLayout();
        AppTheme.Apply(this);
        PopulateUsers();
        PopulateRoles();
        PopulateScopeTypes();
        ScopeTypeChanged();
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 6; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddLabel(layout, "User", 0);
        userCombo.Dock = DockStyle.Fill;
        userCombo.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(userCombo, 1, 0);

        AddLabel(layout, "Role", 1);
        roleCombo.Dock = DockStyle.Fill;
        roleCombo.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(roleCombo, 1, 1);

        AddLabel(layout, "Scope type", 2);
        scopeTypeCombo.Dock = DockStyle.Fill;
        scopeTypeCombo.Margin = new Padding(0, 0, 0, 8);
        scopeTypeCombo.SelectedIndexChanged += (_, _) => ScopeTypeChanged();
        layout.Controls.Add(scopeTypeCombo, 1, 2);

        scopeTargetLabel.Text = "Scope target";
        scopeTargetLabel.AutoSize = true;
        scopeTargetLabel.Anchor = AnchorStyles.Left;
        scopeTargetLabel.Margin = new Padding(0, 6, 12, 8);
        layout.Controls.Add(scopeTargetLabel, 0, 3);
        scopeTargetCombo.Dock = DockStyle.Fill;
        scopeTargetCombo.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(scopeTargetCombo, 1, 3);

        AddLabel(layout, "Reason", 4);
        reasonTextBox.Dock = DockStyle.Fill;
        reasonTextBox.Multiline = true;
        reasonTextBox.Height = 60;
        reasonTextBox.ScrollBars = ScrollBars.Vertical;
        reasonTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(reasonTextBox, 1, 4);

        var hint = new Label
        {
            Text = "An authority assignment binds a user to a role within a scope (Project, Section, or Test). Assignments are permanent until revoked. Witness assignments are typically scoped to a specific test.",
            AutoSize = true,
            MaximumSize = new Size(380, 0),
            ForeColor = AppTheme.Current.TextSecondary,
            Margin = new Padding(0, 0, 0, 12)
        };
        layout.Controls.Add(hint, 1, 5);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var assignButton = new Button { Text = "Assign Authority", AutoSize = true };
        assignButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(assignButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 6);
        layout.SetColumnSpan(actions, 2);

        Controls.Add(layout);
    }

    private void PopulateUsers()
    {
        var activeUsers = project.Users.Where(user => user.IsActive).OrderBy(user => user.DisplayName).ToList();
        userCombo.DataSource = activeUsers;
        userCombo.DisplayMember = nameof(UserAccount.DisplayName);
    }

    private void PopulateRoles()
    {
        roleCombo.DataSource = Enum.GetValues<AuthorityRole>();
    }

    private void PopulateScopeTypes()
    {
        scopeTypeCombo.DataSource = new[]
        {
            AuthorityScopeType.Project,
            AuthorityScopeType.Section,
            AuthorityScopeType.TestItem
        };
    }

    private void ScopeTypeChanged()
    {
        if (scopeTypeCombo.SelectedItem is not AuthorityScopeType scopeType)
        {
            return;
        }

        switch (scopeType)
        {
            case AuthorityScopeType.Project:
                scopeTargetLabel.Text = "Project";
                scopeTargetCombo.DataSource = new[]
                {
                    new ScopeTarget(project.ProjectId, project.ContractRoot.ProjectCode + " - " + project.ContractRoot.ProjectName)
                };
                scopeTargetCombo.DisplayMember = nameof(ScopeTarget.Label);
                break;
            case AuthorityScopeType.Section:
                scopeTargetLabel.Text = "Section";
                scopeTargetCombo.DataSource = project.Sections
                    .OrderBy(section => section.DisplayOrder)
                    .Select(section => new ScopeTarget(section.SectionId, section.Title))
                    .ToList();
                scopeTargetCombo.DisplayMember = nameof(ScopeTarget.Label);
                break;
            case AuthorityScopeType.TestItem:
                scopeTargetLabel.Text = "Test item";
                scopeTargetCombo.DataSource = project.Sections
                    .SelectMany(section => section.TestItems
                        .Select(testItem => new ScopeTarget(testItem.TestItemId, $"{section.Title} | {testItem.TestReference} - {testItem.TestTitle}")))
                    .ToList();
                scopeTargetCombo.DisplayMember = nameof(ScopeTarget.Label);
                break;
        }
    }

    private void Accept()
    {
        if (userCombo.SelectedItem is not UserAccount selectedUser)
        {
            MessageBox.Show(this, "Pick a user.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (roleCombo.SelectedItem is not AuthorityRole selectedRole)
        {
            MessageBox.Show(this, "Pick a role.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (scopeTypeCombo.SelectedItem is not AuthorityScopeType selectedScopeType)
        {
            MessageBox.Show(this, "Pick a scope type.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (scopeTargetCombo.SelectedItem is not ScopeTarget selectedScope || selectedScope.Id == Guid.Empty)
        {
            MessageBox.Show(this, "Pick a scope target.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SelectedUserId = selectedUser.UserId;
        SelectedRole = selectedRole;
        SelectedScopeType = selectedScopeType;
        SelectedScopeId = selectedScope.Id;
        DialogResult = DialogResult.OK;
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

    private sealed record ScopeTarget(Guid Id, string Label);
}
