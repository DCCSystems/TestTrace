namespace TestTrace_V1.UI;

public sealed class EditOperatorProfileForm : Form
{
    private readonly TextBox displayNameTextBox = new();
    private readonly ComboBox jobRoleComboBox = new();
    private readonly TextBox emailTextBox = new();
    private readonly TextBox phoneTextBox = new();
    private readonly TextBox organisationTextBox = new();
    private readonly Label initialsBadgeLabel = new();
    private readonly Label identityPreviewLabel = new();
    private readonly Label validationLabel = new();
    private readonly Button saveButton = new();
    private readonly Button cancelButton = new();
    private readonly bool isFirstRun;

    public string DisplayName => displayNameTextBox.Text.Trim();
    public string? JobRole => Trim(jobRoleComboBox.Text);
    public string? Email => Trim(emailTextBox.Text);
    public string? Phone => Trim(phoneTextBox.Text);
    public string? Organisation => Trim(organisationTextBox.Text);

    public EditOperatorProfileForm(OperatorProfile? existing, bool isFirstRun = false)
    {
        this.isFirstRun = isFirstRun;
        Text = existing is null
            ? (isFirstRun ? "Welcome to TestTrace" : "Create Operator Profile")
            : $"Edit Operator Profile - {existing.DisplayName}";
        ClientSize = new Size(760, 520);
        MinimumSize = new Size(680, 480);
        StartPosition = isFirstRun ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        ShowInTaskbar = isFirstRun;

        if (existing is not null)
        {
            displayNameTextBox.Text = existing.DisplayName;
            jobRoleComboBox.Text = existing.JobRole ?? string.Empty;
            emailTextBox.Text = existing.Email ?? string.Empty;
            phoneTextBox.Text = existing.Phone ?? string.Empty;
            organisationTextBox.Text = existing.Organisation ?? string.Empty;
        }
        else
        {
            var windowsName = Environment.UserName;
            if (!string.IsNullOrWhiteSpace(windowsName))
            {
                displayNameTextBox.Text = windowsName;
            }
        }

        InitializeLayout(existing is null);
        AppTheme.Apply(this);
        StyleLocalSurfaces();
        RefreshIdentityPreview();
    }

    private void InitializeLayout(bool creating)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(26, 24, 26, 22)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var heroPanel = CreateBorderedPanel(AppTheme.Current.SurfaceElevated);
        heroPanel.Dock = DockStyle.Fill;
        heroPanel.Margin = new Padding(0, 0, 22, 0);
        heroPanel.Padding = new Padding(22, 22, 22, 22);
        root.Controls.Add(heroPanel, 0, 0);

