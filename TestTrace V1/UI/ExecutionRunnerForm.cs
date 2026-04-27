using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;
using TestTrace_V1.Workspace;

namespace TestTrace_V1.UI;

public sealed class ExecutionRunnerForm : Form
{
    private readonly string projectFolderPath;
    private readonly JsonProjectRepository repository = new();
    private readonly ExecutionService executionService;

    private TestTraceProject? project;
    private ActiveUserContext? activeUser;
    private Guid? selectedTestItemId;
    private bool populatingFilters;

    private readonly Label titleLabel = new();
    private readonly Label subtitleLabel = new();
    private readonly Label stateBadge = new();
    private readonly Label actingAsLabel = new();
    private readonly Label accessLabel = new();
    private readonly Label statusLabel = new();
    private readonly ComboBox sectionFilter = new();
    private readonly ComboBox assetFilter = new();
    private readonly ComboBox statusFilter = new();
    private readonly CheckBox showNotApplicableCheckBox = new() { Text = "Show not applicable", AutoSize = true };
    private readonly ListView workList = new();
    private readonly FlowLayoutPanel cardsPanel = new();

    public ExecutionRunnerForm(string projectFolderPath)
    {
        this.projectFolderPath = projectFolderPath;
        executionService = new ExecutionService(repository);

        Text = $"TestTrace FAT Runner [{TestTraceAppEnvironment.ModeLabel}]";
        MinimumSize = new Size(1180, 760);
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;

        InitializeLayout();
        AppTheme.Apply(this);
        LoadProject();
    }

    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(CreateHeader(), 0, 0);
        root.Controls.Add(CreateToolbar(), 0, 1);
        root.Controls.Add(CreateWorkSurface(), 0, 2);
        root.Controls.Add(CreateFooter(), 0, 3);

