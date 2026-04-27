using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;
using TestTrace_V1.Workspace;

namespace TestTrace_V1.UI;

public sealed class UsersAuthorityForm : Form
{
    private readonly string projectFolderPath;
    private readonly Func<string> currentActor;
    private readonly JsonProjectRepository repository = new();
    private readonly UsersAuthorityService usersService;

    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly ListView usersList = new();
    private readonly ListView authorityList = new();
    private readonly Label statusLabel = new();
    private readonly Button addUserButton = new() { Text = "Add User...", AutoSize = true };
    private readonly Button editUserButton = new() { Text = "Edit...", AutoSize = true, Enabled = false, Margin = new Padding(8, 0, 0, 0) };
    private readonly Button deactivateUserButton = new() { Text = "Deactivate", AutoSize = true, Enabled = false, Margin = new Padding(8, 0, 0, 0) };
    private readonly Button reactivateUserButton = new() { Text = "Reactivate", AutoSize = true, Enabled = false, Margin = new Padding(8, 0, 0, 0) };
    private readonly Button assignAuthorityButton = new() { Text = "Assign Authority...", AutoSize = true };
    private readonly Button revokeAuthorityButton = new() { Text = "Revoke...", AutoSize = true, Enabled = false, Margin = new Padding(8, 0, 0, 0) };

    private TestTraceProject? project;

