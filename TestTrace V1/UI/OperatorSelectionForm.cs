namespace TestTrace_V1.UI;

public sealed class OperatorSelectionForm : Form
{
    private readonly OperatorRegistry registry;
    private readonly FlowLayoutPanel operatorCardsPanel = new();
    private readonly Label headlineLabel = new();
    private readonly Label helperLabel = new();
    private readonly Button continueButton = new() { Text = "Continue", AutoSize = true, UseMnemonic = false, Enabled = false };
    private readonly Button createButton = new() { Text = "Create Profile", AutoSize = true, UseMnemonic = false, Margin = new Padding(8, 0, 0, 0) };
    private readonly Button editButton = new() { Text = "Edit Profile", AutoSize = true, UseMnemonic = false, Margin = new Padding(8, 0, 0, 0), Enabled = false };
    private readonly Button quitButton = new() { Text = "Quit", AutoSize = true, UseMnemonic = false, Margin = new Padding(8, 0, 0, 0) };
    private Guid? selectedOperatorId;

    public OperatorProfile? SelectedOperator { get; private set; }

    public OperatorSelectionForm(OperatorRegistry registry)
    {
        this.registry = registry;
        Text = "TestTrace - Operator Sign In";
        ClientSize = new Size(840, 560);
        MinimumSize = new Size(700, 480);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        ShowInTaskbar = true;

        InitializeLayout();
        AppTheme.Apply(this);
        StyleLocalControls();
        Populate();
    }

    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(28, 24, 28, 22)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var identityPanel = CreateBorderedPanel(AppTheme.Current.SurfaceElevated);
        identityPanel.Dock = DockStyle.Fill;
        identityPanel.Padding = new Padding(24);
        identityPanel.Margin = new Padding(0, 0, 22, 0);
        root.Controls.Add(identityPanel, 0, 0);

        var identityLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        identityLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        identityLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        identityLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        identityLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        identityPanel.Controls.Add(identityLayout);

        var brandMark = BrandAssets.CreatePictureBox(
            BrandAssets.LoadWordmark(),
            new Size(238, 54),
            new Padding(0, 0, 0, 10));
        identityLayout.Controls.Add(brandMark, 0, 0);

        var brandLabel = new Label
        {
            Text = "TestTrace",
            AutoSize = true,
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            Margin = brandMark.Visible ? new Padding(0) : new Padding(0, 0, 0, 4),
            Visible = !brandMark.Visible,
            UseMnemonic = false
        };
        identityLayout.Controls.Add(brandLabel, 0, 0);

        var subtitleLabel = new Label
        {
            Text = "Choose the operator whose authority and audit trail will be used for this session.",
            AutoSize = true,
            MaximumSize = new Size(295, 0),
            ForeColor = AppTheme.Current.TextSecondary,
            Margin = new Padding(0, 0, 0, 24),
            UseMnemonic = false
        };
        identityLayout.Controls.Add(subtitleLabel, 0, 1);

        var guidanceLabel = new Label
        {
            Text = "Operator profiles are local identities. Project roles and authority assignments remain controlled inside each TestTrace project.",
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = AppTheme.Current.TextMuted,
            UseMnemonic = false
        };
        identityLayout.Controls.Add(guidanceLabel, 0, 2);

        var localNote = new Label
        {
            Text = "No cloud sign-in. No password. Just auditable local attribution.",
            AutoSize = true,
            MaximumSize = new Size(295, 0),
            ForeColor = AppTheme.Current.TextMuted,
            Margin = new Padding(0, 18, 0, 0),
            UseMnemonic = false
        };
        identityLayout.Controls.Add(localNote, 0, 3);

