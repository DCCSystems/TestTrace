using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class SwitchUserForm : Form
{
    private readonly TestTraceProject project;
    private readonly Guid? currentUserId;
    private readonly ListView userList = new();
    private readonly Label hintLabel = new();
    private readonly Button confirmButton = new() { Text = "Use Selected", AutoSize = true, Enabled = false };

    public Guid SelectedUserId { get; private set; }
    public string SelectedDisplayName { get; private set; } = string.Empty;
    public bool ManageUsersRequested { get; private set; }

    public SwitchUserForm(TestTraceProject project, Guid? currentUserId)
    {
        this.project = project;
        this.currentUserId = currentUserId;
        Text = "Switch Active User";
        MinimumSize = new Size(620, 460);
        StartPosition = FormStartPosition.CenterParent;

        InitializeLayout();
        AppTheme.Apply(this);
        Populate();
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Who is acting in TestTrace right now?",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 13, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 4)
        };
        layout.Controls.Add(title, 0, 0);

        hintLabel.Text = "Each test result, evidence file, approval and release will be stamped with this identity. Pick the user you are acting as before continuing.";
        hintLabel.AutoSize = true;
        hintLabel.MaximumSize = new Size(580, 0);
        hintLabel.ForeColor = AppTheme.Current.TextSecondary;
        hintLabel.Margin = new Padding(0, 0, 0, 12);
        layout.Controls.Add(hintLabel, 0, 1);

        userList.Dock = DockStyle.Fill;
        userList.View = View.Details;
        userList.FullRowSelect = true;
        userList.HideSelection = false;
        userList.MultiSelect = false;
        userList.Columns.Add("User", 200);
        userList.Columns.Add("Initials", 80);
        userList.Columns.Add("Roles", 240);
        userList.Columns.Add("Status", 90);
        userList.SelectedIndexChanged += (_, _) => confirmButton.Enabled = userList.SelectedItems.Count > 0;
        userList.DoubleClick += (_, _) => Accept();
        layout.Controls.Add(userList, 0, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };

        confirmButton.Click += (_, _) => Accept();
        var manageButton = new Button { Text = "Manage Users & Authority...", AutoSize = true, Margin = new Padding(8, 0, 0, 0), UseMnemonic = false };
        manageButton.Click += (_, _) =>
        {
            ManageUsersRequested = true;
            DialogResult = DialogResult.Cancel;
        };
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        actions.Controls.Add(confirmButton);
        actions.Controls.Add(cancelButton);
        actions.Controls.Add(manageButton);
        layout.Controls.Add(actions, 0, 3);

        Controls.Add(layout);
    }

    private void Populate()
    {
        userList.BeginUpdate();
        userList.Items.Clear();

        foreach (var user in project.Users.OrderByDescending(user => user.IsActive).ThenBy(user => user.DisplayName))
        {
            var roles = string.Join(", ", project.AuthorityAssignments
                .Where(assignment => assignment.IsActive && assignment.UserId == user.UserId)
                .Select(assignment => assignment.Role.ToString())
                .Distinct());

            var item = new ListViewItem(user.DisplayName);
            item.SubItems.Add(user.Initials);
            item.SubItems.Add(string.IsNullOrWhiteSpace(roles) ? "-" : roles);
            item.SubItems.Add(user.IsActive ? "Active" : "Inactive");
            item.Tag = user;
            if (!user.IsActive)
            {
                item.ForeColor = AppTheme.Current.TextMuted;
            }
            if (currentUserId is not null && user.UserId == currentUserId.Value)
            {
                item.Font = new Font(userList.Font, FontStyle.Bold);
                item.SubItems[3].Text += " (current)";
            }

            userList.Items.Add(item);
        }

        userList.EndUpdate();

        var defaultUser = project.Users.FirstOrDefault(user => currentUserId is not null && user.UserId == currentUserId.Value)
            ?? ActiveUserContext.PickSensibleDefault(project);
        if (defaultUser is not null)
        {
            foreach (ListViewItem item in userList.Items)
            {
                if (item.Tag is UserAccount user && user.UserId == defaultUser.UserId)
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    break;
                }
            }
        }
    }

    private void Accept()
    {
        if (userList.SelectedItems.Count == 0 || userList.SelectedItems[0].Tag is not UserAccount user)
        {
            return;
        }

        if (!user.IsActive)
        {
            MessageBox.Show(this, $"{user.DisplayName} is deactivated and cannot be the active user. Reactivate them first.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SelectedUserId = user.UserId;
        SelectedDisplayName = user.DisplayName;
        DialogResult = DialogResult.OK;
    }
}