        Controls.Add(root);
    }

    private Control CreateHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 14)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true
        };

        titleLabel.AutoSize = true;
        titleLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 20, FontStyle.Bold);
        titleLabel.Margin = new Padding(0, 0, 0, 4);

        subtitleLabel.AutoSize = true;
        subtitleLabel.ForeColor = AppTheme.Current.TextSecondary;
        subtitleLabel.Margin = new Padding(0);

        left.Controls.Add(titleLabel, 0, 0);
        left.Controls.Add(subtitleLabel, 0, 1);

        var right = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 3,
            Anchor = AnchorStyles.Right | AnchorStyles.Top
        };

        stateBadge.AutoSize = true;
        stateBadge.Anchor = AnchorStyles.Right;
        stateBadge.Padding = new Padding(12, 6, 12, 6);
        stateBadge.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        stateBadge.Margin = new Padding(0, 0, 0, 8);

        actingAsLabel.AutoSize = true;
        actingAsLabel.Anchor = AnchorStyles.Right;
        actingAsLabel.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        actingAsLabel.ForeColor = AppTheme.Current.Accent;
        actingAsLabel.Margin = new Padding(0, 0, 0, 4);

        accessLabel.AutoSize = true;
        accessLabel.Anchor = AnchorStyles.Right;
        accessLabel.ForeColor = AppTheme.Current.TextSecondary;

        right.Controls.Add(stateBadge, 0, 0);
        right.Controls.Add(actingAsLabel, 0, 1);
        right.Controls.Add(accessLabel, 0, 2);

        header.Controls.Add(left, 0, 0);
        header.Controls.Add(right, 1, 0);
        return header;
    }

    private Control CreateToolbar()
    {
        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 10,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 12)
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var filterLabel = new Label
        {
            Text = "Work scope",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 0, 10, 0)
        };

        var assetLabel = new Label
        {
            Text = "Asset",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(14, 0, 10, 0)
        };

        var statusFilterLabel = new Label
        {
            Text = "Status",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(14, 0, 10, 0)
        };

        sectionFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        sectionFilter.Dock = DockStyle.Fill;
        sectionFilter.SelectedIndexChanged += (_, _) =>
        {
            if (!populatingFilters)
            {
                PopulateAssetFilter();
                RenderWork();
            }
        };

        assetFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        assetFilter.Dock = DockStyle.Fill;
        assetFilter.SelectedIndexChanged += (_, _) =>
        {
            if (!populatingFilters)
            {
                RenderWork();
            }
        };

        statusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        statusFilter.Dock = DockStyle.Fill;
        statusFilter.SelectedIndexChanged += (_, _) =>
        {
            if (!populatingFilters)
            {
                RenderWork();
            }
        };

        showNotApplicableCheckBox.Margin = new Padding(14, 0, 0, 0);
        showNotApplicableCheckBox.CheckedChanged += (_, _) => RenderWork();

        var switchUserButton = new Button { Text = "Switch User", AutoSize = true };
        switchUserButton.Click += (_, _) => OpenSwitchUserDialog();

        var refreshButton = new Button { Text = "Refresh", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        refreshButton.Click += (_, _) => LoadProject();

        toolbar.Controls.Add(filterLabel, 0, 0);
        toolbar.Controls.Add(sectionFilter, 1, 0);
        toolbar.Controls.Add(assetLabel, 2, 0);
        toolbar.Controls.Add(assetFilter, 3, 0);
        toolbar.Controls.Add(statusFilterLabel, 4, 0);
        toolbar.Controls.Add(statusFilter, 5, 0);
        toolbar.Controls.Add(showNotApplicableCheckBox, 6, 0);
        toolbar.Controls.Add(new Label(), 7, 0);
        toolbar.Controls.Add(switchUserButton, 8, 0);
        toolbar.Controls.Add(refreshButton, 9, 0);

        return CreateSurface(toolbar, new Padding(12), elevated: true);
    }

    private Control CreateWorkSurface()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 420,
            Panel1MinSize = 360,
            Panel2MinSize = 520
        };

        split.Panel1.Controls.Add(CreateQueuePanel());

        cardsPanel.Dock = DockStyle.Fill;
        cardsPanel.AutoScroll = true;
        cardsPanel.FlowDirection = FlowDirection.TopDown;
        cardsPanel.WrapContents = false;
        cardsPanel.Padding = new Padding(12);
        cardsPanel.BackColor = AppTheme.Current.SurfaceAlt;
        cardsPanel.Resize += (_, _) => ResizeCards();

        split.Panel2.Controls.Add(CreateSurface(cardsPanel, new Padding(0), elevated: false));
        return split;
    }

    private Control CreateQueuePanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label
        {
            Text = "My FAT Queue",
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 6)
        }, 0, 0);

        var hint = new Label
        {
            Text = "Filter by section, asset, or status. Pick a check, work the card, move on.",
            AutoSize = true,
            ForeColor = AppTheme.Current.TextSecondary,
            Margin = new Padding(0, 0, 0, 12)
        };
        layout.Controls.Add(hint, 0, 1);

        workList.Dock = DockStyle.Fill;
        workList.View = View.Details;
        workList.FullRowSelect = true;
        workList.HideSelection = false;
        workList.MultiSelect = false;
        workList.Columns.Add("Ref", 75);
        workList.Columns.Add("Section", 115);
        workList.Columns.Add("Asset", 140);
        workList.Columns.Add("State", 85);
        workList.SelectedIndexChanged += (_, _) =>
        {
            if (workList.SelectedItems.Count == 0 || workList.SelectedItems[0].Tag is not RunnerWorkItem item)
            {
                return;
            }

            selectedTestItemId = item.TestItem.TestItemId;
            HighlightSelectedCard();
        };

        layout.Controls.Add(workList, 0, 2);
        return CreateSurface(layout, new Padding(12), elevated: true);
    }

    private Control CreateFooter()
    {
        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 12, 0, 0)
        };

        var closeButton = new Button { Text = "Close Runner", AutoSize = true };
        closeButton.Click += (_, _) => Close();

        statusLabel.AutoSize = true;
        statusLabel.Anchor = AnchorStyles.Left;
        statusLabel.Margin = new Padding(16, 6, 0, 0);
        statusLabel.ForeColor = AppTheme.Current.TextSecondary;

        footer.Controls.Add(closeButton);
        footer.Controls.Add(statusLabel);
        return footer;
    }

    private void LoadProject()
    {
        try
        {
            project = repository.Load(ProjectLocation.FromProjectFolder(projectFolderPath));
            EnsureActiveUserSelected();
            PopulateSectionFilter();
            PopulateAssetFilter();
            PopulateStatusFilter();
            RenderHeader();
            RenderWork();
        }
        catch (Exception ex)
        {
            project = null;
            titleLabel.Text = "FAT Runner";
            subtitleLabel.Text = projectFolderPath;
            SetStateBadge("Load error", AppTheme.Current.FailSoftBackground, AppTheme.Current.TextPrimary);
            cardsPanel.Controls.Clear();
            workList.Items.Clear();
            statusLabel.Text = ex.Message;
        }
    }

    private void EnsureActiveUserSelected()
    {
        if (project is null)
        {
            activeUser = null;
            return;
        }

        var operatorProfile = OperatorSession.Current;
        if (operatorProfile is not null)
        {
            var matched = project.Users.FirstOrDefault(user =>
                user.IsActive &&
                string.Equals(user.DisplayName, operatorProfile.DisplayName, StringComparison.OrdinalIgnoreCase));
            if (matched is not null)
            {
                activeUser = ActiveUserContext.FromUser(matched);
                return;
            }
        }

        if (activeUser is not null)
        {
            var stillActive = project.Users.FirstOrDefault(user => user.UserId == activeUser.UserId && user.IsActive);
            if (stillActive is not null)
            {
                activeUser = ActiveUserContext.FromUser(stillActive);
                return;
            }
        }

        var fallback = ActiveUserContext.PickSensibleDefault(project);
        activeUser = fallback is null ? null : ActiveUserContext.FromUser(fallback);
    }

    private void PopulateSectionFilter()
    {
        if (project is null)
        {
            return;
        }

        var selectedSectionId = (sectionFilter.SelectedItem as SectionFilterItem)?.SectionId;
        populatingFilters = true;
        sectionFilter.Items.Clear();
        sectionFilter.Items.Add(new SectionFilterItem(null, "All assigned work"));

        foreach (var section in project.Sections.OrderBy(section => section.DisplayOrder))
        {
            if (SectionHasVisibleWork(section))
            {
                sectionFilter.Items.Add(new SectionFilterItem(section.SectionId, section.Title));
            }
        }

        var target = sectionFilter.Items
            .OfType<SectionFilterItem>()
            .FirstOrDefault(item => item.SectionId == selectedSectionId)
            ?? sectionFilter.Items.OfType<SectionFilterItem>().FirstOrDefault();
        sectionFilter.SelectedItem = target;
        populatingFilters = false;
    }

    private void PopulateAssetFilter()
    {
        if (project is null)
        {
            return;
        }

        var selectedAssetId = (assetFilter.SelectedItem as AssetFilterItem)?.AssetId;
        var wasPopulating = populatingFilters;
        populatingFilters = true;
        assetFilter.Items.Clear();
        assetFilter.Items.Add(new AssetFilterItem(null, "All assets"));

        var sectionId = (sectionFilter.SelectedItem as SectionFilterItem)?.SectionId;
        var assetIds = new HashSet<Guid>();
        foreach (var section in project.Sections.OrderBy(section => section.DisplayOrder))
        {
            if (sectionId is not null && section.SectionId != sectionId.Value)
            {
                continue;
            }

            foreach (var testItem in section.TestItems)
            {
                if (!CanSeeWork(section, testItem) || testItem.AssetId is null)
                {
                    continue;
                }

                assetIds.Add(testItem.AssetId.Value);
                var asset = FindAsset(testItem.AssetId.Value);
                if (asset?.ParentAssetId is not null)
                {
                    assetIds.Add(asset.ParentAssetId.Value);
                }
            }
        }

        foreach (var assetId in assetIds
            .Select(assetId => new AssetFilterItem(assetId, AssetHierarchyLabel(assetId)))
            .OrderBy(item => item.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            assetFilter.Items.Add(assetId);
        }

        var target = assetFilter.Items
            .OfType<AssetFilterItem>()
            .FirstOrDefault(item => item.AssetId == selectedAssetId)
            ?? assetFilter.Items.OfType<AssetFilterItem>().FirstOrDefault();
        assetFilter.SelectedItem = target;
        populatingFilters = wasPopulating;
    }

    private void PopulateStatusFilter()
    {
        var selectedStatus = (statusFilter.SelectedItem as StatusFilterItem)?.Status ?? WorkStatusFilter.Outstanding;
        var wasPopulating = populatingFilters;
        populatingFilters = true;
        statusFilter.Items.Clear();
        statusFilter.Items.Add(new StatusFilterItem(WorkStatusFilter.Outstanding, "Outstanding"));
        statusFilter.Items.Add(new StatusFilterItem(WorkStatusFilter.All, "All visible work"));
        statusFilter.Items.Add(new StatusFilterItem(WorkStatusFilter.NotTested, "Not tested"));
        statusFilter.Items.Add(new StatusFilterItem(WorkStatusFilter.Passed, "Passed"));
        statusFilter.Items.Add(new StatusFilterItem(WorkStatusFilter.Failed, "Failed"));
        statusFilter.Items.Add(new StatusFilterItem(WorkStatusFilter.NeedsEvidence, "Needs evidence"));
        statusFilter.Items.Add(new StatusFilterItem(WorkStatusFilter.RequiresWitness, "Requires witness"));
        statusFilter.Items.Add(new StatusFilterItem(WorkStatusFilter.NotApplicable, "Not applicable"));

        var target = statusFilter.Items
            .OfType<StatusFilterItem>()
            .FirstOrDefault(item => item.Status == selectedStatus)
            ?? statusFilter.Items.OfType<StatusFilterItem>().FirstOrDefault();
        statusFilter.SelectedItem = target;
        populatingFilters = wasPopulating;
    }

    private bool SectionHasVisibleWork(Section section)
    {
        if (project is null)
        {
            return false;
        }

        return section.TestItems.Any(testItem => CanSeeWork(section, testItem));
    }

    private void RenderHeader()
    {
        if (project is null)
        {
            return;
        }

        titleLabel.Text = "FAT Runner";
        subtitleLabel.Text = $"{project.ContractRoot.ProjectCode} - {project.ContractRoot.ProjectName} | {project.ContractRoot.MachineModel} / {project.ContractRoot.MachineSerialNumber}";

        SetStateBadge(project.State switch
        {
            ProjectState.Executable => "Executable",
            ProjectState.Released => "Released",
            _ => "Draft"
        }, StateBackColor(project.State), StateForeColor(project.State));

        actingAsLabel.Text = activeUser is null
            ? "No project user selected"
            : $"Acting as {activeUser.DisplayName} ({activeUser.Initials})";

        accessLabel.Text = AccessSummary();
    }

    private void SetStateBadge(string text, Color backColor, Color foreColor)
    {
        stateBadge.Text = text.ToUpperInvariant();
        stateBadge.BackColor = backColor;
        stateBadge.ForeColor = foreColor;
    }

    private void RenderWork()
    {
        if (project is null)
        {
            return;
        }

        var items = FilteredWorkItems().ToList();
        RenderQueue(items);
        RenderCards(items);
        RenderHeader();

        if (items.Count == 0)
        {
            statusLabel.Text = project.State == ProjectState.Executable
                ? "No work items match this operator/filter."
                : "This project is not open for FAT execution yet.";
            return;
        }

        statusLabel.Text = $"Showing {items.Count} check(s) for {FilterSummary()}.";
    }

    private IEnumerable<RunnerWorkItem> FilteredWorkItems()
    {
        if (project is null)
        {
            yield break;
        }

        var sectionId = (sectionFilter.SelectedItem as SectionFilterItem)?.SectionId;
        var assetId = (assetFilter.SelectedItem as AssetFilterItem)?.AssetId;
        var status = (statusFilter.SelectedItem as StatusFilterItem)?.Status ?? WorkStatusFilter.Outstanding;
        foreach (var section in project.Sections.OrderBy(section => section.DisplayOrder))
        {
            if (sectionId is not null && section.SectionId != sectionId.Value)
            {
                continue;
            }

            foreach (var testItem in section.TestItems.OrderBy(testItem => testItem.DisplayOrder))
            {
                if (!CanSeeWork(section, testItem))
                {
                    continue;
                }

                var effectiveApplicability = project.EffectiveTestApplicability(section, testItem);
                if (effectiveApplicability == ApplicabilityState.NotApplicable &&
                    !showNotApplicableCheckBox.Checked &&
                    status != WorkStatusFilter.NotApplicable)
                {
                    continue;
                }

                if (assetId is not null && !TestBelongsToAssetScope(testItem, assetId.Value))
                {
                    continue;
                }

                if (!MatchesStatusFilter(testItem, effectiveApplicability, status))
                {
                    continue;
                }

                yield return new RunnerWorkItem(section, testItem, effectiveApplicability);
            }
        }
    }

    private bool TestBelongsToAssetScope(TestItem testItem, Guid assetId)
    {
        if (project is null || testItem.AssetId is null)
        {
            return false;
        }

        return project.AssetScope(assetId).Contains(testItem.AssetId.Value);
    }

    private bool MatchesStatusFilter(
        TestItem testItem,
        ApplicabilityState effectiveApplicability,
        WorkStatusFilter status)
    {
        var isNotApplicable = effectiveApplicability == ApplicabilityState.NotApplicable;
        return status switch
        {
            WorkStatusFilter.All => true,
            WorkStatusFilter.Outstanding => !isNotApplicable &&
                (testItem.LatestResult == TestResult.NotTested ||
                 testItem.LatestFailureBlocksProgression() ||
                 MissingEvidenceCount(testItem) > 0),
            WorkStatusFilter.NotTested => !isNotApplicable && testItem.LatestResult == TestResult.NotTested,
            WorkStatusFilter.Passed => !isNotApplicable &&
                testItem.LatestResult == TestResult.Pass &&
                MissingEvidenceCount(testItem) == 0,
            WorkStatusFilter.Failed => !isNotApplicable && testItem.LatestResult == TestResult.Fail,
            WorkStatusFilter.NeedsEvidence => !isNotApplicable && MissingEvidenceCount(testItem) > 0,
            WorkStatusFilter.RequiresWitness => !isNotApplicable && testItem.BehaviourRules.RequiresWitness,
            WorkStatusFilter.NotApplicable => isNotApplicable,
            _ => true
        };
    }

    private int MissingEvidenceCount(TestItem testItem)
    {
        return project?.MissingEvidenceTypes(testItem).Count ?? 0;
    }

    private string FilterSummary()
    {
        var actor = activeUser?.DisplayName ?? "operator";
        var section = (sectionFilter.SelectedItem as SectionFilterItem)?.ToString() ?? "all assigned work";
        var asset = (assetFilter.SelectedItem as AssetFilterItem)?.ToString() ?? "all assets";
        var status = (statusFilter.SelectedItem as StatusFilterItem)?.ToString() ?? "outstanding";
        return $"{actor} | {section} | {asset} | {status}";
    }

    private void RenderQueue(IReadOnlyList<RunnerWorkItem> items)
    {
        workList.BeginUpdate();
        try
        {
            workList.Items.Clear();
            foreach (var item in items)
            {
                var row = new ListViewItem(item.TestItem.TestReference);
                row.SubItems.Add(item.Section.Title);
                row.SubItems.Add(AssetHierarchyLabel(item.TestItem.AssetId));
                row.SubItems.Add(StateText(item));
                row.Tag = item;
                workList.Items.Add(row);

                if (selectedTestItemId == item.TestItem.TestItemId)
                {
                    row.Selected = true;
                    row.Focused = true;
                }
            }

            if (workList.SelectedItems.Count == 0 && workList.Items.Count > 0)
            {
                workList.Items[0].Selected = true;
                workList.Items[0].Focused = true;
                if (workList.Items[0].Tag is RunnerWorkItem first)
                {
                    selectedTestItemId = first.TestItem.TestItemId;
                }
            }
        }
        finally
        {
            workList.EndUpdate();
        }
    }

    private void RenderCards(IReadOnlyList<RunnerWorkItem> items)
    {
        cardsPanel.SuspendLayout();
        cardsPanel.Controls.Clear();

        if (items.Count == 0)
        {
            cardsPanel.Controls.Add(CreateEmptyState());
        }
        else
        {
            foreach (var item in items)
            {
                cardsPanel.Controls.Add(CreateExecutionCard(item));
            }
        }

        ResizeCards();
        cardsPanel.ResumeLayout();
        HighlightSelectedCard();
    }

    private Control CreateEmptyState()
    {
        var label = new Label
        {
            Text = "No FAT checks are available for the current operator and filters.",
            AutoSize = false,
            Height = 90,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = AppTheme.Current.TextSecondary,
            BackColor = AppTheme.Current.SurfaceAlt
        };
        return label;
    }

    private Control CreateExecutionCard(RunnerWorkItem item)
    {
        var tag = new RunnerCardTag(item.Section.SectionId, item.TestItem.TestItemId);
        var testItem = item.TestItem;
        var latest = LatestResultEntry(testItem);
        var notApplicable = item.EffectiveApplicability == ApplicabilityState.NotApplicable;
        var canExecute = CanExecuteWork(item);
        var hasInputs = testItem.Inputs.Count > 0;
        var backColor = CardBackColor(item);
        var foreColor = notApplicable ? AppTheme.Current.NotApplicableForeground : AppTheme.Current.TextPrimary;

        var card = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            Height = hasInputs ? 330 : 270,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 12, 12),
            BackColor = backColor,
            Tag = tag
        };
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
        card.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.Click += (_, _) => SelectCard(item.TestItem.TestItemId);

        var narrative = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = hasInputs ? 7 : 6,
            BackColor = backColor,
            Tag = tag
        };
        narrative.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        narrative.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        narrative.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        narrative.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        narrative.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        if (hasInputs)
        {
            narrative.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        }

        narrative.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var row = 0;
        narrative.Controls.Add(CreateCardLabel(
            $"[{testItem.TestReference}]  {PrimaryTestText(testItem)}",
            backColor,
            foreColor,
            new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            tag), 0, row++);
        narrative.Controls.Add(CreateCardLabel(
            $"{item.Section.Title} > {AssetHierarchyLabel(testItem.AssetId)}",
            backColor,
            AppTheme.Current.TextMuted,
            SystemFonts.DefaultFont,
            tag), 0, row++);
        narrative.Controls.Add(CreateCardLabel(
            "Action: " + ActionText(testItem),
            backColor,
            foreColor,
            SystemFonts.DefaultFont,
            tag), 0, row++);
        narrative.Controls.Add(CreateCardLabel(
            "Expected: " + ExpectedText(testItem),
            backColor,
            foreColor,
            SystemFonts.DefaultFont,
            tag), 0, row++);
        narrative.Controls.Add(CreateCardLabel(
            PassCriteriaText(testItem),
            backColor,
            foreColor,
            new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            tag), 0, row++);

        if (hasInputs)
        {
            narrative.Controls.Add(CreateInputsPanel(testItem, latest, canExecute, backColor, foreColor, tag), 0, row++);
        }

        if (notApplicable)
        {
            narrative.Controls.Add(CreateCardLabel(
                "Not applicable: " + (project?.EffectiveTestApplicabilityReason(item.Section, testItem) ?? "No reason recorded."),
                backColor,
                AppTheme.Current.NotApplicableForeground,
                SystemFonts.DefaultFont,
                tag), 0, row);
        }

        card.Controls.Add(narrative, 0, 0);

        var execution = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = backColor,
            Tag = tag
        };
        execution.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        execution.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        execution.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        execution.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        execution.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        execution.Controls.Add(CreateStateBlock(item, latest, backColor, foreColor, tag), 0, 0);

        var decisions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = backColor,
            Margin = new Padding(0, 0, 0, 8),
            Tag = tag
        };
        decisions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        decisions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        decisions.Controls.Add(CreateResultButton("PASS", testItem.LatestResult == TestResult.Pass, canExecute, tag, TestResult.Pass), 0, 0);
        decisions.Controls.Add(CreateResultButton("FAIL", testItem.LatestResult == TestResult.Fail, canExecute, tag, TestResult.Fail), 1, 0);
        execution.Controls.Add(decisions, 0, 1);

        execution.Controls.Add(CreateCardLabel(
            "Observation",
            backColor,
            foreColor,
            new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            tag), 0, 2);

        var observation = new TextBox
        {
            Name = "RunnerObservationTextBox",
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Text = LatestComment(testItem),
            Enabled = canExecute,
            BackColor = canExecute ? AppTheme.Current.InputBackground : AppTheme.Current.InputReadOnlyBackground,
            ForeColor = canExecute ? AppTheme.Current.InputForeground : AppTheme.Current.TextMuted,
            Tag = tag
        };
        observation.GotFocus += (_, _) => SelectCard(testItem.TestItemId);
        observation.Leave += (_, _) => CommitObservation(tag, observation.Text);
        execution.Controls.Add(observation, 0, 3);

        execution.Controls.Add(CreateEvidenceLine(testItem, canExecute, notApplicable, backColor, foreColor, tag), 0, 4);

        card.Controls.Add(execution, 1, 0);
        if (hasInputs)
        {
            WireStructuredInputEvents(card, canExecute);
            UpdateResultButtonsForRequiredInputs(card, canExecute);
        }

        return card;
    }

    private Control CreateStateBlock(RunnerWorkItem item, ResultEntry? latest, Color backColor, Color foreColor, RunnerCardTag tag)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = backColor,
            Tag = tag
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));

        panel.Controls.Add(CreateCardLabel(
            ExecutionStateText(item),
            backColor,
            StateForeColor(item),
            new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            tag), 0, 0);

        panel.Controls.Add(CreateCardLabel(
            latest is null || item.EffectiveApplicability == ApplicabilityState.NotApplicable ? "By: -" : $"By: {latest.ExecutedBy}",
            backColor,
            foreColor,
            SystemFonts.DefaultFont,
            tag), 0, 1);

        panel.Controls.Add(CreateCardLabel(
            latest is null || item.EffectiveApplicability == ApplicabilityState.NotApplicable ? "At: -" : $"At: {latest.ExecutedAt.LocalDateTime:g}",
            backColor,
            foreColor,
            SystemFonts.DefaultFont,
            tag), 0, 2);

        return panel;
    }

    private Control CreateInputsPanel(TestItem testItem, ResultEntry? latest, bool enabled, Color backColor, Color foreColor, RunnerCardTag tag)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = backColor,
            Tag = tag
        };

        var captured = latest?.CapturedInputValues.ToDictionary(value => value.TestInputId, value => value.Value)
            ?? new Dictionary<Guid, string>();
        var capturedCount = testItem.Inputs.Count(input =>
            captured.TryGetValue(input.TestInputId, out var value) &&
            !string.IsNullOrWhiteSpace(value));

        panel.Controls.Add(new Label
        {
            Text = $"Inputs {capturedCount}/{testItem.Inputs.Count}",
            AutoSize = false,
            Width = 96,
            Height = 28,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag
        });

        foreach (var input in testItem.Inputs.OrderBy(input => input.DisplayOrder))
        {
            panel.Controls.Add(CreateInputEditor(input, captured.GetValueOrDefault(input.TestInputId), enabled, backColor, foreColor, tag));
        }

        return panel;
    }

    private Control CreateInputEditor(TestInput input, string? currentValue, bool enabled, Color backColor, Color foreColor, RunnerCardTag tag)
    {
        var wrapper = new FlowLayoutPanel
        {
            AutoSize = true,
            Height = 32,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 14, 6),
            BackColor = backColor,
            Tag = tag
        };

        wrapper.Controls.Add(new Label
        {
            Text = InputLabel(input),
            AutoSize = false,
            Width = 145,
            Height = 24,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag
        });

        Control editor = input.InputType switch
        {
            TestInputType.Boolean => CreateBooleanInputEditor(currentValue, enabled),
            _ => CreateTextInputEditor(currentValue, enabled, input.InputType == TestInputType.Numeric ? 78 : 132)
        };
        editor.Tag = new InputControlTag(input.TestInputId, input.Required);
        editor.GotFocus += (_, _) => SelectCard(tag.TestItemId);
        wrapper.Controls.Add(editor);

        wrapper.Controls.Add(new Label
        {
            Text = InputContext(input),
            AutoSize = false,
            Width = input.InputType == TestInputType.Numeric ? 145 : 40,
            Height = 24,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = AppTheme.Current.TextMuted,
            BackColor = backColor,
            Tag = tag
        });

        return wrapper;
    }

    private Control CreateEvidenceLine(TestItem testItem, bool canExecute, bool notApplicable, Color backColor, Color foreColor, RunnerCardTag tag)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = backColor,
            Tag = tag
        };

        panel.Controls.Add(new Label
        {
            Text = testItem.EvidenceRecords.Count == 0 ? "Evidence: 0" : $"Evidence: {testItem.EvidenceRecords.Count} attached",
            AutoSize = false,
            Width = 160,
            Height = 34,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(SystemFonts.DefaultFont, testItem.EvidenceRecords.Count > 0 ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = testItem.EvidenceRecords.Count > 0 && !notApplicable ? AppTheme.Current.PassForeground : foreColor,
            BackColor = backColor,
            Tag = tag
        });

        var add = new Button
        {
            Text = "Add Evidence",
            AutoSize = true,
            Visible = !notApplicable,
            Cursor = canExecute ? Cursors.Hand : Cursors.No,
            Tag = tag
        };
        AppTheme.StyleButton(add, ThemeButtonKind.Secondary);
        add.Click += (_, _) =>
        {
            SelectCard(tag.TestItemId);
            if (!canExecute)
            {
                statusLabel.Text = "Project must be executable before evidence can be attached.";
                return;
            }

            AttachEvidence(testItem);
        };
        panel.Controls.Add(add);
        return panel;
    }

    private Button CreateResultButton(string text, bool active, bool canExecute, RunnerCardTag tag, TestResult result)
    {
        var palette = AppTheme.Current;
        var activeBack = result == TestResult.Pass ? palette.PassBackground : palette.FailBackground;
        var softBack = result == TestResult.Pass ? palette.PassSoftBackground : palette.FailSoftBackground;
        var backColor = active ? activeBack : softBack;

        var button = new Button
        {
            Name = "RunnerResultButton",
            Text = text,
            Dock = DockStyle.Fill,
            Enabled = true,
            Cursor = canExecute ? Cursors.Hand : Cursors.No,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = backColor,
            ForeColor = palette.TextPrimary,
            Tag = tag
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = active ? AppTheme.Lighten(activeBack, 10) : palette.ButtonBorder;
        button.FlatAppearance.MouseOverBackColor = canExecute ? AppTheme.Lighten(backColor, 8) : backColor;
        button.FlatAppearance.MouseDownBackColor = canExecute ? AppTheme.Darken(backColor, 8) : backColor;
        button.Click += (_, _) =>
        {
            SelectCard(tag.TestItemId);
            if (!canExecute)
            {
                statusLabel.Text = "This check is not available for result capture in the current state/access scope.";
                return;
            }

            var card = FindRunnerCard(button);
            if (card is not null && !RequiredInputsComplete(card))
            {
                statusLabel.Text = "Complete required structured inputs before recording a result.";
                return;
            }

            RecordResult(tag, result, FindObservationTextForCard(button), FindCapturedInputValuesForCard(button));
        };

        return button;
    }

    private void RecordResult(RunnerCardTag tag, TestResult result, string? comment, IReadOnlyList<CapturedTestInputValue> capturedInputValues)
    {
        var testItem = FindTestItem(tag.TestItemId);
        if (testItem is null)
        {
            return;
        }

        if (!TryCollectExecutionGovernance(testItem, result, out var witnessedBy, out var overrideReason))
        {
            return;
        }

        var latest = testItem?.ResultHistory.OrderByDescending(entry => entry.ExecutedAt).FirstOrDefault();
        var operation = executionService.RecordResult(new RecordResultRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = tag.TestItemId,
            Result = result,
            Comments = comment,
            CapturedInputValues = capturedInputValues,
            SupersedesResultEntryId = latest?.ResultEntryId,
            WitnessedBy = witnessedBy,
            OverrideReason = overrideReason,
            ExecutedBy = CurrentActor()
        });

        HandleOperation(operation, $"{result} recorded.");
    }

    private void CommitObservation(RunnerCardTag tag, string? comment)
    {
        var testItem = FindTestItem(tag.TestItemId);
        var latest = testItem?.ResultHistory.OrderByDescending(entry => entry.ExecutedAt).FirstOrDefault();
        if (latest is null || latest.Result is not TestResult.Pass and not TestResult.Fail)
        {
            return;
        }

        if (string.Equals(latest.Comments ?? string.Empty, comment ?? string.Empty, StringComparison.Ordinal))
        {
            return;
        }

        var operation = executionService.RecordResult(new RecordResultRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = tag.TestItemId,
            Result = latest.Result,
            Comments = comment,
            CapturedInputValues = latest.CapturedInputValues,
            SupersedesResultEntryId = latest.ResultEntryId,
            WitnessedBy = latest.WitnessedBy,
            OverrideReason = latest.OverrideReason,
            ExecutedBy = CurrentActor()
        });

        HandleOperation(operation, "Observation recorded.");
    }

    private bool TryCollectExecutionGovernance(
        TestItem testItem,
        TestResult result,
        out string? witnessedBy,
        out string? overrideReason)
    {
        witnessedBy = null;
        overrideReason = null;

        if (!ExecutionGovernanceForm.IsRequiredFor(testItem, result))
        {
            return true;
        }

        using var form = new ExecutionGovernanceForm(testItem, result, CurrentActor());
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        witnessedBy = form.WitnessedBy;
        overrideReason = form.OverrideReason;
        return true;
    }

    private void AttachEvidence(TestItem testItem)
    {
        using var dialog = new OpenFileDialog
        {
            Title = $"Attach evidence - {testItem.TestReference}",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var operation = executionService.AttachEvidence(new AttachEvidenceRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = testItem.TestItemId,
            SourceFilePath = dialog.FileName,
            EvidenceType = SuggestedEvidenceType(testItem.EvidenceRequirements, dialog.FileName),
            AttachedBy = CurrentActor()
        });

        HandleOperation(operation, "Evidence attached.");
    }

    private static EvidenceType SuggestedEvidenceType(EvidenceRequirements requirements, string filePath)
    {
        var requiredType = requirements.RequiredEvidenceTypes().FirstOrDefault();
        if (requiredType != EvidenceType.Other)
        {
            return requiredType;
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif"
            ? EvidenceType.Photo
            : EvidenceType.FileUpload;
    }

    private void HandleOperation(OperationResult operation, string successMessage)
    {
        if (!operation.Succeeded)
        {
            statusLabel.Text = BuildErrorText(operation);
            return;
        }

        statusLabel.Text = successMessage;
        LoadProject();
    }

    private bool CanSeeWork(Section section, TestItem testItem)
    {
        if (project is null || activeUser is null)
        {
            return true;
        }

        var assignments = ActiveAssignments().ToList();
        if (assignments.Count == 0)
        {
            return true;
        }

        if (HasProjectWideAccess(assignments))
        {
            return true;
        }

        return assignments.Any(assignment =>
            IsWorkRole(assignment.Role) &&
            ((assignment.ScopeType == AuthorityScopeType.Section && assignment.ScopeId == section.SectionId) ||
             (assignment.ScopeType == AuthorityScopeType.TestItem && assignment.ScopeId == testItem.TestItemId)));
    }

    private bool CanExecuteWork(RunnerWorkItem item)
    {
        if (project?.State != ProjectState.Executable || item.EffectiveApplicability == ApplicabilityState.NotApplicable)
        {
            return false;
        }

        if (activeUser is null)
        {
            return false;
        }

        var assignments = ActiveAssignments().ToList();
        if (assignments.Count == 0)
        {
            return true;
        }

        if (assignments.Any(assignment =>
                assignment.Role is AuthorityRole.Admin or AuthorityRole.LeadTestEngineer or AuthorityRole.TestExecutor &&
                assignment.ScopeType == AuthorityScopeType.Project &&
                assignment.ScopeId == project.ProjectId))
        {
            return true;
        }

        return assignments.Any(assignment =>
            assignment.Role == AuthorityRole.TestExecutor &&
            ((assignment.ScopeType == AuthorityScopeType.Section && assignment.ScopeId == item.Section.SectionId) ||
             (assignment.ScopeType == AuthorityScopeType.TestItem && assignment.ScopeId == item.TestItem.TestItemId)));
    }

    private IEnumerable<AuthorityAssignment> ActiveAssignments()
    {
        if (project is null || activeUser is null)
        {
            return [];
        }

        return project.AuthorityAssignments.Where(assignment =>
            assignment.IsActive &&
            assignment.UserId == activeUser.UserId);
    }

    private bool HasProjectWideAccess(IReadOnlyList<AuthorityAssignment> assignments)
    {
        return project is not null && assignments.Any(assignment =>
            assignment.ScopeType == AuthorityScopeType.Project &&
            assignment.ScopeId == project.ProjectId &&
            assignment.Role is AuthorityRole.Admin or
                AuthorityRole.LeadTestEngineer or
                AuthorityRole.ReleaseAuthority or
                AuthorityRole.SectionApprover or
                AuthorityRole.TestExecutor);
    }

    private static bool IsWorkRole(AuthorityRole role)
    {
        return role is AuthorityRole.Admin or
            AuthorityRole.LeadTestEngineer or
            AuthorityRole.TestExecutor or
            AuthorityRole.SectionApprover or
            AuthorityRole.Witness;
    }

    private string AccessSummary()
    {
        if (project is null)
        {
            return string.Empty;
        }

        if (activeUser is null)
        {
            return "No project identity selected";
        }

        var assignments = ActiveAssignments().ToList();
        if (assignments.Count == 0)
        {
            return "Development access: all checks visible";
        }

        var projectWide = HasProjectWideAccess(assignments);
        if (projectWide)
        {
            return "Project access";
        }

        var sectionCount = assignments.Count(assignment => assignment.ScopeType == AuthorityScopeType.Section);
        var testCount = assignments.Count(assignment => assignment.ScopeType == AuthorityScopeType.TestItem);
        return $"Scoped access: {sectionCount} section(s), {testCount} test(s)";
    }

    private void OpenSwitchUserDialog()
    {
        if (project is null)
        {
            return;
        }

        using var form = new SwitchUserForm(project, activeUser?.UserId);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            var picked = project.Users.FirstOrDefault(user => user.UserId == form.SelectedUserId);
            if (picked is not null)
            {
                activeUser = ActiveUserContext.FromUser(picked);
                selectedTestItemId = null;
                RenderWork();
                statusLabel.Text = $"Acting as {picked.DisplayName}.";
            }
        }
    }

    private void SelectCard(Guid testItemId)
    {
        selectedTestItemId = testItemId;
        foreach (ListViewItem row in workList.Items)
        {
            if (row.Tag is RunnerWorkItem item && item.TestItem.TestItemId == testItemId)
            {
                row.Selected = true;
                row.Focused = true;
                row.EnsureVisible();
                break;
            }
        }

        HighlightSelectedCard();
    }

    private void HighlightSelectedCard()
    {
        foreach (Control control in cardsPanel.Controls)
        {
            if (control.Tag is not RunnerCardTag tag)
            {
                continue;
            }

            control.Padding = tag.TestItemId == selectedTestItemId
                ? new Padding(13)
                : new Padding(16);
        }
    }

    private void ResizeCards()
    {
        var width = Math.Max(620, cardsPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 32);
        foreach (Control control in cardsPanel.Controls)
        {
            control.Width = width;
        }
    }

    private TestItem? FindTestItem(Guid testItemId)
    {
        return project?.Sections
            .SelectMany(section => section.TestItems)
            .SingleOrDefault(testItem => testItem.TestItemId == testItemId);
    }

    private Asset? FindAsset(Guid assetId)
    {
        return project?.Assets.SingleOrDefault(asset => asset.AssetId == assetId);
    }

    private string AssetHierarchyLabel(Guid? assetId)
    {
        if (assetId is null)
        {
            return "-";
        }

        var asset = FindAsset(assetId.Value);
        if (asset is null)
        {
            return "Unknown asset";
        }

        if (asset.ParentAssetId is null)
        {
            return asset.Name;
        }

        var parent = FindAsset(asset.ParentAssetId.Value);
        return parent is null ? asset.Name : $"{parent.Name} -> {asset.Name}";
    }

    private Color CardBackColor(RunnerWorkItem item)
    {
        if (item.EffectiveApplicability == ApplicabilityState.NotApplicable)
        {
            return AppTheme.Current.NotApplicableBackground;
        }

        return item.TestItem.LatestResult switch
        {
            TestResult.Pass => AppTheme.Current.PassSoftBackground,
            TestResult.Fail => AppTheme.Current.FailSoftBackground,
            _ => AppTheme.Current.SurfaceElevated
        };
    }

    private static Color StateForeColor(RunnerWorkItem item)
    {
        if (item.EffectiveApplicability == ApplicabilityState.NotApplicable)
        {
            return AppTheme.Current.NotApplicableForeground;
        }

        return item.TestItem.LatestResult switch
        {
            TestResult.Pass => AppTheme.Current.PassForeground,
            TestResult.Fail => AppTheme.Current.FailForeground,
            _ => AppTheme.Current.TextSecondary
        };
    }

    private static string StateText(RunnerWorkItem item)
    {
        if (item.EffectiveApplicability == ApplicabilityState.NotApplicable)
        {
            return "N/A";
        }

        return item.TestItem.LatestResult switch
        {
            TestResult.Pass => "Pass",
            TestResult.Fail => "Fail",
            _ => "Not tested"
        };
    }

    private static string ExecutionStateText(RunnerWorkItem item)
    {
        if (item.EffectiveApplicability == ApplicabilityState.NotApplicable)
        {
            return "Status: NOT APPLICABLE";
        }

        if (item.TestItem.ResultHistory.Count == 0)
        {
            return "Status: Not tested";
        }

        var prefix = item.TestItem.ResultHistory.Count > 1 ? "Status: Retested -> " : "Status: ";
        return item.TestItem.LatestResult switch
        {
            TestResult.Pass => prefix + "Passed",
            TestResult.Fail => prefix + "Failed",
            TestResult.NotApplicable => "Legacy not-applicable result",
            _ => "Status: Not tested"
        };
    }

    private static ResultEntry? LatestResultEntry(TestItem testItem)
    {
        return testItem.ResultHistory
            .OrderByDescending(result => result.ExecutedAt)
            .FirstOrDefault();
    }

    private static string LatestComment(TestItem testItem)
    {
        return LatestResultEntry(testItem)?.Comments ?? string.Empty;
    }

    private static string PrimaryTestText(TestItem testItem)
    {
        var primary = string.IsNullOrWhiteSpace(testItem.TestDescription)
            ? testItem.TestTitle
            : testItem.TestDescription;
        return string.Equals(primary, testItem.TestTitle, StringComparison.OrdinalIgnoreCase)
            ? primary
            : $"{primary} | {testItem.TestTitle}";
    }

    private static string ActionText(TestItem testItem)
    {
        if (!string.IsNullOrWhiteSpace(testItem.TestDescription))
        {
            return testItem.TestDescription.Trim();
        }

        if (!string.IsNullOrWhiteSpace(testItem.TestTitle))
        {
            return "Perform " + testItem.TestTitle.Trim().ToLowerInvariant() + ".";
        }

        return "Perform this test and record the observed outcome.";
    }

    private static string ExpectedText(TestItem testItem)
    {
        return string.IsNullOrWhiteSpace(testItem.ExpectedOutcome)
            ? "Condition should match the approved test definition."
            : testItem.ExpectedOutcome.Trim();
    }

    private static string PassCriteriaText(TestItem testItem)
    {
        return testItem.AcceptanceCriteria.ConditionType switch
        {
            PassConditionType.NumericRange => "Pass criteria: Pass if measured value is " +
                $"{testItem.AcceptanceCriteria.TargetValue:0.###}{UnitSuffix(testItem.AcceptanceCriteria.Unit)} +/- {testItem.AcceptanceCriteria.Tolerance:0.###}.",
            PassConditionType.BooleanCondition => "Pass criteria: Pass when condition is TRUE.",
            _ => "Pass criteria: Pass if condition matches expected result."
        };
    }

    private static string UnitSuffix(string? unit)
    {
        return string.IsNullOrWhiteSpace(unit) ? string.Empty : " " + unit.Trim();
    }

    private static string InputLabel(TestInput input)
    {
        var required = input.Required ? " *" : string.Empty;
        return $"{input.Label}{required}";
    }

    private static string InputContext(TestInput input)
    {
        if (input.InputType != TestInputType.Numeric)
        {
            return string.Empty;
        }

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(input.Unit))
        {
            parts.Add(input.Unit.Trim());
        }

        if (input.TargetValue is not null)
        {
            var target = $"target {input.TargetValue:0.###}";
            if (input.Tolerance is not null)
            {
                target += $" +/- {input.Tolerance:0.###}";
            }

            parts.Add(target);
        }

        return string.Join(" ", parts);
    }

    private static TextBox CreateTextInputEditor(string? currentValue, bool enabled, int width)
    {
        return new TextBox
        {
            Text = currentValue ?? string.Empty,
            Enabled = enabled,
            Width = width,
            Height = 24,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = enabled ? AppTheme.Current.InputBackground : AppTheme.Current.InputReadOnlyBackground,
            ForeColor = enabled ? AppTheme.Current.InputForeground : AppTheme.Current.TextMuted
        };
    }

    private static ComboBox CreateBooleanInputEditor(string? currentValue, bool enabled)
    {
        var combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = enabled,
            Width = 76,
            Height = 24,
            FlatStyle = FlatStyle.Flat,
            BackColor = enabled ? AppTheme.Current.InputBackground : AppTheme.Current.InputReadOnlyBackground,
            ForeColor = enabled ? AppTheme.Current.InputForeground : AppTheme.Current.TextMuted
        };
        combo.Items.Add("");
        combo.Items.Add("Yes");
        combo.Items.Add("No");
        combo.SelectedItem = string.Equals(currentValue, "true", StringComparison.OrdinalIgnoreCase)
            ? "Yes"
            : string.Equals(currentValue, "false", StringComparison.OrdinalIgnoreCase)
                ? "No"
                : "";
        return combo;
    }

    private Label CreateCardLabel(string text, Color backColor, Color foreColor, Font font, RunnerCardTag tag)
    {
        var label = new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = font,
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag,
            UseMnemonic = false
        };
        label.Click += (_, _) => SelectCard(tag.TestItemId);
        return label;
    }

    private static Control? FindRunnerCard(Control child)
    {
        var parent = child.Parent;
        while (parent?.Parent is not null && parent.Parent is not FlowLayoutPanel)
        {
            parent = parent.Parent;
        }

        return parent;
    }

    private static string? FindObservationTextForCard(Control child)
    {
        var parent = FindRunnerCard(child);
        return parent?.Controls
            .Cast<Control>()
            .SelectMany(DescendantControls)
            .OfType<TextBox>()
            .FirstOrDefault(textBox => textBox.Name == "RunnerObservationTextBox")
            ?.Text;
    }

    private static IReadOnlyList<CapturedTestInputValue> FindCapturedInputValuesForCard(Control child)
    {
        var parent = FindRunnerCard(child);
        if (parent is null)
        {
            return [];
        }

        var captured = new List<CapturedTestInputValue>();
        foreach (var control in parent.Controls.Cast<Control>().SelectMany(DescendantControls))
        {
            if (control.Tag is not InputControlTag tag)
            {
                continue;
            }

            var value = control switch
            {
                ComboBox combo => combo.SelectedItem switch
                {
                    "Yes" => "true",
                    "No" => "false",
                    _ => string.Empty
                },
                TextBox textBox => textBox.Text,
                _ => string.Empty
            };

            captured.Add(CapturedTestInputValue.Create(tag.TestInputId, value));
        }

        return captured;
    }

    private static void WireStructuredInputEvents(Control card, bool canExecute)
    {
        foreach (var control in card.Controls.Cast<Control>().SelectMany(DescendantControls))
        {
            if (control.Tag is not InputControlTag)
            {
                continue;
            }

            switch (control)
            {
                case TextBox textBox:
                    textBox.TextChanged += (_, _) => UpdateResultButtonsForRequiredInputs(card, canExecute);
                    break;
                case ComboBox comboBox:
                    comboBox.SelectedIndexChanged += (_, _) => UpdateResultButtonsForRequiredInputs(card, canExecute);
                    break;
            }
        }
    }

    private static void UpdateResultButtonsForRequiredInputs(Control card, bool canExecute)
    {
        var canRecord = canExecute && RequiredInputsComplete(card);
        foreach (var button in card.Controls
                     .Cast<Control>()
                     .SelectMany(DescendantControls)
                     .OfType<Button>()
                     .Where(button => button.Name == "RunnerResultButton"))
        {
            button.Cursor = canRecord ? Cursors.Hand : Cursors.No;
        }
    }

    private static bool RequiredInputsComplete(Control card)
    {
        var requiredControls = card.Controls
            .Cast<Control>()
            .SelectMany(DescendantControls)
            .Where(control => control.Tag is InputControlTag { Required: true })
            .ToList();

        return requiredControls.All(control => control switch
        {
            ComboBox comboBox => comboBox.SelectedItem is "Yes" or "No",
            TextBox textBox => !string.IsNullOrWhiteSpace(textBox.Text),
            _ => false
        });
    }

    private static IEnumerable<Control> DescendantControls(Control control)
    {
        foreach (Control child in control.Controls)
        {
            yield return child;
            foreach (var descendant in DescendantControls(child))
            {
                yield return descendant;
            }
        }
    }

    private static string BuildErrorText(OperationResult result)
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            lines.Add(result.ErrorMessage);
            lines.Add("");
        }

        foreach (var issue in result.Validation.Issues)
        {
            lines.Add($"{issue.TargetField}: {issue.Message}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private string CurrentActor()
    {
        if (activeUser is not null && !string.IsNullOrWhiteSpace(activeUser.DisplayName))
        {
            return activeUser.DisplayName;
        }

        return string.IsNullOrWhiteSpace(Environment.UserName)
            ? "Local Operator"
            : Environment.UserName;
    }

    private static Control CreateSurface(Control content, Padding padding, bool elevated = false)
    {
        var border = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            Margin = new Padding(0),
            BackColor = AppTheme.Current.Border
        };

        var surface = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = padding,
            BackColor = elevated ? AppTheme.Current.SurfaceElevated : AppTheme.Current.Surface
        };

        content.Dock = DockStyle.Fill;
        surface.Controls.Add(content);
        border.Controls.Add(surface);
        return border;
    }

    private static Color StateBackColor(ProjectState state)
    {
        return state switch
        {
            ProjectState.DraftContractBuilt => AppTheme.Current.StatusDraftBackground,
            ProjectState.Executable => AppTheme.Current.StatusExecutableBackground,
            ProjectState.Released => AppTheme.Current.StatusReleasedBackground,
            _ => AppTheme.Current.SurfaceElevated
        };
    }

    private static Color StateForeColor(ProjectState state)
    {
        return state switch
        {
            ProjectState.DraftContractBuilt => AppTheme.Current.StatusDraftForeground,
            ProjectState.Executable => AppTheme.Current.StatusExecutableForeground,
            ProjectState.Released => AppTheme.Current.StatusReleasedForeground,
            _ => AppTheme.Current.TextPrimary
        };
    }

    private sealed record RunnerWorkItem(Section Section, TestItem TestItem, ApplicabilityState EffectiveApplicability);

    private sealed record RunnerCardTag(Guid SectionId, Guid TestItemId);

    private sealed record InputControlTag(Guid TestInputId, bool Required);

    private enum WorkStatusFilter
    {
        Outstanding,
        All,
        NotTested,
        Passed,
        Failed,
        NeedsEvidence,
        RequiresWitness,
        NotApplicable
    }

    private sealed class SectionFilterItem
    {
        public SectionFilterItem(Guid? sectionId, string text)
        {
            SectionId = sectionId;
            Text = text;
        }

        public Guid? SectionId { get; }
        private string Text { get; }

        public override string ToString()
        {
            return Text;
        }
    }

    private sealed class AssetFilterItem
    {
        public AssetFilterItem(Guid? assetId, string text)
        {
            AssetId = assetId;
            Text = text;
        }

        public Guid? AssetId { get; }
        private string Text { get; }

        public override string ToString()
        {
            return Text;
        }
    }

    private sealed class StatusFilterItem
    {
        public StatusFilterItem(WorkStatusFilter status, string text)
        {
            Status = status;
            Text = text;
        }

        public WorkStatusFilter Status { get; }
        private string Text { get; }

        public override string ToString()
        {
            return Text;
        }
    }
}