        var heroLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5
        };
        heroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        heroPanel.Controls.Add(heroLayout);

        var productLabel = new Label
        {
            Text = "TestTrace",
            AutoSize = true,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 2),
            UseMnemonic = false
        };
        heroLayout.Controls.Add(productLabel, 0, 0);

        var productSubtitle = new Label
        {
            Text = "Operator identity",
            AutoSize = true,
            ForeColor = AppTheme.Current.TextSecondary,
            Margin = new Padding(0, 0, 0, 22),
            UseMnemonic = false
        };
        heroLayout.Controls.Add(productSubtitle, 0, 1);

        initialsBadgeLabel.Size = new Size(104, 104);
        initialsBadgeLabel.Anchor = AnchorStyles.None;
        initialsBadgeLabel.TextAlign = ContentAlignment.MiddleCenter;
        initialsBadgeLabel.Font = new Font("Segoe UI", 26, FontStyle.Bold);
        initialsBadgeLabel.Margin = new Padding(0, 0, 0, 18);
        initialsBadgeLabel.UseMnemonic = false;
        initialsBadgeLabel.Paint += DrawBadgeBorder;
        heroLayout.Controls.Add(initialsBadgeLabel, 0, 2);

        identityPreviewLabel.AutoSize = false;
        identityPreviewLabel.Dock = DockStyle.Fill;
        identityPreviewLabel.ForeColor = AppTheme.Current.TextSecondary;
        identityPreviewLabel.Margin = new Padding(0);
        identityPreviewLabel.UseMnemonic = false;
        heroLayout.Controls.Add(identityPreviewLabel, 0, 3);

        var trustNote = new Label
        {
            Text = "No password. Stored locally on this machine. Project authority is assigned separately.",
            AutoSize = true,
            MaximumSize = new Size(295, 0),
            ForeColor = AppTheme.Current.TextMuted,
            Margin = new Padding(0, 18, 0, 0),
            UseMnemonic = false
        };
        heroLayout.Controls.Add(trustNote, 0, 4);

        var formLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7
        };
        formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        formLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(formLayout, 1, 0);

        var stepLabel = new Label
        {
            Text = creating ? "Create operator profile" : "Edit operator profile",
            AutoSize = true,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 4),
            UseMnemonic = false
        };
        formLayout.Controls.Add(stepLabel, 0, 0);

        var helpLabel = new Label
        {
            Text = isFirstRun
                ? "This profile identifies you in TestTrace audit entries, result records, evidence uploads, approvals, and release decisions."
                : "Update how this local operator appears in future TestTrace audit entries and authority screens.",
            AutoSize = true,
            MaximumSize = new Size(440, 0),
            ForeColor = AppTheme.Current.TextSecondary,
            Margin = new Padding(0, 0, 0, 18),
            UseMnemonic = false
        };
        formLayout.Controls.Add(helpLabel, 0, 1);

        var fieldsPanel = CreateBorderedPanel(AppTheme.Current.SurfaceElevated);
        fieldsPanel.Dock = DockStyle.Top;
        fieldsPanel.AutoSize = true;
        fieldsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        fieldsPanel.Padding = new Padding(20, 18, 20, 16);
        fieldsPanel.Margin = new Padding(0, 0, 0, 14);
        formLayout.Controls.Add(fieldsPanel, 0, 2);

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 0
        };
        fieldsPanel.Controls.Add(fields);

        displayNameTextBox.PlaceholderText = "Name shown in audit records";
        jobRoleComboBox.Items.AddRange(new object[]
        {
            "Mechanical Engineer",
            "Electrical Engineer",
            "Controls Engineer",
            "Commissioning Engineer",
            "Lead Test Engineer",
            "Project Engineer",
            "Quality Engineer",
            "Customer Witness",
            "Other"
        });
        emailTextBox.PlaceholderText = "Optional";
        phoneTextBox.PlaceholderText = "Optional";
        organisationTextBox.PlaceholderText = "Optional";
        SetupTextBox(displayNameTextBox);
        SetupComboBox(jobRoleComboBox);
        SetupTextBox(emailTextBox);
        SetupTextBox(phoneTextBox);
        SetupTextBox(organisationTextBox);
        displayNameTextBox.TextChanged += (_, _) => RefreshIdentityPreview();
        jobRoleComboBox.TextChanged += (_, _) => RefreshIdentityPreview();
        jobRoleComboBox.SelectedIndexChanged += (_, _) => RefreshIdentityPreview();

        AddField(fields, "Display name *", displayNameTextBox);
        AddField(fields, "Job role", jobRoleComboBox);
        AddField(fields, "Email", emailTextBox);
        AddField(fields, "Phone", phoneTextBox);
        AddField(fields, "Organisation", organisationTextBox);

        validationLabel.AutoSize = true;
        validationLabel.ForeColor = AppTheme.Current.FailForeground;
        validationLabel.Margin = new Padding(0, 0, 0, 10);
        validationLabel.UseMnemonic = false;
        formLayout.Controls.Add(validationLabel, 0, 3);

        var boundaryNote = new Label
        {
            Text = "Display name is required. Job role and contact details describe the operator; project access and authority are assigned separately inside each TestTrace project.",
            AutoSize = true,
            MaximumSize = new Size(520, 0),
            ForeColor = AppTheme.Current.TextMuted,
            Margin = new Padding(0, 0, 0, 18),
            UseMnemonic = false
        };
        formLayout.Controls.Add(boundaryNote, 0, 4);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = false
        };

        saveButton.Text = creating ? "Create Profile" : "Save Profile";
        saveButton.AutoSize = true;
        saveButton.UseMnemonic = false;
        saveButton.Click += (_, _) => Accept();

        cancelButton.Text = "Cancel";
        cancelButton.AutoSize = true;
        cancelButton.Margin = new Padding(8, 0, 0, 0);
        cancelButton.UseMnemonic = false;
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        actions.Controls.Add(saveButton);
        actions.Controls.Add(cancelButton);
        formLayout.Controls.Add(actions, 0, 6);

        AcceptButton = saveButton;
        CancelButton = cancelButton;

        Controls.Add(root);
    }

    private void StyleLocalSurfaces()
    {
        AppTheme.StyleButton(saveButton, ThemeButtonKind.Primary);
        AppTheme.StyleButton(cancelButton, ThemeButtonKind.Secondary);
        initialsBadgeLabel.BackColor = AppTheme.Current.SelectionBackground;
        initialsBadgeLabel.ForeColor = AppTheme.Current.SelectionForeground;
        identityPreviewLabel.ForeColor = AppTheme.Current.TextSecondary;
        validationLabel.ForeColor = AppTheme.Current.FailForeground;
    }

    private void RefreshIdentityPreview()
    {
        var displayName = displayNameTextBox.Text.Trim();
        var jobRole = jobRoleComboBox.Text.Trim();
        initialsBadgeLabel.Text = BuildInitials(displayName);
        identityPreviewLabel.Text = string.IsNullOrWhiteSpace(displayName)
            ? "This profile will become the local identity used for audited TestTrace actions."
            : string.IsNullOrWhiteSpace(jobRole)
                ? $"{displayName}\r\nwill be used for audited TestTrace actions on this machine."
                : $"{displayName}\r\n{jobRole}\r\nwill be used for audited TestTrace actions on this machine.";
        validationLabel.Text = string.Empty;
        initialsBadgeLabel.Invalidate();
    }

    private void Accept()
    {
        if (string.IsNullOrWhiteSpace(displayNameTextBox.Text))
        {
            validationLabel.Text = "Display name is required before this operator profile can be saved.";
            displayNameTextBox.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private static void SetupTextBox(TextBox textBox)
    {
        textBox.Dock = DockStyle.Top;
        textBox.Margin = new Padding(0, 0, 0, 12);
        textBox.Height = 28;
        textBox.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void SetupComboBox(ComboBox comboBox)
    {
        comboBox.Dock = DockStyle.Top;
        comboBox.Margin = new Padding(0, 0, 0, 12);
        comboBox.Height = 30;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.DropDownStyle = ComboBoxStyle.DropDown;
        comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
    }

    private static void AddField(TableLayoutPanel layout, string labelText, Control control)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4),
            ForeColor = AppTheme.Current.TextSecondary,
            UseMnemonic = false
        };
        layout.Controls.Add(label, 0, layout.RowCount++);
        layout.Controls.Add(control, 0, layout.RowCount++);
    }

    private static Panel CreateBorderedPanel(Color backColor)
    {
        var panel = new Panel
        {
            BackColor = backColor
        };
        panel.Paint += (_, e) =>
        {
            using var borderPen = new Pen(AppTheme.Current.Border);
            var rect = panel.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            e.Graphics.DrawRectangle(borderPen, rect);
        };
        return panel;
    }

    private static void DrawBadgeBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        using var borderPen = new Pen(AppTheme.Current.Accent, 2);
        var rect = control.ClientRectangle;
        rect.Width -= 1;
        rect.Height -= 1;
        e.Graphics.DrawRectangle(borderPen, rect);
    }

    private static string BuildInitials(string displayName)
    {
        var parts = (displayName ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => part.Length > 0)
            .ToArray();
        if (parts.Length == 0)
        {
            return "?";
        }

        var initials = parts.Length == 1
            ? parts[0][..Math.Min(parts[0].Length, 2)]
            : string.Concat(parts.Take(3).Select(part => part[0]));
        return initials.ToUpperInvariant();
    }

    private static string? Trim(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