        var pickerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5
        };
        pickerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pickerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pickerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pickerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pickerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(pickerLayout, 1, 0);

        headlineLabel.AutoSize = true;
        headlineLabel.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        headlineLabel.Margin = new Padding(0, 0, 0, 4);
        headlineLabel.UseMnemonic = false;
        pickerLayout.Controls.Add(headlineLabel, 0, 0);

        helperLabel.AutoSize = true;
        helperLabel.MaximumSize = new Size(520, 0);
        helperLabel.ForeColor = AppTheme.Current.TextSecondary;
        helperLabel.Margin = new Padding(0, 0, 0, 16);
        helperLabel.UseMnemonic = false;
        pickerLayout.Controls.Add(helperLabel, 0, 1);

        operatorCardsPanel.Dock = DockStyle.Fill;
        operatorCardsPanel.FlowDirection = FlowDirection.TopDown;
        operatorCardsPanel.WrapContents = false;
        operatorCardsPanel.AutoScroll = true;
        operatorCardsPanel.Margin = new Padding(0, 0, 0, 16);
        operatorCardsPanel.Padding = new Padding(0, 0, 8, 0);
        operatorCardsPanel.Resize += (_, _) => UpdateCardWidths();
        pickerLayout.Controls.Add(operatorCardsPanel, 0, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0)
        };

        continueButton.Click += (_, _) => Accept();
        createButton.Click += (_, _) => CreateNew();
        editButton.Click += (_, _) => EditSelected();
        quitButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        ConfigureActionButton(continueButton, 170, new Padding(8, 0, 0, 0));
        ConfigureActionButton(createButton, 150, new Padding(8, 0, 0, 0));
        ConfigureActionButton(editButton, 130, new Padding(8, 0, 0, 0));
        ConfigureActionButton(quitButton, 86, new Padding(0));

        actions.Controls.Add(continueButton);
        actions.Controls.Add(createButton);
        actions.Controls.Add(editButton);
        actions.Controls.Add(quitButton);
        pickerLayout.Controls.Add(actions, 0, 3);

        var footnote = new Label
        {
            Text = "The selected operator is stamped into session actions once you continue.",
            AutoSize = true,
            ForeColor = AppTheme.Current.TextMuted,
            Margin = new Padding(0, 12, 0, 0),
            UseMnemonic = false
        };
        pickerLayout.Controls.Add(footnote, 0, 4);

        AcceptButton = continueButton;
        CancelButton = quitButton;

        Controls.Add(root);
    }

    private void StyleLocalControls()
    {
        AppTheme.StyleButton(continueButton, ThemeButtonKind.Primary);
        AppTheme.StyleButton(createButton, ThemeButtonKind.Secondary);
        AppTheme.StyleButton(editButton, ThemeButtonKind.Secondary);
        AppTheme.StyleButton(quitButton, ThemeButtonKind.Secondary);
    }

    private static void ConfigureActionButton(Button button, int width, Padding margin)
    {
        button.AutoSize = false;
        button.Width = width;
        button.Height = 38;
        button.Margin = margin;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.Padding = new Padding(10, 0, 10, 0);
    }

    private void Populate()
    {
        var operators = registry.OrderedForPicker().ToList();
        if (operators.Count == 0)
        {
            selectedOperatorId = null;
            headlineLabel.Text = "Create your first operator profile";
            helperLabel.Text = "TestTrace needs a local operator identity before it can stamp audit records, result entries, evidence uploads, and approvals.";
            RenderCards(operators);
            UpdateButtons();
            return;
        }

        headlineLabel.Text = "Who is using TestTrace?";
        helperLabel.Text = "Pick the local operator profile for this session. This does not grant project authority by itself.";

        if (selectedOperatorId is null || registry.FindById(selectedOperatorId.Value) is null)
        {
            selectedOperatorId = registry.LastActiveOperatorId is { } lastId && registry.FindById(lastId) is not null
                ? lastId
                : operators[0].OperatorId;
        }

        RenderCards(operators);
        UpdateButtons();
    }

    private void RenderCards(IReadOnlyList<OperatorProfile> operators)
    {
        operatorCardsPanel.SuspendLayout();
        operatorCardsPanel.Controls.Clear();

        if (operators.Count == 0)
        {
            var emptyPanel = CreateBorderedPanel(AppTheme.Current.SurfaceElevated);
            emptyPanel.Width = CardWidth();
            emptyPanel.Height = 126;
            emptyPanel.Margin = new Padding(0, 0, 0, 12);
            emptyPanel.Padding = new Padding(18);
            emptyPanel.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "No operator profiles exist yet.\r\nCreate one to begin using governed TestTrace attribution.",
                ForeColor = AppTheme.Current.TextSecondary,
                UseMnemonic = false
            });
            operatorCardsPanel.Controls.Add(emptyPanel);
        }
        else
        {
            foreach (var op in operators)
            {
                operatorCardsPanel.Controls.Add(CreateOperatorCard(op));
            }
        }

        operatorCardsPanel.ResumeLayout();
    }

    private Panel CreateOperatorCard(OperatorProfile op)
    {
        var selected = selectedOperatorId == op.OperatorId;
        var panel = CreateBorderedPanel(selected ? AppTheme.Current.SelectionBackground : AppTheme.Current.SurfaceElevated);
        panel.Width = CardWidth();
        panel.Height = 104;
        panel.Margin = new Padding(0, 0, 0, 12);
        panel.Padding = new Padding(14);
        panel.Cursor = Cursors.Hand;
        panel.Tag = op.OperatorId;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = panel.BackColor
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 66));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        panel.Controls.Add(layout);

        var badge = new Label
        {
            Text = op.Initials,
            Size = new Size(50, 50),
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = selected ? AppTheme.Current.Accent : AppTheme.Current.SurfaceAlt,
            ForeColor = selected ? AppTheme.Current.SelectionForeground : AppTheme.Current.TextPrimary,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Margin = new Padding(0, 10, 16, 0),
            UseMnemonic = false
        };
        badge.Paint += DrawBadgeBorder;
        layout.Controls.Add(badge, 0, 0);

        var details = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = panel.BackColor,
            Margin = new Padding(0)
        };
        layout.Controls.Add(details, 1, 0);

        details.Controls.Add(new Label
        {
            Text = op.DisplayName,
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = selected ? AppTheme.Current.SelectionForeground : AppTheme.Current.TextPrimary,
            Margin = new Padding(0, 8, 0, 2),
            UseMnemonic = false
        }, 0, 0);

        details.Controls.Add(new Label
        {
            Text = BuildProfileLine(op),
            AutoSize = true,
            ForeColor = selected ? AppTheme.Current.SelectionForeground : AppTheme.Current.TextSecondary,
            Margin = new Padding(0, 0, 0, 4),
            UseMnemonic = false
        }, 0, 1);

        details.Controls.Add(new Label
        {
            Text = "Authority is assigned per project.",
            AutoSize = true,
            ForeColor = selected ? AppTheme.Current.SelectionForeground : AppTheme.Current.TextMuted,
            Margin = new Padding(0),
            UseMnemonic = false
        }, 0, 2);

        var lastUsed = new Label
        {
            Text = op.LastActiveAt is null ? "Never used" : FormatRelative(op.LastActiveAt.Value),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = selected ? AppTheme.Current.SelectionForeground : AppTheme.Current.TextMuted,
            Margin = new Padding(0, 10, 0, 0),
            UseMnemonic = false
        };
        layout.Controls.Add(lastUsed, 2, 0);

        WireCardClicks(panel, op.OperatorId);
        return panel;
    }

    private void WireCardClicks(Control control, Guid id)
    {
        control.Click += (_, _) => SelectOperator(id);
        control.DoubleClick += (_, _) =>
        {
            SelectOperator(id);
            Accept();
        };

        foreach (Control child in control.Controls)
        {
            WireCardClicks(child, id);
        }
    }

    private void SelectOperator(Guid id)
    {
        if (selectedOperatorId == id)
        {
            return;
        }

        selectedOperatorId = id;
        RenderCards(registry.OrderedForPicker().ToList());
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var selected = SelectedFromCards();
        continueButton.Enabled = selected is not null;
        editButton.Enabled = selected is not null;
        continueButton.Text = selected is null ? "Continue" : $"Continue as {selected.DisplayName}";
    }

    private OperatorProfile? SelectedFromCards()
    {
        return selectedOperatorId is null ? null : registry.FindById(selectedOperatorId.Value);
    }

    private void Accept()
    {
        var selected = SelectedFromCards();
        if (selected is null)
        {
            return;
        }

        SelectedOperator = selected;
        DialogResult = DialogResult.OK;
    }

    private void CreateNew()
    {
        using var form = new EditOperatorProfileForm(existing: null, isFirstRun: registry.Operators.Count == 0);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var profile = OperatorProfile.Create(
                form.DisplayName,
                form.JobRole,
                form.Email,
                form.Phone,
                form.Organisation,
                DateTimeOffset.UtcNow);
            registry.Add(profile);
            registry.Save();
            selectedOperatorId = profile.OperatorId;
            Populate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void EditSelected()
    {
        var selected = SelectedFromCards();
        if (selected is null)
        {
            return;
        }

        using var form = new EditOperatorProfileForm(selected);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var updated = selected.WithProfile(form.DisplayName, form.JobRole, form.Email, form.Phone, form.Organisation);
            registry.Update(selected.OperatorId, updated);
            registry.Save();
            selectedOperatorId = updated.OperatorId;
            Populate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private int CardWidth()
    {
        var available = operatorCardsPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 14;
        return Math.Max(420, available);
    }

    private void UpdateCardWidths()
    {
        var width = CardWidth();
        foreach (Control card in operatorCardsPanel.Controls)
        {
            card.Width = width;
        }
    }

    private static string BuildProfileLine(OperatorProfile op)
    {
        var details = new List<string>();
        if (!string.IsNullOrWhiteSpace(op.JobRole))
        {
            details.Add(op.JobRole);
        }

        if (!string.IsNullOrWhiteSpace(op.Organisation))
        {
            details.Add(op.Organisation);
        }

        if (!string.IsNullOrWhiteSpace(op.Email))
        {
            details.Add(op.Email);
        }

        return details.Count == 0 ? "No optional contact details" : string.Join(" | ", details);
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

        using var borderPen = new Pen(AppTheme.Current.Border);
        var rect = control.ClientRectangle;
        rect.Width -= 1;
        rect.Height -= 1;
        e.Graphics.DrawRectangle(borderPen, rect);
    }

    private static string FormatRelative(DateTimeOffset at)
    {
        var delta = DateTimeOffset.UtcNow - at;
        if (delta.TotalSeconds < 60) return "Just now";
        if (delta.TotalMinutes < 60) return $"{(int)delta.TotalMinutes}m ago";
        if (delta.TotalHours < 24) return $"{(int)delta.TotalHours}h ago";
        if (delta.TotalDays < 30) return $"{(int)delta.TotalDays}d ago";
        return at.LocalDateTime.ToString("dd MMM yyyy");
    }
}