    public UsersAuthorityForm(string projectFolderPath, Func<string> currentActor)
    {
        this.projectFolderPath = projectFolderPath;
        this.currentActor = currentActor;
        usersService = new UsersAuthorityService(repository);

        Text = "Users & Authority";
        MinimumSize = new Size(960, 620);
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;

        InitializeLayout();
        AppTheme.Apply(this);
        LoadProject();
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };
        var title = new Label
        {
            Text = "Local Users & Authority Assignments",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 4),
            UseMnemonic = false
        };
        var subtitle = new Label
        {
            Text = "Manage local TestTrace users and the roles they hold within this project. Every change here is recorded in the audit log.",
            AutoSize = true,
            ForeColor = AppTheme.Current.TextSecondary,
            MaximumSize = new Size(820, 0),
            Margin = new Padding(0)
        };
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(subtitle, 0, 1);
        layout.Controls.Add(header, 0, 0);

        tabs.TabPages.Add(BuildUsersTab());
        tabs.TabPages.Add(BuildAuthorityTab());
        layout.Controls.Add(tabs, 0, 1);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        statusLabel.AutoSize = true;
        statusLabel.ForeColor = AppTheme.Current.TextSecondary;
        statusLabel.Margin = new Padding(0, 6, 0, 0);
        var closeButton = new Button { Text = "Close", AutoSize = true };
        closeButton.Click += (_, _) => DialogResult = DialogResult.OK;
        footer.Controls.Add(statusLabel, 0, 0);
        footer.Controls.Add(closeButton, 1, 0);
        layout.Controls.Add(footer, 0, 2);

        Controls.Add(layout);
    }

    private TabPage BuildUsersTab()
    {
        var page = new TabPage("Users");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };
        addUserButton.Click += (_, _) => AddUser();
        editUserButton.Click += (_, _) => EditUser();
        deactivateUserButton.Click += (_, _) => DeactivateUser();
        reactivateUserButton.Click += (_, _) => ReactivateUser();
        toolbar.Controls.Add(addUserButton);
        toolbar.Controls.Add(editUserButton);
        toolbar.Controls.Add(deactivateUserButton);
        toolbar.Controls.Add(reactivateUserButton);
        panel.Controls.Add(toolbar, 0, 0);

        usersList.Dock = DockStyle.Fill;
        usersList.View = View.Details;
        usersList.FullRowSelect = true;
        usersList.HideSelection = false;
        usersList.MultiSelect = false;
        usersList.Columns.Add("Display name", 200);
        usersList.Columns.Add("Initials", 70);
        usersList.Columns.Add("Email", 200);
        usersList.Columns.Add("Phone", 130);
        usersList.Columns.Add("Organisation", 140);
        usersList.Columns.Add("Status", 110);
        usersList.SelectedIndexChanged += (_, _) => UpdateUserButtons();
        panel.Controls.Add(usersList, 0, 1);

        page.Controls.Add(panel);
        return page;
    }

    private TabPage BuildAuthorityTab()
    {
        var page = new TabPage("Authority");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };
        assignAuthorityButton.Click += (_, _) => AssignAuthority();
        revokeAuthorityButton.Click += (_, _) => RevokeAuthority();
        toolbar.Controls.Add(assignAuthorityButton);
        toolbar.Controls.Add(revokeAuthorityButton);
        panel.Controls.Add(toolbar, 0, 0);

        authorityList.Dock = DockStyle.Fill;
        authorityList.View = View.Details;
        authorityList.FullRowSelect = true;
        authorityList.HideSelection = false;
        authorityList.MultiSelect = false;
        authorityList.Columns.Add("User", 180);
        authorityList.Columns.Add("Role", 160);
        authorityList.Columns.Add("Scope", 280);
        authorityList.Columns.Add("Assigned by", 130);
        authorityList.Columns.Add("Status", 110);
        authorityList.SelectedIndexChanged += (_, _) => UpdateAuthorityButtons();
        panel.Controls.Add(authorityList, 0, 1);

        page.Controls.Add(panel);
        return page;
    }

    private void LoadProject()
    {
        try
        {
            project = repository.Load(ProjectLocation.FromProjectFolder(projectFolderPath));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not load project: {ex.Message}", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Error);
            DialogResult = DialogResult.Cancel;
            return;
        }

        RenderUsers();
        RenderAuthority();
        UpdateUserButtons();
        UpdateAuthorityButtons();
    }

    private void RenderUsers()
    {
        usersList.BeginUpdate();
        usersList.Items.Clear();
        if (project is null)
        {
            usersList.EndUpdate();
            return;
        }

        foreach (var user in project.Users.OrderByDescending(user => user.IsActive).ThenBy(user => user.DisplayName))
        {
            var item = new ListViewItem(user.DisplayName);
            item.SubItems.Add(user.Initials);
            item.SubItems.Add(user.Email ?? "-");
            item.SubItems.Add(user.Phone ?? "-");
            item.SubItems.Add(user.Organisation ?? "-");
            item.SubItems.Add(user.IsActive ? "Active" : $"Inactive ({user.DeactivatedReason})");
            item.Tag = user;
            if (!user.IsActive)
            {
                item.ForeColor = AppTheme.Current.TextMuted;
            }
            usersList.Items.Add(item);
        }
        usersList.EndUpdate();
        statusLabel.Text = $"{project.Users.Count(u => u.IsActive)} active users / {project.Users.Count(u => !u.IsActive)} inactive / {project.AuthorityAssignments.Count(a => a.IsActive)} active authority assignments.";
    }

    private void RenderAuthority()
    {
        authorityList.BeginUpdate();
        authorityList.Items.Clear();
        if (project is null)
        {
            authorityList.EndUpdate();
            return;
        }

        var ordered = project.AuthorityAssignments
            .OrderByDescending(assignment => assignment.IsActive)
            .ThenBy(assignment => assignment.Role)
            .ThenBy(assignment => assignment.DisplayNameSnapshot);

        foreach (var assignment in ordered)
        {
            var item = new ListViewItem(assignment.DisplayNameSnapshot);
            item.SubItems.Add(assignment.Role.ToString());
            item.SubItems.Add(project.ScopeLabel(assignment.ScopeType, assignment.ScopeId));
            item.SubItems.Add(assignment.AssignedBy);
            item.SubItems.Add(assignment.IsActive ? "Active" : $"Revoked ({assignment.RevokedReason})");
            item.Tag = assignment;
            if (!assignment.IsActive)
            {
                item.ForeColor = AppTheme.Current.TextMuted;
            }
            authorityList.Items.Add(item);
        }
        authorityList.EndUpdate();
    }

    private void UpdateUserButtons()
    {
        var selectedUser = SelectedUser();
        editUserButton.Enabled = selectedUser is not null;
        deactivateUserButton.Enabled = selectedUser is { IsActive: true };
        reactivateUserButton.Enabled = selectedUser is { IsActive: false };
    }

    private void UpdateAuthorityButtons()
    {
        var selected = SelectedAssignment();
        revokeAuthorityButton.Enabled = selected is { IsActive: true };
    }

    private UserAccount? SelectedUser()
    {
        return usersList.SelectedItems.Count == 0
            ? null
            : usersList.SelectedItems[0].Tag as UserAccount;
    }

    private AuthorityAssignment? SelectedAssignment()
    {
        return authorityList.SelectedItems.Count == 0
            ? null
            : authorityList.SelectedItems[0].Tag as AuthorityAssignment;
    }

    private void AddUser()
    {
        using var form = new EditUserForm(null);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = usersService.AddUser(new AddUserRequest
        {
            ProjectFolderPath = projectFolderPath,
            DisplayName = form.DisplayName,
            Email = form.Email,
            Phone = form.Phone,
            Organisation = form.Organisation,
            Actor = currentActor()
        });

        HandleResult(result, $"User '{form.DisplayName}' added.");
    }

    private void EditUser()
    {
        var user = SelectedUser();
        if (user is null)
        {
            return;
        }

        using var form = new EditUserForm(user);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = usersService.UpdateUser(new UpdateUserRequest
        {
            ProjectFolderPath = projectFolderPath,
            UserId = user.UserId,
            DisplayName = form.DisplayName,
            Email = form.Email,
            Phone = form.Phone,
            Organisation = form.Organisation,
            Actor = currentActor()
        });

        HandleResult(result, $"User '{form.DisplayName}' updated.");
    }

    private void DeactivateUser()
    {
        var user = SelectedUser();
        if (user is null)
        {
            return;
        }

        var reason = PromptForReason($"Deactivate {user.DisplayName}", "Reason for deactivation:");
        if (reason is null)
        {
            return;
        }

        var result = usersService.DeactivateUser(new DeactivateUserRequest
        {
            ProjectFolderPath = projectFolderPath,
            UserId = user.UserId,
            Reason = reason,
            Actor = currentActor()
        });

        HandleResult(result, $"{user.DisplayName} deactivated.");
    }

    private void ReactivateUser()
    {
        var user = SelectedUser();
        if (user is null)
        {
            return;
        }

        var result = usersService.ReactivateUser(new ReactivateUserRequest
        {
            ProjectFolderPath = projectFolderPath,
            UserId = user.UserId,
            Actor = currentActor()
        });

        HandleResult(result, $"{user.DisplayName} reactivated.");
    }

    private void AssignAuthority()
    {
        if (project is null)
        {
            return;
        }

        if (project.Users.All(user => !user.IsActive))
        {
            MessageBox.Show(this, "Add at least one active user before assigning authority.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new AssignAuthorityForm(project);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = usersService.AssignAuthority(new AssignAuthorityRequest
        {
            ProjectFolderPath = projectFolderPath,
            UserId = form.SelectedUserId,
            Role = form.SelectedRole,
            ScopeType = form.SelectedScopeType,
            ScopeId = form.SelectedScopeId,
            Reason = form.Reason,
            Actor = currentActor()
        });

        HandleResult(result, "Authority assigned.");
        if (tabs.SelectedIndex != 1)
        {
            tabs.SelectedIndex = 1;
        }
    }

    private void RevokeAuthority()
    {
        var assignment = SelectedAssignment();
        if (assignment is null)
        {
            return;
        }

        var reason = PromptForReason($"Revoke {assignment.Role} from {assignment.DisplayNameSnapshot}", "Reason for revocation:");
        if (reason is null)
        {
            return;
        }

        var result = usersService.RevokeAuthority(new RevokeAuthorityRequest
        {
            ProjectFolderPath = projectFolderPath,
            AssignmentId = assignment.AssignmentId,
            Reason = reason,
            Actor = currentActor()
        });

        HandleResult(result, "Authority revoked.");
    }

    private void HandleResult(OperationResult result, string successMessage)
    {
        if (!result.Succeeded)
        {
            var message = result.ErrorMessage ?? "Operation failed.";
            if (result.Validation.Issues.Count > 0)
            {
                message += Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, result.Validation.Issues.Select(issue => $"- {issue.Message}"));
            }
            MessageBox.Show(this, message, "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        statusLabel.Text = successMessage;
        LoadProject();
    }

    private string? PromptForReason(string title, string prompt)
    {
        using var form = new ReasonPromptForm(title, prompt);
        return form.ShowDialog(this) == DialogResult.OK ? form.Reason : null;
    }

    private sealed class ReasonPromptForm : Form
    {
        private readonly TextBox reasonTextBox = new();
        public string Reason => reasonTextBox.Text.Trim();

        public ReasonPromptForm(string title, string prompt)
        {
            Text = title;
            MinimumSize = new Size(480, 240);
            StartPosition = FormStartPosition.CenterParent;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(new Label
            {
                Text = prompt,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            }, 0, 0);

            reasonTextBox.Dock = DockStyle.Fill;
            reasonTextBox.Multiline = true;
            reasonTextBox.ScrollBars = ScrollBars.Vertical;
            layout.Controls.Add(reasonTextBox, 0, 1);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0)
            };
            var ok = new Button { Text = "Confirm", AutoSize = true };
            ok.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(reasonTextBox.Text))
                {
                    MessageBox.Show(this, "A reason is required.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult = DialogResult.OK;
            };
            var cancel = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
            cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
            actions.Controls.Add(ok);
            actions.Controls.Add(cancel);
            layout.Controls.Add(actions, 0, 2);

            Controls.Add(layout);
            AppTheme.Apply(this);
        }
    }
}
