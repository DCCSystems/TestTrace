using System.Diagnostics;
using TestTrace_V1;
using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;
using TestTrace_V1.Workspace;

namespace TestTrace_V1.UI;

public sealed class ProjectWorkspaceForm : Form
{
    private readonly string projectFolderPath;
    private readonly JsonProjectRepository repository = new();
    private readonly WorkspaceService workspaceService;
    private readonly ExecutionService executionService;
    private readonly ApprovalService approvalService;
    private readonly ExportService exportService;
    private readonly SplitContainer workspaceSplit = new();
    private readonly SplitContainer contentSplit = new();
    private readonly TreeView workspaceTree = new();
    private readonly Label titleLabel = new();
    private readonly Label stateLabel = new();
    private readonly Label customerLabel = new();
    private readonly Label machineLabel = new();
    private readonly Label folderLabel = new();
    private readonly Label lifecycleBannerLabel = new();
    private readonly Label statusLabel = new();
    private readonly TextBox readinessTextBox = new();
    private readonly TableLayoutPanel projectDetailsPanel = CreateDetailsTable();
    private readonly TableLayoutPanel selectionDetailsPanel = CreateDetailsTable();
    private readonly TableLayoutPanel contextDetailsPanel = CreateDetailsTable();
    private readonly FlowLayoutPanel overviewCardsPanel = new();
    private readonly TableLayoutPanel overviewDetailsPanel = CreateDetailsTable();
    private readonly ListView assetModeList = CreateListView(("Asset", 240), ("Type", 130), ("Parent", 220), ("Sections", 100), ("Tests", 100), ("Evidence", 100), ("Metadata", 280));
    private readonly ListView triageModeList = CreateListView(("State", 130), ("Test", 300), ("Section", 180), ("Asset", 240), ("Evidence", 100), ("Latest", 240));
    private readonly TableLayoutPanel reportDetailsPanel = CreateDetailsTable();
    private readonly FlowLayoutPanel executionSheetPanel = new();
    private readonly CheckBox showNotApplicableCheckBox = new();
    private readonly Button sheetMarkNotApplicableButton = new();
    private readonly Button sheetMarkApplicableButton = new();
    private readonly ListView resultList = CreateListView(("Result", 120), ("By", 160), ("When", 160), ("Measured", 220), ("Comments", 360));
    private readonly ListView evidenceList = CreateListView(("Stored file", 260), ("Original", 220), ("Type", 110), ("Attached by", 150), ("SHA-256", 460));
    private readonly ListView auditList = CreateListView(("When", 160), ("Actor", 160), ("Action", 180), ("Target", 160), ("Details", 460));
    private readonly ListView contextEvidenceList = CreateListView(("File name", 190), ("Type", 80), ("Added by", 120), ("Timestamp", 130));
    private readonly ListView contextHistoryList = CreateListView(("Result", 90), ("By", 120), ("When", 130), ("Notes", 300));
    private readonly Button addSectionButton = new();
    private readonly Button addComponentButton = new();
    private readonly Button addSubAssetButton = new();
    private readonly Button addTestItemButton = new();
    private readonly Button renameButton = new();
    private readonly Button deleteButton = new();
    private readonly Button openForExecutionButton = new();
    private readonly Button recordResultButton = new();
    private readonly Button attachEvidenceButton = new();
    private readonly Button approveSectionButton = new();
    private readonly Button releaseProjectButton = new();
    private readonly Button exportReportButton = new();
    private readonly Button startRunnerButton = new();
    private readonly Button editProjectMetadataButton = new();
    private readonly ContextMenuStrip structureContextMenu = new();
    private readonly ToolTip actionToolTip = new();

    private TestTraceProject? project;
    private ReadinessReport? readiness;
    private bool renderingExecutionSheet;
    private bool syncingExecutionSheetSelection;
    private ActiveUserContext? activeUser;
    private readonly Label actingAsValueLabel = new();
    private readonly Button switchUserButton = new() { Text = "Switch...", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
    private readonly Button manageUsersButton = new() { Text = "Users & Authority...", AutoSize = true, Margin = new Padding(8, 0, 0, 0), UseMnemonic = false };

    public ProjectWorkspaceForm(string projectFolderPath)
    {
        this.projectFolderPath = projectFolderPath;
        workspaceService = new WorkspaceService(repository);
        executionService = new ExecutionService(repository);
        approvalService = new ApprovalService(repository);
        exportService = new ExportService(repository);

        Text = $"TestTrace Workspace [{TestTraceAppEnvironment.ModeLabel}]";
        MinimumSize = new Size(1100, 720);
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
            RowCount = 5,
            Padding = new Padding(16)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        titleLabel.AutoSize = true;
        titleLabel.Font = new Font(Font.FontFamily, 17, FontStyle.Bold);
        titleLabel.Margin = new Padding(0, 0, 0, 10);

        var summary = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 4,
            AutoSize = true,
            Margin = new Padding(0)
        };
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        AddSummaryRow(summary, 0, "State", stateLabel, "Customer", customerLabel);
        AddSummaryRow(summary, 1, "Machine", machineLabel, "Folder", folderLabel);

        var summarySurface = CreateShellSurface(summary, new Padding(12));
        summarySurface.Margin = new Padding(0, 0, 0, 10);

        lifecycleBannerLabel.Dock = DockStyle.Fill;
        lifecycleBannerLabel.AutoSize = false;
        lifecycleBannerLabel.Height = 36;
        lifecycleBannerLabel.TextAlign = ContentAlignment.MiddleLeft;
        lifecycleBannerLabel.Padding = new Padding(10, 0, 10, 0);
        lifecycleBannerLabel.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        lifecycleBannerLabel.Margin = new Padding(0, 0, 0, 12);

        workspaceTree.Dock = DockStyle.Fill;
        workspaceTree.HideSelection = false;
        workspaceTree.AfterSelect += (_, _) =>
        {
            if (syncingExecutionSheetSelection)
            {
                RenderSupportingDetails();
            }
            else
            {
                RenderDetails();
            }

            UpdateButtonState();
        };
        workspaceTree.NodeMouseClick += WorkspaceTreeNodeMouseClick;

        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(0)
        };
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftPanel.Controls.Add(CreatePanelTitle("Structure"), 0, 0);
        leftPanel.Controls.Add(CreateStructureToolbar(), 0, 1);
        leftPanel.Controls.Add(workspaceTree, 0, 2);

        var leftSurface = CreateShellSurface(leftPanel, new Padding(12), elevated: true);

        var readinessSurface = CreateShellSurface(CreateReadinessPanel(), new Padding(12), elevated: true);
        readinessSurface.Margin = new Padding(0, 0, 0, 10);

        var detailsSurface = CreateShellSurface(CreateDetailsTabs(), new Padding(12), elevated: true);

        var centerPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0)
        };
        centerPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        centerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        centerPanel.Controls.Add(readinessSurface, 0, 0);
        centerPanel.Controls.Add(detailsSurface, 0, 1);

        var contextSurface = CreateShellSurface(CreateContextPanel(), new Padding(12), elevated: true);

        contentSplit.Dock = DockStyle.Fill;
        contentSplit.Orientation = Orientation.Vertical;
        contentSplit.Panel1.Padding = new Padding(0, 0, 12, 0);
        contentSplit.SizeChanged += (_, _) => EnsureContentSplitterDistance();
        contentSplit.Panel1.Controls.Add(centerPanel);
        contentSplit.Panel2.Controls.Add(contextSurface);

        workspaceSplit.Dock = DockStyle.Fill;
        workspaceSplit.Orientation = Orientation.Vertical;
        workspaceSplit.Panel1.Padding = new Padding(0, 0, 12, 0);
        workspaceSplit.SizeChanged += (_, _) => EnsureWorkspaceSplitterDistance();
        workspaceSplit.Panel1.Controls.Add(leftSurface);
        workspaceSplit.Panel2.Controls.Add(contentSplit);

        var actionSurface = CreateShellSurface(CreateActionPanel(), new Padding(12), elevated: true);
        actionSurface.Margin = new Padding(0, 12, 0, 0);

        layout.Controls.Add(CreateHeaderRow(), 0, 0);
        layout.Controls.Add(summarySurface, 0, 1);
        layout.Controls.Add(lifecycleBannerLabel, 0, 2);
        layout.Controls.Add(workspaceSplit, 0, 3);
        layout.Controls.Add(actionSurface, 0, 4);

        Controls.Add(layout);
        Shown += (_, _) =>
        {
            EnsureWorkspaceSplitterDistance();
            EnsureContentSplitterDistance();
        };
    }

    private Control CreateReadinessPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(CreatePanelTitle("Readiness"), 0, 0);

        readinessTextBox.Dock = DockStyle.Fill;
        readinessTextBox.Multiline = true;
        readinessTextBox.ReadOnly = true;
        readinessTextBox.ScrollBars = ScrollBars.Vertical;
        readinessTextBox.MinimumSize = new Size(0, 108);
        readinessTextBox.Margin = new Padding(0);
        readinessTextBox.BackColor = AppTheme.Current.InputReadOnlyBackground;
        readinessTextBox.ForeColor = AppTheme.Current.TextPrimary;
        panel.Controls.Add(readinessTextBox, 0, 1);
        return panel;
    }

    private Control CreateDetailsTabs()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill
        };
        tabs.TabPages.Add(CreateTab("Overview", CreateOverviewMode()));
        tabs.TabPages.Add(CreateTab("Execute", CreateExecutionSheetTab()));
        tabs.TabPages.Add(CreateTab("Assets", CreateAssetsMode()));
        tabs.TabPages.Add(CreateTab("Triage", CreateTriageMode()));
        tabs.TabPages.Add(CreateTab("Report", CreateReportMode()));
        return tabs;
    }

    private Control CreateOverviewMode()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(CreateModeIntro(
            "Command Centre",
            "Project state, execution health, evidence pressure, and approval readiness at a glance."), 0, 0);

        overviewCardsPanel.Dock = DockStyle.Top;
        overviewCardsPanel.AutoSize = true;
        overviewCardsPanel.FlowDirection = FlowDirection.LeftToRight;
        overviewCardsPanel.WrapContents = true;
        overviewCardsPanel.Margin = new Padding(0, 0, 0, 12);
        layout.Controls.Add(overviewCardsPanel, 0, 1);

        layout.Controls.Add(overviewDetailsPanel, 0, 2);
        return layout;
    }

    private Control CreateAssetsMode()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(CreateModeIntro(
            "Machine Assets",
            "Asset and sub-asset inventory with section use, linked tests, evidence volume, and metadata coverage."), 0, 0);
        layout.Controls.Add(assetModeList, 0, 1);
        return layout;
    }

    private Control CreateTriageMode()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(CreateModeIntro(
            "Execution Triage",
            "A state-led work list for tests that need execution, evidence, retest, or scope review."), 0, 0);
        layout.Controls.Add(triageModeList, 0, 1);
        return layout;
    }

    private Control CreateReportMode()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(CreateModeIntro(
            "Report Record",
            "Contract metadata, selected execution records, evidence, and audit activity that feed the FAT report."), 0, 0);

        var reportTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        reportTabs.TabPages.Add(CreateTab("Report Status", reportDetailsPanel));
        reportTabs.TabPages.Add(CreateTab("Contract", CreateProjectTab()));
        reportTabs.TabPages.Add(CreateTab("Selection", selectionDetailsPanel));
        reportTabs.TabPages.Add(CreateTab("Results", resultList));
        reportTabs.TabPages.Add(CreateTab("Evidence", evidenceList));
        reportTabs.TabPages.Add(CreateTab("Activity", auditList));
        layout.Controls.Add(reportTabs, 0, 1);
        return layout;
    }

    private static Control CreateModeIntro(string title, string description)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 14)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 13, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 3),
            UseMnemonic = false
        }, 0, 0);
        layout.Controls.Add(new Label
        {
            Text = description,
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            ForeColor = AppTheme.Current.TextSecondary,
            Margin = new Padding(0, 0, 0, 0),
            UseMnemonic = false
        }, 0, 1);
        return layout;
    }

    private Control CreateProjectTab()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        ConfigureButton(editProjectMetadataButton, "Edit Project Metadata", EditProjectMetadata);
        editProjectMetadataButton.Margin = new Padding(0);
        toolbar.Controls.Add(editProjectMetadataButton);

        var note = new Label
        {
            Text = "Contract identity is read-only here. Machine context locks when execution opens. Reporting and contact details remain editable until release.",
            AutoSize = true,
            Margin = new Padding(12, 6, 0, 0),
            ForeColor = AppTheme.Current.TextSecondary
        };
        toolbar.Controls.Add(note);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(projectDetailsPanel, 0, 1);
        return layout;
    }

    private Control CreateContextPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(CreatePanelTitle("Selection Details"), 0, 0);

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        tabs.TabPages.Add(CreateTab("Details", contextDetailsPanel));
        tabs.TabPages.Add(CreateTab("Evidence", contextEvidenceList));
        tabs.TabPages.Add(CreateTab("History", contextHistoryList));
        contextEvidenceList.DoubleClick += (_, _) => OpenSelectedContextEvidence();
        panel.Controls.Add(tabs, 0, 1);
        return panel;
    }

    private Control CreateExecutionSheetTab()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        showNotApplicableCheckBox.Text = "Show not applicable items";
        showNotApplicableCheckBox.AutoSize = true;
        showNotApplicableCheckBox.Margin = new Padding(0, 2, 16, 10);
        showNotApplicableCheckBox.CheckedChanged += (_, _) =>
        {
            RenderExecutionSheet();
            UpdateButtonState();
        };

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 12)
        };
        ConfigureButton(sheetMarkNotApplicableButton, "Mark Not Applicable", MarkSelectedNotApplicable);
        ConfigureButton(sheetMarkApplicableButton, "Return to Scope", MarkSelectedApplicable);
        toolbar.Controls.Add(showNotApplicableCheckBox);
        toolbar.Controls.Add(sheetMarkNotApplicableButton);
        toolbar.Controls.Add(sheetMarkApplicableButton);
        layout.Controls.Add(toolbar, 0, 0);

        ConfigureExecutionSheetPanel();
        layout.Controls.Add(executionSheetPanel, 0, 1);

        return layout;
    }

    private void ConfigureExecutionSheetPanel()
    {
        executionSheetPanel.Dock = DockStyle.Fill;
        executionSheetPanel.AutoScroll = true;
        executionSheetPanel.FlowDirection = FlowDirection.TopDown;
        executionSheetPanel.WrapContents = false;
        executionSheetPanel.BackColor = AppTheme.Current.SurfaceAlt;
        executionSheetPanel.Padding = new Padding(12);
        executionSheetPanel.Resize += (_, _) => ResizeExecutionStrips();
    }

    private Control CreateStructureToolbar()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 8)
        };

        ConfigureToolbarButton(addSectionButton, "Add Section", AddSection);
        ConfigureToolbarButton(addComponentButton, "Add Asset", AddAsset);
        ConfigureToolbarButton(addSubAssetButton, "Add Sub-Asset", AddSubAsset);
        ConfigureToolbarButton(addTestItemButton, "Add Test Item", AddTestItem);
        ConfigureToolbarButton(renameButton, "Rename", RenameSelected);
        ConfigureToolbarButton(deleteButton, "Delete", DeleteSelected);

        toolbar.Controls.Add(addSectionButton);
        toolbar.Controls.Add(addComponentButton);
        toolbar.Controls.Add(addSubAssetButton);
        toolbar.Controls.Add(addTestItemButton);
        toolbar.Controls.Add(renameButton);
        toolbar.Controls.Add(deleteButton);
        return toolbar;
    }

    private Control CreateActionPanel()
    {
        var outer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true,
            Margin = new Padding(0)
        };
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0)
        };

        ConfigureButton(openForExecutionButton, "Open for FAT Execution", OpenForExecution);
        ConfigureButton(recordResultButton, "Record Result", RecordResult);
        ConfigureButton(attachEvidenceButton, "Attach Evidence", AttachEvidence);
        ConfigureButton(approveSectionButton, "Approve Section", ApproveSelectedSection);
        ConfigureButton(releaseProjectButton, "Release Project", ReleaseProject);
        ConfigureButton(exportReportButton, "Export FAT Report", ExportReport);
        ConfigureButton(startRunnerButton, "Start FAT Runner", OpenFatRunner);

        manageUsersButton.Margin = new Padding(8, 0, 0, 0);
        manageUsersButton.UseMnemonic = false;
        manageUsersButton.Click += (_, _) => OpenUsersAuthorityDialog();

        var refreshButton = new Button { Text = "Refresh", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        refreshButton.Click += (_, _) => LoadProject();

        var openFolderButton = new Button { Text = "Open Folder", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        openFolderButton.Click += (_, _) => OpenFolder();

        actions.Controls.Add(openForExecutionButton);
        actions.Controls.Add(recordResultButton);
        actions.Controls.Add(attachEvidenceButton);
        actions.Controls.Add(approveSectionButton);
        actions.Controls.Add(releaseProjectButton);
        actions.Controls.Add(exportReportButton);
        actions.Controls.Add(startRunnerButton);
        actions.Controls.Add(manageUsersButton);
        actions.Controls.Add(refreshButton);
        actions.Controls.Add(openFolderButton);

        statusLabel.AutoSize = true;
        statusLabel.Anchor = AnchorStyles.Left;
        statusLabel.Margin = new Padding(0, 8, 0, 0);
        statusLabel.ForeColor = AppTheme.Current.TextSecondary;

        outer.Controls.Add(actions, 0, 0);
        outer.Controls.Add(statusLabel, 0, 1);
        return outer;
    }

    private static void ConfigureButton(Button button, string text, Action handler)
    {
        button.Text = text;
        button.AutoSize = true;
        button.Margin = new Padding(8, 0, 0, 0);
        button.Click += (_, _) => handler();
    }

    private static void ConfigureToolbarButton(Button button, string text, Action handler)
    {
        button.Text = text;
        button.AutoSize = true;
        button.Margin = new Padding(0, 0, 6, 6);
        button.Click += (_, _) => handler();
    }

    private static TabPage CreateTab(string title, Control control)
    {
        var tab = new TabPage(title);
        control.Dock = DockStyle.Fill;
        tab.Controls.Add(control);
        return tab;
    }

    private void EnsureWorkspaceSplitterDistance()
    {
        if (workspaceSplit.Width <= 0)
        {
            return;
        }

        const int desiredLeftPanelWidth = 300;
        const int minimumLeftPanelWidth = 260;
        const int minimumRightPanelWidth = 620;

        var min = minimumLeftPanelWidth;
        var max = workspaceSplit.Width - minimumRightPanelWidth;
        if (max < min)
        {
            return;
        }

        var target = Math.Min(desiredLeftPanelWidth, max);
        target = Math.Max(min, target);
        if (workspaceSplit.SplitterDistance != target)
        {
            workspaceSplit.SplitterDistance = target;
        }

        EnsureContentSplitterDistance();
    }

    private void EnsureContentSplitterDistance()
    {
        if (contentSplit.Width <= 0)
        {
            return;
        }

        const int desiredContextPanelWidth = 410;
        const int minimumCenterPanelWidth = 620;
        const int minimumContextPanelWidth = 320;

        var min = minimumCenterPanelWidth;
        var max = contentSplit.Width - minimumContextPanelWidth;
        if (max < min)
        {
            return;
        }

        var target = contentSplit.Width - desiredContextPanelWidth;
        target = Math.Max(min, Math.Min(target, max));
        if (contentSplit.SplitterDistance != target)
        {
            contentSplit.SplitterDistance = target;
        }
    }

    private void LoadProject()
    {
        try
        {
            project = repository.Load(ProjectLocation.FromProjectFolder(projectFolderPath));
            RenderProject();
            EnsureActiveUserSelected(promptIfMissing: activeUser is null);
            statusLabel.Text = TestTraceAppEnvironment.IsSandbox
                ? "Sandbox project loaded."
                : "Project loaded.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Could not load project", MessageBoxButtons.OK, MessageBoxIcon.Error);
            statusLabel.Text = "Project load failed.";
        }
    }

    private void RenderProject()
    {
        if (project is null)
        {
            return;
        }

        titleLabel.Text = $"{project.ContractRoot.ProjectCode} - {project.ContractRoot.ProjectName}";
        stateLabel.Text = project.State.ToString();
        customerLabel.Text = project.ContractRoot.CustomerName;
        machineLabel.Text = $"{project.ContractRoot.MachineModel} / {project.ContractRoot.MachineSerialNumber}";
        folderLabel.Text = projectFolderPath;
        RenderLifecycleBanner(project);

        workspaceTree.BeginUpdate();
        workspaceTree.Nodes.Clear();

        var rootNode = new TreeNode($"{project.ContractRoot.ProjectCode} - {project.ContractRoot.ProjectName}")
        {
            Tag = new ProjectNodeTag(project.ProjectId)
        };

        foreach (var section in project.Sections.OrderBy(section => section.DisplayOrder))
        {
            var approvalSuffix = section.Approval is null ? "" : " [Approved]";
            var sectionApplicabilitySuffix = section.Applicability == ApplicabilityState.NotApplicable ? " [N/A]" : "";
            var sectionNode = new TreeNode($"{section.DisplayOrder}. {section.Title}{sectionApplicabilitySuffix}{approvalSuffix}")
            {
                Tag = new SectionNodeTag(section.SectionId)
            };
            ApplyApplicabilityStyle(sectionNode, section.Applicability == ApplicabilityState.NotApplicable);

            foreach (var sectionAsset in section.Assets.OrderBy(asset => asset.DisplayOrder))
            {
                var asset = FindAsset(sectionAsset.AssetId);
                var assetName = asset?.Name ?? "Unknown asset";
                var assetApplicabilitySuffix = sectionAsset.Applicability == ApplicabilityState.NotApplicable ? " [N/A]" : "";
                var assetNode = new TreeNode($"{sectionAsset.DisplayOrder}. {assetName}{assetApplicabilitySuffix}")
                {
                    Tag = new AssetNodeTag(section.SectionId, sectionAsset.AssetId, false)
                };
                ApplyApplicabilityStyle(assetNode, section.Applicability == ApplicabilityState.NotApplicable || sectionAsset.Applicability == ApplicabilityState.NotApplicable);

                foreach (var testItem in section.TestItems
                             .Where(testItem => testItem.AssetId == sectionAsset.AssetId)
                             .OrderBy(testItem => testItem.DisplayOrder))
                {
                    assetNode.Nodes.Add(CreateTestNode(section, sectionAsset.AssetId, testItem));
                }

                if (asset is not null)
                {
                    foreach (var child in project.ChildAssets(asset.AssetId))
                    {
                        var inheritedApplicabilitySuffix = sectionAsset.Applicability == ApplicabilityState.NotApplicable ? " [N/A inherited]" : "";
                        var childNode = new TreeNode(child.Name + inheritedApplicabilitySuffix)
                        {
                            Tag = new AssetNodeTag(section.SectionId, child.AssetId, true)
                        };
                        ApplyApplicabilityStyle(childNode, section.Applicability == ApplicabilityState.NotApplicable || sectionAsset.Applicability == ApplicabilityState.NotApplicable);

                        foreach (var testItem in section.TestItems
                                     .Where(testItem => testItem.AssetId == child.AssetId)
                                     .OrderBy(testItem => testItem.DisplayOrder))
                        {
                            childNode.Nodes.Add(CreateTestNode(section, child.AssetId, testItem));
                        }

                        assetNode.Nodes.Add(childNode);
                    }
                }

                sectionNode.Nodes.Add(assetNode);
            }

            foreach (var testItem in section.TestItems
                         .Where(testItem => testItem.AssetId is null)
                         .OrderBy(testItem => testItem.DisplayOrder))
            {
                sectionNode.Nodes.Add(CreateTestNode(section, null, testItem));
            }

            rootNode.Nodes.Add(sectionNode);
        }

        workspaceTree.Nodes.Add(rootNode);
        workspaceTree.SelectedNode ??= rootNode;
        workspaceTree.ExpandAll();
        workspaceTree.EndUpdate();
        RenderDetails();
        UpdateButtonState();
    }

    private void RenderLifecycleBanner(TestTraceProject currentProject)
    {
        lifecycleBannerLabel.Text = currentProject.State switch
        {
            ProjectState.DraftContractBuilt => "DRAFT CONTRACT - define sections and test items, then open for execution.",
            ProjectState.Executable => "READY FOR EXECUTION - record results, attach evidence, approve sections, then release.",
            ProjectState.Released => "RELEASED - project is closed and read-only.",
            _ => currentProject.State.ToString()
        };

        lifecycleBannerLabel.BackColor = currentProject.State switch
        {
            ProjectState.DraftContractBuilt => AppTheme.Current.StatusDraftBackground,
            ProjectState.Executable => AppTheme.Current.StatusExecutableBackground,
            ProjectState.Released => AppTheme.Current.StatusReleasedBackground,
            _ => AppTheme.Current.SurfaceAlt
        };
        lifecycleBannerLabel.ForeColor = currentProject.State switch
        {
            ProjectState.DraftContractBuilt => AppTheme.Current.StatusDraftForeground,
            ProjectState.Executable => AppTheme.Current.StatusExecutableForeground,
            ProjectState.Released => AppTheme.Current.StatusReleasedForeground,
            _ => AppTheme.Current.TextPrimary
        };
    }

    private TreeNode CreateTestNode(Section section, Guid? assetId, TestItem testItem)
    {
        var evidenceText = testItem.EvidenceRecords.Count == 0 ? "" : $" | Evidence: {testItem.EvidenceRecords.Count}";
        var effectiveApplicability = project?.EffectiveTestApplicability(section, testItem) ?? ApplicabilityState.Applicable;
        var applicabilityText = effectiveApplicability == ApplicabilityState.NotApplicable ? " [N/A]" : "";
        var node = new TreeNode($"{testItem.DisplayOrder}. {testItem.TestReference} - {testItem.TestTitle}{applicabilityText} [{FormatLatestResult(testItem)}]{evidenceText}")
        {
            Tag = new TestNodeTag(section.SectionId, assetId, testItem.TestItemId)
        };
        ApplyApplicabilityStyle(node, effectiveApplicability == ApplicabilityState.NotApplicable);
        return node;
    }

    private static void ApplyApplicabilityStyle(TreeNode node, bool notApplicable)
    {
        if (notApplicable)
        {
            node.ForeColor = AppTheme.Current.TextMuted;
        }
    }

    private void WorkspaceTreeNodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node is null)
        {
            return;
        }

        workspaceTree.SelectedNode = e.Node;
        RenderDetails();
        UpdateButtonState();

        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        BuildStructureContextMenu(e.Node);
        structureContextMenu.Show(workspaceTree, e.Location);
    }

    private void BuildStructureContextMenu(TreeNode node)
    {
        structureContextMenu.Items.Clear();
        structureContextMenu.ShowItemToolTips = true;

        switch (node.Tag)
        {
            case ProjectNodeTag:
                AddContextItem("Add Section", CanAddSectionNow(), StructureBlockedReason(), AddSection);
                AddContextItem("Add Asset", CanAddAssetNow(), AddAssetBlockedReason(), AddAsset);
                break;

            case SectionNodeTag:
                AddContextItem("Add Asset", CanAddAssetNow(), AddAssetBlockedReason(), AddAsset);
                AddContextItem("Mark Not Applicable", CanChangeApplicabilityNow(), ApplicabilityBlockedReason(), MarkSelectedNotApplicable);
                AddContextItem("Return to Scope", CanChangeApplicabilityNow(), ApplicabilityBlockedReason(), MarkSelectedApplicable);
                AddContextItem("Rename Section", CanRenameSelectedNow(), StructureBlockedReason(), RenameSelected);
                AddContextItem("Delete Section", CanDeleteSelectedNow(), StructureBlockedReason(), DeleteSelected);
                break;

            case AssetNodeTag assetTag:
                if (!assetTag.IsSubAsset)
                {
                    AddContextItem("Add Sub-Asset", CanAddSubAssetNow(), AddSubAssetBlockedReason(), AddSubAsset);
                }

                AddContextItem("Add Test Item", CanAddTestItemNow(), AddTestItemBlockedReason(), AddTestItem);
                AddContextItem("Mark Not Applicable", CanChangeApplicabilityNow(), ApplicabilityBlockedReason(), MarkSelectedNotApplicable);
                AddContextItem("Return to Scope", CanChangeApplicabilityNow(), ApplicabilityBlockedReason(), MarkSelectedApplicable);
                AddContextItem(assetTag.IsSubAsset ? "Rename Sub-Asset" : "Rename Asset", CanRenameSelectedNow(), StructureBlockedReason(), RenameSelected);
                AddContextItem("Edit Metadata", CanRenameSelectedNow(), StructureBlockedReason(), EditSelectedAssetMetadata);
                AddContextItem(assetTag.IsSubAsset ? "Delete Sub-Asset" : "Remove Asset From Section", CanDeleteSelectedNow(), StructureBlockedReason(), DeleteSelected);
                if (!assetTag.IsSubAsset)
                {
                    AddContextItem("Delete Asset Globally", CanDeleteSelectedNow(), StructureBlockedReason(), DeleteAssetGlobally);
                }
                break;

            case TestNodeTag:
                AddContextItem("Mark Not Applicable", CanChangeApplicabilityNow(), ApplicabilityBlockedReason(), MarkSelectedNotApplicable);
                AddContextItem("Return to Scope", CanChangeApplicabilityNow(), ApplicabilityBlockedReason(), MarkSelectedApplicable);
                AddContextItem("Rename Test Item", CanRenameSelectedNow(), StructureBlockedReason(), RenameSelected);
                AddContextItem("Delete Test Item", CanDeleteSelectedNow(), StructureBlockedReason(), DeleteSelected);
                break;

            default:
                AddContextItem("Add Section", CanAddSectionNow(), StructureBlockedReason(), AddSection);
                break;
        }
    }

    private void AddContextItem(string text, bool enabled, string blockedReason, Action handler)
    {
        var item = new ToolStripMenuItem(enabled ? text : $"{text} - {blockedReason}")
        {
            Enabled = enabled,
            ToolTipText = blockedReason
        };
        item.Click += (_, _) => handler();
        structureContextMenu.Items.Add(item);
    }

    private void RenderDetails()
    {
        if (project is null)
        {
            return;
        }

        var selectedSection = SelectedSection();
        var selectedAsset = SelectedAsset();
        var selectedTest = SelectedTestItem();
        readiness = ProjectReadinessService.Evaluate(project, selectedSection, selectedAsset, selectedTest);

        RenderExecutionSheet();
        RenderSupportingDetails();
    }

    private void RenderSupportingDetails()
    {
        if (project is null)
        {
            return;
        }

        var selectedSection = SelectedSection();
        var selectedAsset = SelectedAsset();
        var selectedTest = SelectedTestItem();
        readiness = ProjectReadinessService.Evaluate(project, selectedSection, selectedAsset, selectedTest);

        RenderProjectDetails(project);
        RenderModePages(project, selectedSection, selectedAsset, selectedTest);
        RenderSelectionDetails(selectedSection, selectedAsset, selectedTest);
        RenderResults(selectedTest);
        RenderEvidence(selectedTest);
        RenderAudit(project);
        RenderContextPanel(selectedSection, selectedAsset, selectedTest);
        RenderReadiness(readiness);
    }

    private void RenderExecutionSheet()
    {
        renderingExecutionSheet = true;
        executionSheetPanel.SuspendLayout();
        executionSheetPanel.Controls.Clear();

        if (project is null)
        {
            executionSheetPanel.ResumeLayout();
            renderingExecutionSheet = false;
            return;
        }

        foreach (var section in project.Sections.OrderBy(section => section.DisplayOrder))
        {
            foreach (var testItem in section.TestItems.OrderBy(testItem => testItem.DisplayOrder))
            {
                var effectiveApplicability = project.EffectiveTestApplicability(section, testItem);
                if (effectiveApplicability == ApplicabilityState.NotApplicable && !showNotApplicableCheckBox.Checked)
                {
                    continue;
                }

                executionSheetPanel.Controls.Add(CreateExecutionStrip(section, testItem, effectiveApplicability));
            }
        }

        ResizeExecutionStrips();
        executionSheetPanel.ResumeLayout();
        renderingExecutionSheet = false;
    }

    private Control CreateExecutionStrip(Section section, TestItem testItem, ApplicabilityState effectiveApplicability)
    {
        var tag = new TestNodeTag(section.SectionId, testItem.AssetId, testItem.TestItemId);
        var notApplicable = effectiveApplicability == ApplicabilityState.NotApplicable;
        var canExecute = project?.State == ProjectState.Executable && !notApplicable;
        var latest = LatestResultEntry(testItem);
        var backColor = testItem.LatestResult switch
        {
            TestResult.Pass when !notApplicable => AppTheme.Current.PassSoftBackground,
            TestResult.Fail when !notApplicable => AppTheme.Current.FailSoftBackground,
            _ when notApplicable => AppTheme.Current.NotApplicableBackground,
            _ => AppTheme.Current.SurfaceElevated
        };
        var foreColor = notApplicable ? AppTheme.Current.NotApplicableForeground : AppTheme.Current.TextPrimary;
        var hasInputs = testItem.Inputs.Count > 0;
        var applicabilityReason = project?.EffectiveTestApplicabilityReason(section, testItem);

        var strip = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            Height = hasInputs ? 360 : 300,
            Margin = new Padding(0, 0, 12, 10),
            Padding = new Padding(12),
            BackColor = backColor,
            Tag = tag
        };
        strip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        strip.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
        strip.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        strip.Click += (_, _) => SelectExecutionStrip(tag);

        var narrativePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = hasInputs ? 7 : 6,
            BackColor = backColor,
            Tag = tag
        };
        narrativePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        narrativePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        narrativePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        narrativePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        narrativePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        if (hasInputs)
        {
            narrativePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        }
        narrativePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        narrativePanel.Click += (_, _) => SelectExecutionStrip(tag);

        var row = 0;
        narrativePanel.Controls.Add(CreateStripLabel(
            $"[{testItem.TestReference}]  {PrimaryTestText(testItem)}",
            backColor,
            foreColor,
            new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            28,
            tag), 0, row++);
        narrativePanel.Controls.Add(CreateStripLabel(
            $"{section.Title} > {AssetHierarchyLabel(testItem.AssetId)}",
            backColor,
            AppTheme.Current.TextMuted,
            SystemFonts.DefaultFont,
            22,
            tag), 0, row++);
        narrativePanel.Controls.Add(CreateStripLabel(
            "Action: " + ActionText(testItem),
            backColor,
            foreColor,
            SystemFonts.DefaultFont,
            44,
            tag), 0, row++);
        narrativePanel.Controls.Add(CreateStripLabel(
            "Expected: " + ExpectedText(testItem),
            backColor,
            foreColor,
            SystemFonts.DefaultFont,
            40,
            tag), 0, row++);
        narrativePanel.Controls.Add(CreateStripLabel(
            PassCriteriaText(testItem),
            backColor,
            foreColor,
            new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            36,
            tag), 0, row++);

        if (hasInputs)
        {
            narrativePanel.Controls.Add(CreateStructuredInputsPanel(testItem, latest, canExecute, backColor, foreColor, tag), 0, row++);
        }

        if (notApplicable)
        {
            narrativePanel.Controls.Add(CreateStripLabel(
                "Not applicable reason: " + (applicabilityReason ?? "No reason recorded."),
                backColor,
                AppTheme.Current.TextMuted,
                SystemFonts.DefaultFont,
                28,
                tag), 0, row);
        }

        strip.Controls.Add(narrativePanel, 0, 0);

        var executionPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = backColor,
            Tag = tag
        };
        executionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        executionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        executionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        executionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        executionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        executionPanel.Click += (_, _) => SelectExecutionStrip(tag);
        executionPanel.Controls.Add(CreateExecutionStatePanel(testItem, latest, notApplicable, applicabilityReason, backColor, foreColor, tag), 0, 0);

        var decisionPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = backColor,
            Margin = new Padding(0, 0, 0, 6),
            Tag = tag
        };
        decisionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        decisionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        decisionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        decisionPanel.Controls.Add(CreateResultButton("PASS", testItem.LatestResult == TestResult.Pass, canExecute, tag, TestResult.Pass), 0, 0);
        decisionPanel.Controls.Add(CreateResultButton("FAIL", testItem.LatestResult == TestResult.Fail, canExecute, tag, TestResult.Fail), 1, 0);
        executionPanel.Controls.Add(decisionPanel, 0, 1);

        executionPanel.Controls.Add(CreateStripLabel(
            "Observation:",
            backColor,
            foreColor,
            new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            22,
            tag), 0, 2);

        var observation = new TextBox
        {
            Name = "ExecutionObservationTextBox",
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Text = LatestComment(testItem),
            Enabled = canExecute,
            BackColor = canExecute ? AppTheme.Current.InputBackground : AppTheme.Current.InputReadOnlyBackground,
            ForeColor = foreColor,
            Tag = tag
        };
        observation.GotFocus += (_, _) => SelectExecutionStrip(tag);
        observation.Leave += (_, _) => CommitExecutionSheetObservation(tag, observation.Text);
        observation.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && e.Control)
            {
                CommitExecutionSheetObservation(tag, observation.Text);
                e.SuppressKeyPress = true;
            }
        };
        executionPanel.Controls.Add(observation, 0, 3);

        var evidencePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = backColor,
            Tag = tag
        };

        var evidenceCount = new Label
        {
            Text = $"Evidence: {testItem.EvidenceRecords.Count}",
            AutoSize = false,
            Width = 128,
            Height = 32,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(SystemFonts.DefaultFont, testItem.EvidenceRecords.Count > 0 ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = testItem.EvidenceRecords.Count > 0 && !notApplicable ? AppTheme.Current.PassForeground : foreColor,
            BackColor = backColor,
            Tag = tag
        };
        evidenceCount.Click += (_, _) => SelectExecutionStrip(tag);
        evidencePanel.Controls.Add(evidenceCount);

        var addEvidence = new Button
        {
            Text = "Add",
            AutoSize = true,
            Enabled = true,
            Visible = !notApplicable,
            Cursor = canExecute ? Cursors.Hand : Cursors.No,
            Tag = tag
        };
        AppTheme.StyleButton(addEvidence, ThemeButtonKind.Secondary);
        addEvidence.Click += (_, _) =>
        {
            SelectExecutionStrip(tag);
            if (!canExecute)
            {
                statusLabel.Text = "Project must be executable before evidence can be attached.";
                return;
            }

            AttachEvidenceInline(testItem);
        };
        evidencePanel.Controls.Add(addEvidence);
        executionPanel.Controls.Add(evidencePanel, 0, 4);

        strip.Controls.Add(executionPanel, 1, 0);
        if (hasInputs)
        {
            WireStructuredInputEvents(strip, canExecute);
            UpdateResultButtonsForRequiredInputs(strip, canExecute);
        }

        return strip;
    }

    private Control CreateStructuredInputsPanel(TestItem testItem, ResultEntry? latest, bool enabled, Color backColor, Color foreColor, TestNodeTag tag)
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

        var summary = new Label
        {
            Text = $"Inputs {capturedCount}/{testItem.Inputs.Count}",
            AutoSize = false,
            Width = 92,
            Height = 26,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag
        };
        summary.Click += (_, _) => SelectExecutionStrip(tag);
        panel.Controls.Add(summary);

        foreach (var input in testItem.Inputs.OrderBy(input => input.DisplayOrder))
        {
            panel.Controls.Add(CreateStructuredInputEditor(input, captured.GetValueOrDefault(input.TestInputId), enabled, backColor, foreColor, tag));
        }

        return panel;
    }

    private Control CreateStructuredInputEditor(TestInput input, string? currentValue, bool enabled, Color backColor, Color foreColor, TestNodeTag tag)
    {
        var wrapper = new FlowLayoutPanel
        {
            AutoSize = true,
            Height = 30,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 14, 6),
            BackColor = backColor,
            Tag = tag
        };

        var label = new Label
        {
            Text = InputLabel(input),
            AutoSize = false,
            Width = 145,
            Height = 24,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag
        };
        label.Click += (_, _) => SelectExecutionStrip(tag);
        wrapper.Controls.Add(label);

        Control editor = input.InputType switch
        {
            TestInputType.Boolean => CreateBooleanInputEditor(currentValue, enabled),
            _ => CreateTextInputEditor(currentValue, enabled, input.InputType == TestInputType.Numeric ? 78 : 132)
        };
        editor.Tag = new InputControlTag(input.TestInputId, input.Required);
        editor.GotFocus += (_, _) => SelectExecutionStrip(tag);
        wrapper.Controls.Add(editor);

        var context = new Label
        {
            Text = InputContext(input),
            AutoSize = false,
            Width = input.InputType == TestInputType.Numeric ? 130 : 36,
            Height = 24,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag
        };
        context.Click += (_, _) => SelectExecutionStrip(tag);
        wrapper.Controls.Add(context);
        return wrapper;
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

    private static Label CreateStripLabel(
        string text,
        Color backColor,
        Color foreColor,
        Font font,
        int height,
        TestNodeTag tag)
    {
        var label = new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = height,
            MaximumSize = new Size(0, height),
            Font = font,
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag
        };
        label.Click += (_, _) => SelectStripFromLabel(label);
        return label;
    }

    private static void SelectStripFromLabel(Control label)
    {
        if (label.FindForm() is ProjectWorkspaceForm form && label.Tag is TestNodeTag tag)
        {
            form.SelectExecutionStrip(tag);
        }
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

    private Control CreateExecutionStatePanel(
        TestItem testItem,
        ResultEntry? latest,
        bool notApplicable,
        string? applicabilityReason,
        Color backColor,
        Color foreColor,
        TestNodeTag tag)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = backColor,
            Tag = tag
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Click += (_, _) => SelectExecutionStrip(tag);

        var state = new Label
        {
            Text = ExecutionStateText(testItem, notApplicable),
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 26,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            ForeColor = ExecutionStateForeColor(testItem, notApplicable),
            BackColor = backColor,
            Tag = tag
        };
        state.Click += (_, _) => SelectExecutionStrip(tag);
        panel.Controls.Add(state, 0, 0);

        var attribution = new Label
        {
            Text = notApplicable
                ? "Reason: " + (applicabilityReason ?? "No reason recorded.")
                : latest is null ? "By: -" : $"By: {latest.ExecutedBy}",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 20,
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag
        };
        attribution.Click += (_, _) => SelectExecutionStrip(tag);
        panel.Controls.Add(attribution, 0, 1);

        var timestamp = new Label
        {
            Text = notApplicable || latest is null ? "At: -" : $"At: {latest.ExecutedAt.LocalDateTime:g}",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 20,
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag
        };
        timestamp.Click += (_, _) => SelectExecutionStrip(tag);
        panel.Controls.Add(timestamp, 0, 2);

        var measured = new Label
        {
            Text = latest is null || string.IsNullOrWhiteSpace(latest.MeasuredValue) ? string.Empty : $"Measured: {latest.MeasuredValue}",
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = foreColor,
            BackColor = backColor,
            Tag = tag
        };
        measured.Click += (_, _) => SelectExecutionStrip(tag);
        panel.Controls.Add(measured, 0, 3);
        return panel;
    }

    private Button CreateResultButton(string text, bool active, bool canExecute, TestNodeTag tag, TestResult result)
    {
        var palette = AppTheme.Current;
        var activeBackColor = result == TestResult.Pass ? palette.PassBackground : palette.FailBackground;
        var inactiveBackColor = result == TestResult.Pass ? palette.PassSoftBackground : palette.FailSoftBackground;
        var buttonBackColor = active ? activeBackColor : inactiveBackColor;

        var button = new Button
        {
            Name = "ExecutionResultButton",
            Text = text,
            Dock = DockStyle.Fill,
            Enabled = true,
            Cursor = canExecute ? Cursors.Hand : Cursors.No,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = buttonBackColor,
            ForeColor = palette.TextPrimary,
            Tag = tag
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = active ? AppTheme.Lighten(activeBackColor, 10) : palette.ButtonBorder;
        button.FlatAppearance.MouseOverBackColor = canExecute ? AppTheme.Lighten(buttonBackColor, 8) : buttonBackColor;
        button.FlatAppearance.MouseDownBackColor = canExecute ? AppTheme.Darken(buttonBackColor, 8) : buttonBackColor;
        button.Click += (_, _) =>
        {
            SelectExecutionStrip(tag);
            if (!canExecute)
            {
                statusLabel.Text = "Project must be executable before results can be recorded.";
                return;
            }

            var strip = FindExecutionStrip(button);
            if (strip is not null && !RequiredInputsComplete(strip))
            {
                statusLabel.Text = "Complete required structured inputs before recording a result.";
                return;
            }

            var comment = FindObservationTextForStrip(button);
            var capturedInputValues = FindCapturedInputValuesForStrip(button);
            RecordExecutionSheetResult(tag, result, comment, capturedInputValues);
        };
        return button;
    }

    private static Control? FindExecutionStrip(Control child)
    {
        var parent = child.Parent;
        while (parent?.Parent is not null && parent.Parent is not FlowLayoutPanel)
        {
            parent = parent.Parent;
        }

        return parent;
    }

    private static string? FindObservationTextForStrip(Control child)
    {
        var parent = FindExecutionStrip(child);

        return parent?.Controls
            .Cast<Control>()
            .SelectMany(DescendantControls)
            .OfType<TextBox>()
            .FirstOrDefault(textBox => textBox.Name == "ExecutionObservationTextBox")
            ?.Text;
    }

    private static void WireStructuredInputEvents(Control strip, bool canExecute)
    {
        foreach (var control in strip.Controls.Cast<Control>().SelectMany(DescendantControls))
        {
            if (control.Tag is not InputControlTag)
            {
                continue;
            }

            switch (control)
            {
                case TextBox textBox:
                    textBox.TextChanged += (_, _) => UpdateResultButtonsForRequiredInputs(strip, canExecute);
                    break;
                case ComboBox comboBox:
                    comboBox.SelectedIndexChanged += (_, _) => UpdateResultButtonsForRequiredInputs(strip, canExecute);
                    break;
            }
        }
    }

    private static void UpdateResultButtonsForRequiredInputs(Control strip, bool canExecute)
    {
        var canRecord = canExecute && RequiredInputsComplete(strip);
        foreach (var button in strip.Controls
                     .Cast<Control>()
                     .SelectMany(DescendantControls)
                     .OfType<Button>()
                     .Where(button => button.Name == "ExecutionResultButton"))
        {
            button.Cursor = canRecord ? Cursors.Hand : Cursors.No;
        }
    }

    private static bool RequiredInputsComplete(Control strip)
    {
        var requiredControls = strip.Controls
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

    private static IReadOnlyList<CapturedTestInputValue> FindCapturedInputValuesForStrip(Control child)
    {
        var parent = FindExecutionStrip(child);
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

    private void SelectExecutionStrip(TestNodeTag tag)
    {
        if (renderingExecutionSheet)
        {
            return;
        }

        ActiveControl = null;
        var targetNode = FindNodeByTestId(workspaceTree.Nodes, tag.TestItemId);
        if (targetNode is not null)
        {
            syncingExecutionSheetSelection = true;
            try
            {
                workspaceTree.SelectedNode = targetNode;
                targetNode.EnsureVisible();
            }
            finally
            {
                syncingExecutionSheetSelection = false;
            }
        }

        HighlightSelectedExecutionStrip(tag.TestItemId);
        RenderSupportingDetails();
        UpdateButtonState();
    }

    private void HighlightSelectedExecutionStrip(Guid testItemId)
    {
        foreach (Control control in executionSheetPanel.Controls)
        {
            if (control.Tag is not TestNodeTag tag)
            {
                continue;
            }

            control.Padding = tag.TestItemId == testItemId
                ? new Padding(10)
                : new Padding(12);
        }
    }

    private void ResizeExecutionStrips()
    {
        var width = Math.Max(520, executionSheetPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 24);
        foreach (Control control in executionSheetPanel.Controls)
        {
            control.Width = width;
        }
    }

    private void RecordExecutionSheetResult(
        TestNodeTag tag,
        TestResult selectedResult,
        string? comment,
        IReadOnlyList<CapturedTestInputValue> capturedInputValues)
    {
        if (project is null)
        {
            return;
        }

        var testItem = FindTestItem(tag.TestItemId);
        if (testItem is null)
        {
            return;
        }

        if (!TryCollectExecutionGovernance(testItem, selectedResult, out var witnessedBy, out var overrideReason))
        {
            return;
        }

        var latest = testItem?.ResultHistory.OrderByDescending(result => result.ExecutedAt).FirstOrDefault();
        var result = executionService.RecordResult(new RecordResultRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = tag.TestItemId,
            Result = selectedResult,
            Comments = comment,
            CapturedInputValues = capturedInputValues,
            SupersedesResultEntryId = latest?.ResultEntryId,
            WitnessedBy = witnessedBy,
            OverrideReason = overrideReason,
            ExecutedBy = CurrentActor()
        });

        HandleInlineOperationResult(result, $"{selectedResult} recorded from execution sheet.");
    }

    private void CommitExecutionSheetObservation(TestNodeTag tag, string? comment)
    {
        if (project is null)
        {
            return;
        }

        var testItem = FindTestItem(tag.TestItemId);
        var latest = testItem?.ResultHistory.OrderByDescending(result => result.ExecutedAt).FirstOrDefault();
        if (latest is null || latest.Result is not TestResult.Pass and not TestResult.Fail)
        {
            statusLabel.Text = "Choose Pass or Fail before recording an observation.";
            return;
        }

        if (string.Equals(latest.Comments ?? string.Empty, comment ?? string.Empty, StringComparison.Ordinal))
        {
            return;
        }

        var result = executionService.RecordResult(new RecordResultRequest
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

        HandleInlineOperationResult(result, "Observation recorded from execution sheet.");
    }

    private void AttachEvidenceInline(TestItem testItem)
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

        var result = executionService.AttachEvidence(new AttachEvidenceRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = testItem.TestItemId,
            SourceFilePath = dialog.FileName,
            EvidenceType = SuggestedEvidenceType(testItem.EvidenceRequirements, dialog.FileName),
            AttachedBy = CurrentActor()
        });

        HandleInlineOperationResult(result, "Evidence attached from execution sheet.");
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

    private void HandleInlineOperationResult(OperationResult result, string successMessage)
    {
        if (!result.Succeeded)
        {
            statusLabel.Text = BuildErrorText(result);
            return;
        }

        statusLabel.Text = successMessage;
        LoadProject();
    }

    private static TreeNode? FindNodeByTestId(TreeNodeCollection nodes, Guid testItemId)
    {
        foreach (TreeNode node in nodes)
        {
            if (node.Tag is TestNodeTag tag && tag.TestItemId == testItemId)
            {
                return node;
            }

            var childMatch = FindNodeByTestId(node.Nodes, testItemId);
            if (childMatch is not null)
            {
                return childMatch;
            }
        }

        return null;
    }

    private void RenderProjectDetails(TestTraceProject currentProject)
    {
        var metadata = currentProject.ReportingMetadata;
        SetKeyValues(projectDetailsPanel,
            ("Project Type", currentProject.ContractRoot.ProjectType),
            ("Project Name", currentProject.ContractRoot.ProjectName),
            ("Project Code", currentProject.ContractRoot.ProjectCode),
            ("State", FormatProjectState(currentProject.State)),
            ("Customer", currentProject.ContractRoot.CustomerName),
            ("Project Scope", currentProject.ContractRoot.ScopeNarrative),
            ("Machine Model", currentProject.ContractRoot.MachineModel),
            ("Machine Serial Number", currentProject.ContractRoot.MachineSerialNumber),
            ("Lead Test Engineer", currentProject.ContractRoot.LeadTestEngineer),
            ("Release Authority", currentProject.ContractRoot.ReleaseAuthority),
            ("Final Approval Authority", currentProject.ContractRoot.FinalApprovalAuthority),
            ("Created By", currentProject.ContractRoot.CreatedBy),
            ("Created At", currentProject.ContractRoot.CreatedAt.LocalDateTime.ToString("g")),
            ("Project Start Date", FormatDate(metadata.ProjectStartDate)),
            ("Customer Address", metadata.CustomerAddress ?? "-"),
            ("Customer Country", metadata.CustomerCountry ?? "-"),
            ("Site Contact Name", metadata.SiteContactName ?? "-"),
            ("Site Contact Email", metadata.SiteContactEmail ?? "-"),
            ("Site Contact Number", metadata.SiteContactPhone ?? "-"),
            ("Customer Project Reference Name / Number", metadata.CustomerProjectReference ?? "-"),
            ("Machine Configuration / Specification", metadata.MachineConfigurationSpecification ?? "-"),
            ("Control Platform", metadata.ControlPlatform ?? "-"),
            ("Machine Role / Application", metadata.MachineRoleApplication ?? "-"),
            ("Software Version", metadata.SoftwareVersion ?? "-"),
            ("Lead Test Engineer Email", metadata.LeadTestEngineerEmail ?? "-"),
            ("Lead Test Engineer Phone", metadata.LeadTestEngineerPhone ?? "-"),
            ("Sections", currentProject.Sections.Count.ToString()),
            ("Assets", currentProject.Assets.Count.ToString()),
            ("Local Users", currentProject.Users.Count.ToString()),
            ("Authority Assignments", currentProject.AuthorityAssignments.Count(assignment => assignment.IsActive).ToString()),
            ("Activity entries", currentProject.AuditLog.Count.ToString()));
    }

    private void RenderModePages(TestTraceProject currentProject, Section? selectedSection, Asset? selectedAsset, TestItem? selectedTest)
    {
        var scopedTests = AllScopedTests(currentProject).ToList();
        var applicableTests = scopedTests
            .Where(item => currentProject.EffectiveTestApplicability(item.Section, item.TestItem) != ApplicabilityState.NotApplicable)
            .ToList();
        var notApplicableTests = scopedTests.Count - applicableTests.Count;
        var passed = applicableTests.Count(item => item.TestItem.LatestResult == TestResult.Pass);
        var failed = applicableTests.Count(item => item.TestItem.LatestResult == TestResult.Fail);
        var notTested = applicableTests.Count(item => item.TestItem.LatestResult == TestResult.NotTested);
        var needsEvidence = applicableTests.Count(item => currentProject.MissingEvidenceTypes(item.TestItem).Count > 0);

        RenderOverviewCards(currentProject, applicableTests.Count, passed, failed, notTested, needsEvidence);
        RenderOverviewDetails(currentProject, selectedSection, selectedAsset, selectedTest, scopedTests.Count, applicableTests.Count, notApplicableTests);
        RenderAssetMode(currentProject);
        RenderTriageMode(currentProject, scopedTests);
        RenderReportMode(currentProject, scopedTests.Count, applicableTests.Count, passed, failed, notTested, needsEvidence);
    }

    private void RenderOverviewCards(
        TestTraceProject currentProject,
        int applicableTests,
        int passed,
        int failed,
        int notTested,
        int needsEvidence)
    {
        overviewCardsPanel.SuspendLayout();
        overviewCardsPanel.Controls.Clear();
        overviewCardsPanel.Controls.Add(CreateMetricCard("Lifecycle", FormatProjectState(currentProject.State), "Current governed state", StateBackColor(currentProject.State), StateForeColor(currentProject.State)));
        overviewCardsPanel.Controls.Add(CreateMetricCard("Applicable tests", applicableTests.ToString(), "In execution scope", AppTheme.Current.SurfaceElevated, AppTheme.Current.TextPrimary));
        overviewCardsPanel.Controls.Add(CreateMetricCard("Passed", passed.ToString(), "Latest result pass", AppTheme.Current.PassSoftBackground, AppTheme.Current.PassForeground));
        overviewCardsPanel.Controls.Add(CreateMetricCard("Failed", failed.ToString(), "Needs attention", AppTheme.Current.FailSoftBackground, AppTheme.Current.FailForeground));
        overviewCardsPanel.Controls.Add(CreateMetricCard("Not tested", notTested.ToString(), "Still open", AppTheme.Current.SurfaceElevated, AppTheme.Current.TextPrimary));
        overviewCardsPanel.Controls.Add(CreateMetricCard("Evidence gaps", needsEvidence.ToString(), "Required evidence missing", needsEvidence > 0 ? AppTheme.Current.StatusDraftBackground : AppTheme.Current.SurfaceElevated, needsEvidence > 0 ? AppTheme.Current.StatusDraftForeground : AppTheme.Current.TextPrimary));
        overviewCardsPanel.ResumeLayout();
    }

    private void RenderOverviewDetails(
        TestTraceProject currentProject,
        Section? selectedSection,
        Asset? selectedAsset,
        TestItem? selectedTest,
        int totalTests,
        int applicableTests,
        int notApplicableTests)
    {
        SetKeyValues(overviewDetailsPanel,
            ("Project", $"{currentProject.ContractRoot.ProjectCode} - {currentProject.ContractRoot.ProjectName}"),
            ("Machine", $"{currentProject.ContractRoot.MachineModel} / {currentProject.ContractRoot.MachineSerialNumber}"),
            ("Customer", currentProject.ContractRoot.CustomerName),
            ("Current focus", CurrentFocusText(selectedSection, selectedAsset, selectedTest)),
            ("Sections", currentProject.Sections.Count.ToString()),
            ("Assets", $"{currentProject.Assets.Count(asset => !asset.IsSubAsset)} top-level / {currentProject.Assets.Count(asset => asset.IsSubAsset)} sub-assets"),
            ("Tests", $"{totalTests} total / {applicableTests} applicable / {notApplicableTests} not applicable"),
            ("Authority", $"{currentProject.Users.Count(user => user.IsActive)} active users / {currentProject.AuthorityAssignments.Count(assignment => assignment.IsActive)} assignments"),
            ("Next governed move", NextGovernedMoveText(currentProject)));
    }

    private void RenderAssetMode(TestTraceProject currentProject)
    {
        assetModeList.BeginUpdate();
        assetModeList.Items.Clear();
        foreach (var asset in currentProject.Assets
                     .OrderBy(asset => asset.ParentAssetId is not null)
                     .ThenBy(asset => AssetHierarchyLabel(asset.AssetId)))
        {
            var sectionsUsingAsset = currentProject.Sections
                .Count(section => section.Assets.Any(sectionAsset => sectionAsset.AssetId == currentProject.TopLevelAssetId(asset.AssetId)));
            var tests = currentProject.Sections
                .SelectMany(section => section.TestItems)
                .Where(testItem => testItem.AssetId == asset.AssetId)
                .ToList();
            var metadata = new List<string>();
            if (!string.IsNullOrWhiteSpace(asset.Manufacturer)) metadata.Add(asset.Manufacturer);
            if (!string.IsNullOrWhiteSpace(asset.Model)) metadata.Add(asset.Model);
            if (!string.IsNullOrWhiteSpace(asset.SerialNumber)) metadata.Add("S/N " + asset.SerialNumber);

            var item = new ListViewItem(AssetHierarchyLabel(asset.AssetId));
            item.SubItems.Add(asset.Type);
            item.SubItems.Add(asset.ParentAssetId is null ? "-" : FindAsset(asset.ParentAssetId.Value)?.Name ?? "-");
            item.SubItems.Add(sectionsUsingAsset.ToString());
            item.SubItems.Add(tests.Count.ToString());
            item.SubItems.Add(tests.Sum(testItem => testItem.EvidenceRecords.Count).ToString());
            item.SubItems.Add(metadata.Count == 0 ? "-" : string.Join(" | ", metadata));
            assetModeList.Items.Add(item);
        }
        assetModeList.EndUpdate();
    }

    private void RenderTriageMode(TestTraceProject currentProject, IReadOnlyList<(Section Section, TestItem TestItem)> scopedTests)
    {
        triageModeList.BeginUpdate();
        triageModeList.Items.Clear();
        foreach (var itemPair in scopedTests
                     .OrderBy(item => TriageSortOrder(currentProject, item.Section, item.TestItem))
                     .ThenBy(item => item.Section.DisplayOrder)
                     .ThenBy(item => item.TestItem.DisplayOrder))
        {
            var section = itemPair.Section;
            var testItem = itemPair.TestItem;
            var state = TriageStateText(currentProject, section, testItem);
            var latest = LatestResultEntry(testItem);
            var latestText = latest is null
                ? "-"
                : $"{latest.Result} by {latest.ExecutedBy} at {latest.ExecutedAt.LocalDateTime:g}";
            var listItem = new ListViewItem(state);
            listItem.SubItems.Add($"{testItem.TestReference} - {testItem.TestTitle}");
            listItem.SubItems.Add(section.Title);
            listItem.SubItems.Add(AssetHierarchyLabel(testItem.AssetId));
            listItem.SubItems.Add(testItem.EvidenceRecords.Count.ToString());
            listItem.SubItems.Add(latestText);
            triageModeList.Items.Add(listItem);
        }
        triageModeList.EndUpdate();
    }

    private void RenderReportMode(
        TestTraceProject currentProject,
        int totalTests,
        int applicableTests,
        int passed,
        int failed,
        int notTested,
        int needsEvidence)
    {
        SetKeyValues(reportDetailsPanel,
            ("Report posture", currentProject.State == ProjectState.Released ? "Released record" : "Draft report preview"),
            ("Export format", "Markdown FAT report"),
            ("Contract root", "Frozen at project creation"),
            ("Machine context", currentProject.State == ProjectState.DraftContractBuilt ? "Editable until execution opens" : "Locked after execution opened"),
            ("Execution scope", $"{applicableTests} applicable tests / {totalTests} total"),
            ("Execution status", $"{passed} passed / {failed} failed / {notTested} not tested"),
            ("Evidence pressure", needsEvidence == 0 ? "No required evidence gaps visible in this summary." : $"{needsEvidence} applicable test(s) are missing required evidence types."),
            ("Export action", "Use Export FAT Report from the governed action bar."));
    }

    private IEnumerable<(Section Section, TestItem TestItem)> AllScopedTests(TestTraceProject currentProject)
    {
        return currentProject.Sections
            .OrderBy(section => section.DisplayOrder)
            .SelectMany(section => section.TestItems
                .OrderBy(testItem => testItem.DisplayOrder)
                .Select(testItem => (Section: section, TestItem: testItem)));
    }

    private static bool HasDeclaredEvidenceRequirement(TestItem testItem)
    {
        return testItem.EvidenceRequirements.PhotoRequired ||
               testItem.EvidenceRequirements.MeasurementRequired ||
               testItem.EvidenceRequirements.SignatureRequired ||
               testItem.EvidenceRequirements.FileUploadRequired;
    }

    private static int TriageSortOrder(TestTraceProject currentProject, Section section, TestItem testItem)
    {
        if (currentProject.EffectiveTestApplicability(section, testItem) == ApplicabilityState.NotApplicable)
        {
            return 5;
        }

        if (testItem.LatestResult == TestResult.Fail)
        {
            return 0;
        }

        if (testItem.LatestResult == TestResult.Pass && currentProject.MissingEvidenceTypes(testItem).Count > 0)
        {
            return 1;
        }

        if (testItem.LatestResult == TestResult.NotTested)
        {
            return 2;
        }

        if (testItem.LatestResult == TestResult.Pass)
        {
            return 4;
        }

        return 3;
    }

    private string TriageStateText(TestTraceProject currentProject, Section section, TestItem testItem)
    {
        if (currentProject.EffectiveTestApplicability(section, testItem) == ApplicabilityState.NotApplicable)
        {
            return "Not applicable";
        }

        if (testItem.LatestResult == TestResult.Fail)
        {
            return "Failed";
        }

        if (testItem.LatestResult == TestResult.Pass && currentProject.MissingEvidenceTypes(testItem).Count > 0)
        {
            return "Needs evidence";
        }

        return FormatLatestResult(testItem);
    }

    private string CurrentFocusText(Section? selectedSection, Asset? selectedAsset, TestItem? selectedTest)
    {
        if (selectedTest is not null)
        {
            return $"Test: {selectedTest.TestReference} - {selectedTest.TestTitle}";
        }

        if (selectedAsset is not null)
        {
            return $"Asset: {AssetHierarchyLabel(selectedAsset.AssetId)}";
        }

        if (selectedSection is not null)
        {
            return $"Section: {selectedSection.Title}";
        }

        return "Project root";
    }

    private static string NextGovernedMoveText(TestTraceProject currentProject)
    {
        return currentProject.State switch
        {
            ProjectState.DraftContractBuilt => "Define sections, assets, and tests, then open for FAT execution.",
            ProjectState.Executable => "Complete applicable tests, attach evidence, approve sections, then release.",
            ProjectState.Released => "Project is released and read-only.",
            _ => "Review readiness and continue the governed workflow."
        };
    }

    private void RenderSelectionDetails(Section? selectedSection, Asset? selectedAsset, TestItem? selectedTest)
    {
        if (project is null)
        {
            return;
        }

        if (selectedTest is not null)
        {
            SetKeyValues(selectionDetailsPanel,
                ("Type", "Test item"),
                ("Reference", selectedTest.TestReference),
                ("Title", selectedTest.TestTitle),
                ("Section", selectedSection?.Title ?? "-"),
                ("Asset", selectedAsset?.Name ?? "-"),
                ("Description", selectedTest.TestDescription ?? "-"),
                ("Expected outcome", selectedTest.ExpectedOutcome ?? "-"),
                ("Applicability", selectedSection is null ? selectedTest.Applicability.ToString() : EffectiveApplicabilityText(selectedSection, selectedTest)),
                ("Applicability reason", selectedSection is null ? selectedTest.ApplicabilityReason ?? "-" : project.EffectiveTestApplicabilityReason(selectedSection, selectedTest) ?? "-"),
                ("Acceptance criteria", selectedTest.AcceptanceCriteria.Describe()),
                ("Evidence requirements", selectedTest.EvidenceRequirements.Describe()),
                ("Behaviour", selectedTest.BehaviourRules.Describe()),
                ("Test inputs", DescribeInputs(selectedTest)),
                ("Latest result", FormatLatestResult(selectedTest)),
                ("Results", selectedTest.ResultHistory.Count.ToString()),
                ("Evidence records", selectedTest.EvidenceRecords.Count.ToString()),
                ("Created by", selectedTest.CreatedBy),
                ("Created at", selectedTest.CreatedAt.LocalDateTime.ToString("g")));
            return;
        }

        if (selectedAsset is not null)
        {
            var testCount = selectedSection?.TestItems.Count(testItem => testItem.AssetId == selectedAsset.AssetId) ?? 0;
            var sectionCount = project.Sections.Count(section =>
                section.Assets.Any(asset => asset.AssetId == project.TopLevelAssetId(selectedAsset.AssetId)));
            var sectionAsset = selectedSection is null ? null : SectionAssetForSelection(selectedSection, selectedAsset);

            SetKeyValues(selectionDetailsPanel,
                ("Type", selectedAsset.IsSubAsset ? "Sub-asset" : "Asset"),
                ("Name", selectedAsset.Name),
                ("Section", selectedSection?.Title ?? "-"),
                ("Applicability", sectionAsset?.Applicability.ToString() ?? "Applicable"),
                ("Applicability reason", sectionAsset?.ApplicabilityReason ?? "-"),
                ("Asset type", selectedAsset.Type),
                ("Manufacturer", selectedAsset.Manufacturer ?? "-"),
                ("Model", selectedAsset.Model ?? "-"),
                ("Serial", selectedAsset.SerialNumber ?? "-"),
                ("Notes", selectedAsset.Notes ?? "-"),
                ("Tests in this section", testCount.ToString()),
                ("Recognized in sections", sectionCount.ToString()),
                ("Created by", selectedAsset.CreatedBy),
                ("Created at", selectedAsset.CreatedAt.LocalDateTime.ToString("g")));
            return;
        }

        if (selectedSection is not null)
        {
            var approval = selectedSection.Approval is null
                ? "Not approved"
                : $"Approved by {selectedSection.Approval.ApprovedBy} at {selectedSection.Approval.ApprovedAt.LocalDateTime:g}";

            SetKeyValues(selectionDetailsPanel,
                ("Type", "Section"),
                ("Title", selectedSection.Title),
                ("Description", selectedSection.Description ?? "-"),
                ("Approver", selectedSection.SectionApprover ?? project.ContractRoot.ReleaseAuthority),
                ("Applicability", selectedSection.Applicability.ToString()),
                ("Applicability reason", selectedSection.ApplicabilityReason ?? "-"),
                ("Approval", approval),
                ("Assets", selectedSection.Assets.Count.ToString()),
                ("Test items", selectedSection.TestItems.Count.ToString()),
                ("Created by", selectedSection.CreatedBy),
                ("Created at", selectedSection.CreatedAt.LocalDateTime.ToString("g")));
            return;
        }

        SetKeyValues(selectionDetailsPanel,
            ("Selection", "No section or test item selected."),
            ("Next step", "Select a section or test item from the structure tree."));
    }

    private void RenderContextPanel(Section? selectedSection, Asset? selectedAsset, TestItem? selectedTest)
    {
        contextEvidenceList.Items.Clear();
        contextHistoryList.Items.Clear();

        if (project is null)
        {
            RenderContextPlaceholder("No project loaded.");
            return;
        }

        if (selectedTest is not null)
        {
            RenderTestContext(selectedSection, selectedAsset, selectedTest);
            RenderContextEvidence([selectedTest]);
            RenderContextHistory([selectedTest]);
            return;
        }

        if (selectedAsset is not null)
        {
            var relatedTests = RelatedTestsForAsset(selectedSection, selectedAsset);
            RenderAssetContext(selectedSection, selectedAsset, relatedTests);
            RenderContextEvidence(relatedTests);
            RenderContextHistory(relatedTests);
            return;
        }

        if (selectedSection is not null)
        {
            var sectionTests = selectedSection.TestItems.OrderBy(testItem => testItem.DisplayOrder).ToList();
            RenderSectionContext(selectedSection, sectionTests);
            RenderContextEvidence(sectionTests);
            RenderContextHistory(sectionTests);
            return;
        }

        RenderContextPlaceholder("Select a section, asset, or test item to see context.");
    }

    private void RenderTestContext(Section? selectedSection, Asset? selectedAsset, TestItem selectedTest)
    {
        var latest = LatestResultEntry(selectedTest);
        var applicability = selectedSection is null
            ? selectedTest.Applicability.ToString()
            : EffectiveApplicabilityText(selectedSection, selectedTest);
        var applicabilityReason = selectedSection is null
            ? selectedTest.ApplicabilityReason
            : project?.EffectiveTestApplicabilityReason(selectedSection, selectedTest);
        var latestStatus = applicability == ApplicabilityState.NotApplicable.ToString()
            ? $"Status: NOT APPLICABLE{Environment.NewLine}Reason: {applicabilityReason ?? "No reason recorded."}"
            : latest is null
                ? "Status: Not tested"
                : $"{ExecutionStateText(selectedTest, false)}{Environment.NewLine}By: {latest.ExecutedBy}{Environment.NewLine}At: {latest.ExecutedAt.LocalDateTime:g}";

        SetKeyValues(contextDetailsPanel,
            ("Test", $"{selectedTest.TestReference} - {selectedTest.TestTitle}"),
            ("Action", ActionText(selectedTest)),
            ("Expected", ExpectedText(selectedTest)),
            ("Pass Criteria", PassCriteriaText(selectedTest).Replace("Pass criteria: ", "", StringComparison.Ordinal)),
            ("Current Status", latestStatus),
            ("Applicability", applicability),
            ("Applicability Reason", applicabilityReason ?? "-"),
            ("Asset", selectedAsset is null ? "-" : AssetHierarchyLabel(selectedAsset.AssetId)),
            ("Inputs", DescribeInputs(selectedTest)),
            ("Latest Captured Values", DescribeCapturedInputValues(selectedTest, latest)),
            ("Evidence", DescribeEvidenceSummary(selectedTest)));
    }

    private void RenderAssetContext(Section? selectedSection, Asset selectedAsset, IReadOnlyList<TestItem> relatedTests)
    {
        var parent = selectedAsset.ParentAssetId is null
            ? null
            : FindAsset(selectedAsset.ParentAssetId.Value);
        var sectionAsset = selectedSection is null ? null : SectionAssetForSelection(selectedSection, selectedAsset);

        SetKeyValues(contextDetailsPanel,
            ("Selection", selectedAsset.IsSubAsset ? "Sub-asset" : "Asset"),
            ("Name", selectedAsset.Name),
            ("Parent Asset", parent?.Name ?? "-"),
            ("Path", AssetHierarchyLabel(selectedAsset.AssetId)),
            ("Section", selectedSection?.Title ?? "-"),
            ("Applicability", FormatApplicability(sectionAsset?.Applicability ?? ApplicabilityState.Applicable)),
            ("Applicability Reason", sectionAsset?.ApplicabilityReason ?? "-"),
            ("Type", selectedAsset.Type),
            ("Manufacturer", selectedAsset.Manufacturer ?? "-"),
            ("Model", selectedAsset.Model ?? "-"),
            ("Serial", selectedAsset.SerialNumber ?? "-"),
            ("Notes", selectedAsset.Notes ?? "-"),
            ("Related Tests", relatedTests.Count.ToString()),
            ("Latest Failures", relatedTests.Count(testItem => testItem.LatestResult == TestResult.Fail).ToString()),
            ("Evidence Items", relatedTests.Sum(testItem => testItem.EvidenceRecords.Count).ToString()));
    }

    private void RenderSectionContext(Section selectedSection, IReadOnlyList<TestItem> sectionTests)
    {
        var applicableTests = sectionTests
            .Where(testItem => project?.EffectiveTestApplicability(selectedSection, testItem) != ApplicabilityState.NotApplicable)
            .ToList();
        var approval = selectedSection.Approval is null
            ? "Not approved"
            : $"Approved by {selectedSection.Approval.ApprovedBy} at {selectedSection.Approval.ApprovedAt.LocalDateTime:g}";

        SetKeyValues(contextDetailsPanel,
            ("Selection", "Section"),
            ("Title", selectedSection.Title),
            ("Description", selectedSection.Description ?? "-"),
            ("Applicability", FormatApplicability(selectedSection.Applicability)),
            ("Applicability Reason", selectedSection.ApplicabilityReason ?? "-"),
            ("Approval", approval),
            ("Approver", selectedSection.SectionApprover ?? project?.ContractRoot.ReleaseAuthority ?? "-"),
            ("Assets", selectedSection.Assets.Count.ToString()),
            ("Tests Total", sectionTests.Count.ToString()),
            ("Tests in Scope", applicableTests.Count.ToString()),
            ("Passed", applicableTests.Count(testItem => testItem.LatestResult == TestResult.Pass).ToString()),
            ("Failed", applicableTests.Count(testItem => testItem.LatestResult == TestResult.Fail).ToString()),
            ("Not Tested", applicableTests.Count(testItem => testItem.LatestResult == TestResult.NotTested).ToString()),
            ("Evidence Items", sectionTests.Sum(testItem => testItem.EvidenceRecords.Count).ToString()));
    }

    private void RenderContextPlaceholder(string message)
    {
        SetKeyValues(contextDetailsPanel,
            ("Selection", "No selection"),
            ("Details", message));
    }

    private void RenderContextEvidence(IEnumerable<TestItem> tests)
    {
        contextEvidenceList.Items.Clear();
        foreach (var testItem in tests.OrderBy(testItem => testItem.DisplayOrder))
        {
            foreach (var evidence in testItem.EvidenceRecords.OrderByDescending(evidence => evidence.AttachedAt))
            {
                var item = new ListViewItem(evidence.StoredFileName);
                item.SubItems.Add(evidence.EvidenceType.ToString());
                item.SubItems.Add(evidence.AttachedBy);
                item.SubItems.Add(evidence.AttachedAt.LocalDateTime.ToString("g"));
                item.Tag = Path.Combine(projectFolderPath, "evidence", evidence.StoredFileName);
                contextEvidenceList.Items.Add(item);
            }
        }
    }

    private void RenderContextHistory(IEnumerable<TestItem> tests)
    {
        contextHistoryList.Items.Clear();
        foreach (var testItem in tests.OrderBy(testItem => testItem.DisplayOrder))
        {
            foreach (var result in testItem.ResultHistory.OrderByDescending(result => result.ExecutedAt))
            {
                var item = new ListViewItem(result.Result == TestResult.NotApplicable ? "Legacy N/A" : result.Result.ToString());
                item.SubItems.Add(result.ExecutedBy);
                item.SubItems.Add(result.ExecutedAt.LocalDateTime.ToString("g"));
                item.SubItems.Add(HistoryNotes(testItem, result));
                contextHistoryList.Items.Add(item);
            }
        }
    }

    private void OpenSelectedContextEvidence()
    {
        if (contextEvidenceList.SelectedItems.Count == 0 ||
            contextEvidenceList.SelectedItems[0].Tag is not string path ||
            !File.Exists(path))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private IReadOnlyList<TestItem> RelatedTestsForAsset(Section? selectedSection, Asset selectedAsset)
    {
        if (project is null)
        {
            return [];
        }

        var assetIds = new HashSet<Guid> { selectedAsset.AssetId };
        if (selectedAsset.ParentAssetId is null)
        {
            foreach (var child in project.ChildAssets(selectedAsset.AssetId))
            {
                assetIds.Add(child.AssetId);
            }
        }

        IEnumerable<Section> sections = selectedSection is null
            ? project.Sections
            : new[] { selectedSection };

        return sections
            .SelectMany(section => section.TestItems)
            .Where(testItem => testItem.AssetId is not null && assetIds.Contains(testItem.AssetId.Value))
            .OrderBy(testItem => testItem.DisplayOrder)
            .ToList();
    }

    private void RenderResults(TestItem? selectedTest)
    {
        resultList.Items.Clear();
        if (selectedTest is null)
        {
            return;
        }

        foreach (var result in selectedTest.ResultHistory.OrderByDescending(result => result.ExecutedAt))
        {
            var item = new ListViewItem(result.Result == TestResult.NotApplicable ? "Legacy result: NotApplicable" : result.Result.ToString());
            item.SubItems.Add(result.ExecutedBy);
            item.SubItems.Add(result.ExecutedAt.LocalDateTime.ToString("g"));
            item.SubItems.Add(result.MeasuredValue ?? string.Empty);
            item.SubItems.Add(result.Comments ?? string.Empty);
            resultList.Items.Add(item);
        }
    }

    private void RenderEvidence(TestItem? selectedTest)
    {
        evidenceList.Items.Clear();
        if (selectedTest is null)
        {
            return;
        }

        foreach (var evidence in selectedTest.EvidenceRecords.OrderByDescending(evidence => evidence.AttachedAt))
        {
            var item = new ListViewItem(evidence.StoredFileName);
            item.SubItems.Add(evidence.OriginalFileName);
            item.SubItems.Add(evidence.EvidenceType.ToString());
            item.SubItems.Add(evidence.AttachedBy);
            item.SubItems.Add(evidence.Sha256Hash);
            evidenceList.Items.Add(item);
        }
    }

    private void RenderAudit(TestTraceProject currentProject)
    {
        auditList.Items.Clear();
        foreach (var entry in currentProject.AuditLog.OrderByDescending(entry => entry.At))
        {
            var item = new ListViewItem(entry.At.LocalDateTime.ToString("g"));
            item.SubItems.Add(entry.Actor);
            item.SubItems.Add(entry.Action);
            item.SubItems.Add(entry.TargetType ?? string.Empty);
            item.SubItems.Add(entry.Details ?? string.Empty);
            auditList.Items.Add(item);
        }
    }

    private void RenderReadiness(ReadinessReport report)
    {
        var lines = new List<string>();
        if (report.Issues.Count == 0)
        {
            lines.Add("Ready. No workflow blockers for the current selection.");
        }
        else
        {
            foreach (var group in report.Issues.GroupBy(issue => issue.Area).OrderBy(group => group.Key))
            {
                lines.Add(group.Key + ":");
                lines.AddRange(group.Select(issue => "- " + issue.Message));
                lines.Add("");
            }
        }

        readinessTextBox.Text = string.Join(Environment.NewLine, lines).TrimEnd();
    }

    private void AddSection()
    {
        if (project is null)
        {
            return;
        }

        using var form = new AddSectionForm(project.ContractRoot.ReleaseAuthority);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = workspaceService.AddSection(new AddSectionRequest
        {
            ProjectFolderPath = projectFolderPath,
            Title = form.SectionTitle,
            Description = form.Description,
            SectionApprover = form.SectionApprover,
            Actor = CurrentActor()
        });

        HandleOperationResult(result, "Section added.");
    }

    private void AddAsset()
    {
        if (project is null)
        {
            return;
        }

        var selectedSection = SelectedSection();
        if (selectedSection is null)
        {
            using var globalForm = new AddComponentForm("Global project asset", [], "Add Asset");
            if (globalForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var globalResult = workspaceService.AddAsset(new AddAssetRequest
            {
                ProjectFolderPath = projectFolderPath,
                AssetName = globalForm.AssetName,
                AssetType = globalForm.AssetType,
                Manufacturer = globalForm.Manufacturer,
                Model = globalForm.Model,
                SerialNumber = globalForm.SerialNumber,
                Notes = globalForm.Notes,
                Actor = CurrentActor()
            });

            HandleOperationResult(globalResult, "Global asset added. Assign it from a section when needed.");
            return;
        }

        var availableAssets = project.Assets
            .Where(asset => asset.ParentAssetId is null)
            .Where(asset => selectedSection.Assets.All(sectionAsset => sectionAsset.AssetId != asset.AssetId))
            .OrderBy(asset => asset.Name)
            .ToList();

        using var form = new AddComponentForm($"Section: {selectedSection.Title}", availableAssets, "Add Asset");
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = workspaceService.AddAssetToSection(new AddAssetToSectionRequest
        {
            ProjectFolderPath = projectFolderPath,
            SectionId = selectedSection.SectionId,
            AssetName = form.AssetName,
            AssetType = form.AssetType,
            Manufacturer = form.Manufacturer,
            Model = form.Model,
            SerialNumber = form.SerialNumber,
            Notes = form.Notes,
            Actor = CurrentActor()
        });

        HandleOperationResult(result, "Asset added to section.");
    }

    private void AddSubAsset()
    {
        if (project is null)
        {
            return;
        }

        var parentAsset = SelectedAsset();
        if (parentAsset is null || parentAsset.ParentAssetId is not null)
        {
            MessageBox.Show(this, "Select a top-level asset first.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new AddComponentForm($"Parent asset: {parentAsset.Name}", [], "Add Sub-Asset");
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = workspaceService.AddSubAsset(new AddSubAssetRequest
        {
            ProjectFolderPath = projectFolderPath,
            ParentAssetId = parentAsset.AssetId,
            AssetName = form.AssetName,
            AssetType = form.AssetType,
            Manufacturer = form.Manufacturer,
            Model = form.Model,
            SerialNumber = form.SerialNumber,
            Notes = form.Notes,
            Actor = CurrentActor()
        });

        HandleOperationResult(result, "Sub-asset added.");
    }

    private void AddTestItem()
    {
        if (project is null)
        {
            return;
        }

        var sectionId = SelectedSectionId();
        var asset = SelectedAsset();
        if (sectionId is null || asset is null)
        {
            MessageBox.Show(this, "Select an asset or sub-asset first.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new AddTestItemForm(asset.Name, NextTestReference(sectionId.Value, asset.AssetId));
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = workspaceService.AddTestItem(new AddTestItemRequest
        {
            ProjectFolderPath = projectFolderPath,
            SectionId = sectionId.Value,
            AssetId = asset.AssetId,
            TestReference = form.TestReference,
            TestTitle = form.TestTitle,
            TestDescription = form.TestDescription,
            ExpectedOutcome = form.ExpectedOutcome,
            AcceptanceCriteria = form.AcceptanceCriteria,
            EvidenceRequirements = form.EvidenceRequirements,
            BehaviourRules = form.BehaviourRules,
            TestInputs = form.TestInputs,
            Actor = CurrentActor()
        });

        HandleOperationResult(result, "Test item added.");
    }

    private void RenameSelected()
    {
        if (project is null || !CanRenameSelectedNow())
        {
            ShowBlocked(StructureBlockedReason());
            return;
        }

        OperationResult? result = workspaceTree.SelectedNode?.Tag switch
        {
            SectionNodeTag sectionTag => RenameSection(sectionTag),
            AssetNodeTag assetTag => RenameAsset(assetTag),
            TestNodeTag testTag => RenameTestItem(testTag),
            _ => null
        };

        if (result is not null)
        {
            HandleOperationResult(result, "Structure renamed.");
        }
    }

    private OperationResult? RenameSection(SectionNodeTag tag)
    {
        var section = project?.Sections.SingleOrDefault(section => section.SectionId == tag.SectionId);
        if (section is null)
        {
            ShowBlocked("Section was not found.");
            return null;
        }

        using var form = new RenameStructureItemForm(
            "Rename Section",
            "Section title",
            section.Title);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        return workspaceService.RenameSection(new RenameSectionRequest
        {
            ProjectFolderPath = projectFolderPath,
            SectionId = tag.SectionId,
            Title = form.PrimaryValue,
            Actor = CurrentActor()
        });
    }

    private OperationResult? RenameAsset(AssetNodeTag tag)
    {
        var asset = FindAsset(tag.AssetId);
        if (asset is null)
        {
            ShowBlocked("Asset was not found.");
            return null;
        }

        using var form = new RenameStructureItemForm(
            tag.IsSubAsset ? "Rename Sub-Asset" : "Rename Asset",
            "Asset name",
            asset.Name,
            note: "Renaming this asset updates its name everywhere it is used in this project.");
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        return workspaceService.RenameAsset(new RenameAssetRequest
        {
            ProjectFolderPath = projectFolderPath,
            AssetId = tag.AssetId,
            AssetName = form.PrimaryValue,
            Actor = CurrentActor()
        });
    }

    private void EditSelectedAssetMetadata()
    {
        if (workspaceTree.SelectedNode?.Tag is not AssetNodeTag tag)
        {
            return;
        }

        var result = EditAssetMetadata(tag);
        if (result is not null)
        {
            HandleOperationResult(result, "Asset metadata updated.");
        }
    }

    private OperationResult? EditAssetMetadata(AssetNodeTag tag)
    {
        var asset = FindAsset(tag.AssetId);
        if (asset is null)
        {
            ShowBlocked("Asset was not found.");
            return null;
        }

        using var form = new AssetMetadataForm(asset);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        return workspaceService.UpdateAssetMetadata(new UpdateAssetMetadataRequest
        {
            ProjectFolderPath = projectFolderPath,
            AssetId = tag.AssetId,
            AssetType = form.AssetType,
            Manufacturer = form.Manufacturer,
            Model = form.Model,
            SerialNumber = form.SerialNumber,
            Notes = form.Notes,
            Actor = CurrentActor()
        });
    }

    private OperationResult? RenameTestItem(TestNodeTag tag)
    {
        var testItem = SelectedTestItem();
        if (testItem is null)
        {
            ShowBlocked("Test item was not found.");
            return null;
        }

        using var form = new RenameStructureItemForm(
            "Rename Test Item",
            "Reference",
            testItem.TestReference,
            "Title",
            testItem.TestTitle);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        return workspaceService.RenameTestItem(new RenameTestItemRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = tag.TestItemId,
            TestReference = form.PrimaryValue,
            TestTitle = form.SecondaryValue,
            Actor = CurrentActor()
        });
    }

    private void DeleteSelected()
    {
        if (project is null || !CanDeleteSelectedNow())
        {
            ShowBlocked(StructureBlockedReason());
            return;
        }

        OperationResult? result = workspaceTree.SelectedNode?.Tag switch
        {
            SectionNodeTag sectionTag => DeleteSection(sectionTag),
            AssetNodeTag assetTag => assetTag.IsSubAsset ? DeleteAsset(assetTag) : RemoveAssetFromSection(assetTag),
            TestNodeTag testTag => DeleteTestItem(testTag),
            _ => null
        };

        if (result is not null)
        {
            HandleOperationResult(result, "Structure updated.");
        }
    }

    private OperationResult? DeleteSection(SectionNodeTag tag)
    {
        var section = project?.Sections.SingleOrDefault(section => section.SectionId == tag.SectionId);
        if (section is null)
        {
            ShowBlocked("Section was not found.");
            return null;
        }

        if (!ConfirmDestructive(
                "Delete section?",
                $"Delete section '{section.Title}' and all test items inside it?"))
        {
            return null;
        }

        return workspaceService.DeleteSection(new DeleteSectionRequest
        {
            ProjectFolderPath = projectFolderPath,
            SectionId = tag.SectionId,
            Actor = CurrentActor()
        });
    }

    private OperationResult? RemoveAssetFromSection(AssetNodeTag tag)
    {
        var section = project?.Sections.SingleOrDefault(section => section.SectionId == tag.SectionId);
        var asset = FindAsset(tag.AssetId);
        if (section is null || asset is null)
        {
            ShowBlocked("Asset assignment was not found.");
            return null;
        }

        if (!ConfirmDestructive(
                "Remove asset assignment?",
                $"Remove '{asset.Name}' from section '{section.Title}'? The asset remains available for other sections."))
        {
            return null;
        }

        return workspaceService.RemoveAssetFromSection(new RemoveAssetFromSectionRequest
        {
            ProjectFolderPath = projectFolderPath,
            SectionId = tag.SectionId,
            AssetId = tag.AssetId,
            Actor = CurrentActor()
        });
    }

    private void DeleteAssetGlobally()
    {
        if (workspaceTree.SelectedNode?.Tag is not AssetNodeTag tag)
        {
            return;
        }

        var result = DeleteAsset(tag);
        if (result is not null)
        {
            HandleOperationResult(result, "Asset deleted.");
        }
    }

    private OperationResult? DeleteAsset(AssetNodeTag tag)
    {
        var asset = FindAsset(tag.AssetId);
        if (asset is null)
        {
            ShowBlocked("Asset was not found.");
            return null;
        }

        var title = tag.IsSubAsset ? "Delete sub-asset?" : "Delete asset globally?";
        var message = tag.IsSubAsset
            ? $"Delete sub-asset '{asset.Name}' and its test items?"
            : $"Delete asset '{asset.Name}', its sub-assets, assignments, and draft test items across the project?";
        if (!ConfirmDestructive(title, message))
        {
            return null;
        }

        return workspaceService.DeleteAsset(new DeleteAssetRequest
        {
            ProjectFolderPath = projectFolderPath,
            AssetId = tag.AssetId,
            Actor = CurrentActor()
        });
    }

    private OperationResult? DeleteTestItem(TestNodeTag tag)
    {
        var testItem = SelectedTestItem();
        if (testItem is null)
        {
            ShowBlocked("Test item was not found.");
            return null;
        }

        if (!ConfirmDestructive(
                "Delete test item?",
                $"Delete test item '{testItem.TestReference} - {testItem.TestTitle}'?"))
        {
            return null;
        }

        return workspaceService.DeleteTestItem(new DeleteTestItemRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = tag.TestItemId,
            Actor = CurrentActor()
        });
    }

    private void MarkSelectedNotApplicable()
    {
        SetSelectedApplicability(ApplicabilityState.NotApplicable);
    }

    private void MarkSelectedApplicable()
    {
        SetSelectedApplicability(ApplicabilityState.Applicable);
    }

    private void SetSelectedApplicability(ApplicabilityState targetState)
    {
        if (project is null)
        {
            return;
        }

        if (!CanChangeApplicabilityNow())
        {
            ShowBlocked(ApplicabilityBlockedReason());
            return;
        }

        using var form = new SetApplicabilityForm(SelectedApplicabilityTargetLabel(), targetState);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        OperationResult? result = workspaceTree.SelectedNode?.Tag switch
        {
            SectionNodeTag sectionTag => SetSectionApplicability(sectionTag, targetState, form.Reason),
            AssetNodeTag assetTag => SetAssetApplicability(assetTag, targetState, form.Reason),
            TestNodeTag testTag => SetTestApplicability(testTag, targetState, form.Reason),
            _ => null
        };

        if (result is not null)
        {
            HandleOperationResult(result, targetState == ApplicabilityState.NotApplicable
                ? "Marked not applicable."
                : "Marked applicable.");
        }
    }

    private OperationResult? SetSectionApplicability(SectionNodeTag tag, ApplicabilityState targetState, string? reason)
    {
        return workspaceService.SetSectionApplicability(new SetSectionApplicabilityRequest
        {
            ProjectFolderPath = projectFolderPath,
            SectionId = tag.SectionId,
            Applicability = targetState,
            Reason = reason,
            Actor = CurrentActor()
        });
    }

    private OperationResult? SetAssetApplicability(AssetNodeTag tag, ApplicabilityState targetState, string? reason)
    {
        return workspaceService.SetAssetApplicability(new SetAssetApplicabilityRequest
        {
            ProjectFolderPath = projectFolderPath,
            SectionId = tag.SectionId,
            AssetId = tag.AssetId,
            Applicability = targetState,
            Reason = reason,
            Actor = CurrentActor()
        });
    }

    private OperationResult? SetTestApplicability(TestNodeTag tag, ApplicabilityState targetState, string? reason)
    {
        return workspaceService.SetTestApplicability(new SetTestApplicabilityRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = tag.TestItemId,
            Applicability = targetState,
            Reason = reason,
            Actor = CurrentActor()
        });
    }

    private void OpenForExecution()
    {
        var result = workspaceService.OpenForExecution(new OpenForExecutionRequest
        {
            ProjectFolderPath = projectFolderPath,
            Actor = CurrentActor()
        });

        HandleOperationResult(result, "Project opened for execution.");
    }

    private void RecordResult()
    {
        var selected = SelectedTestItem();
        if (selected is null)
        {
            MessageBox.Show(this, "Select a test item first.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (selected.Inputs.Count > 0)
        {
            MessageBox.Show(
                this,
                "This test has structured inputs. Record it from the Execution Sheet so the input values are captured with the result.",
                "TestTrace",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var form = new RecordResultForm(selected.TestReference, selected.TestTitle);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var supersedes = selected.ResultHistory.LastOrDefault()?.ResultEntryId;
        if (!TryCollectExecutionGovernance(selected, form.SelectedResult, out var witnessedBy, out var overrideReason))
        {
            return;
        }

        var result = executionService.RecordResult(new RecordResultRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = selected.TestItemId,
            Result = form.SelectedResult,
            MeasuredValue = form.MeasuredValue,
            Comments = form.Comments,
            SupersedesResultEntryId = supersedes,
            WitnessedBy = witnessedBy,
            OverrideReason = overrideReason,
            ExecutedBy = CurrentActor()
        });

        HandleOperationResult(result, "Result recorded.");
    }

    private void AttachEvidence()
    {
        var selected = SelectedTestItem();
        if (selected is null)
        {
            MessageBox.Show(this, "Select a test item first.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new AttachEvidenceForm(selected.TestReference, selected.TestTitle, selected.EvidenceRequirements);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = executionService.AttachEvidence(new AttachEvidenceRequest
        {
            ProjectFolderPath = projectFolderPath,
            TestItemId = selected.TestItemId,
            SourceFilePath = form.SourceFilePath,
            EvidenceType = form.EvidenceType,
            Description = form.Description,
            AttachedBy = CurrentActor()
        });

        HandleOperationResult(result, "Evidence attached.");
    }

    private void ApproveSelectedSection()
    {
        if (project is null)
        {
            return;
        }

        var selectedSection = SelectedSection();
        if (selectedSection is null)
        {
            MessageBox.Show(this, "Select a section first.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new ApproveSectionForm(
            selectedSection.Title,
            selectedSection.SectionApprover ?? project.ContractRoot.ReleaseAuthority);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = approvalService.ApproveSection(new ApproveSectionRequest
        {
            ProjectFolderPath = projectFolderPath,
            SectionId = selectedSection.SectionId,
            ApprovedBy = form.ApprovedBy,
            Comments = form.Comments
        });

        HandleOperationResult(result, "Section approved.");
    }

    private void ReleaseProject()
    {
        if (project is null)
        {
            return;
        }

        using var form = new ReleaseProjectForm(
            $"{project.ContractRoot.ProjectCode} - {project.ContractRoot.ProjectName}",
            project.ContractRoot.ReleaseAuthority);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = approvalService.ReleaseProject(new ReleaseProjectRequest
        {
            ProjectFolderPath = projectFolderPath,
            ReleasedBy = form.ReleasedBy,
            Declaration = form.Declaration
        });

        HandleOperationResult(result, "Project released.");
    }

    private void ExportReport()
    {
        var result = exportService.ExportFatReport(projectFolderPath);
        if (!result.Succeeded || string.IsNullOrWhiteSpace(result.FilePath))
        {
            MessageBox.Show(this, result.ErrorMessage ?? "Export failed.", "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        statusLabel.Text = $"Export created: {result.FilePath}";
        MessageBox.Show(this, result.FilePath, "FAT report exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void EditProjectMetadata()
    {
        if (project is null)
        {
            return;
        }

        if (!CanEditProjectMetadataNow())
        {
            MessageBox.Show(this, ProjectMetadataBlockedReason(), "Metadata locked", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new EditProjectMetadataForm(
            $"{project.ContractRoot.ProjectCode} - {project.ContractRoot.ProjectName}",
            project.ReportingMetadata.Clone(),
            project.State);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = workspaceService.UpdateProjectMetadata(new UpdateProjectMetadataRequest
        {
            ProjectFolderPath = projectFolderPath,
            Metadata = form.Metadata,
            Actor = CurrentActor()
        });

        HandleOperationResult(result, "Project metadata updated.");
    }

    private void HandleOperationResult(OperationResult result, string successMessage)
    {
        if (!result.Succeeded)
        {
            MessageBox.Show(this, BuildErrorText(result), "Operation blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        statusLabel.Text = successMessage;
        LoadProject();
    }

    private void UpdateButtonState()
    {
        if (project is null)
        {
            return;
        }

        var selectedSection = SelectedSection();
        var selectedAsset = SelectedAsset();
        var selectedTest = SelectedTestItem();
        readiness = ProjectReadinessService.Evaluate(project, selectedSection, selectedAsset, selectedTest);

        addSectionButton.Enabled = readiness.CanAddSection;
        addComponentButton.Enabled = CanAddAssetNow();
        addSubAssetButton.Enabled = readiness.CanAddSubAsset;
        addTestItemButton.Enabled = readiness.CanAddTestItem;
        renameButton.Enabled = CanRenameSelectedNow();
        deleteButton.Enabled = CanDeleteSelectedNow();
        openForExecutionButton.Enabled = readiness.CanOpenForExecution;
        recordResultButton.Enabled = readiness.CanRecordResults;
        attachEvidenceButton.Enabled = readiness.CanAttachEvidence;
        sheetMarkNotApplicableButton.Enabled = CanChangeApplicabilityNow();
        sheetMarkApplicableButton.Enabled = CanChangeApplicabilityNow();
        approveSectionButton.Enabled = readiness.CanApproveSection;
        releaseProjectButton.Enabled = readiness.CanRelease;
        exportReportButton.Enabled = true;
        editProjectMetadataButton.Enabled = CanEditProjectMetadataNow();
        UpdateStructureToolTips();
        RenderReadiness(readiness);
    }

    private void UpdateStructureToolTips()
    {
        actionToolTip.SetToolTip(addSectionButton, CanAddSectionNow() ? "Add a new top-level section." : StructureBlockedReason());
        actionToolTip.SetToolTip(addComponentButton, CanAddAssetNow() ? "Add or reuse a global asset in the selected section." : AddAssetBlockedReason());
        actionToolTip.SetToolTip(addSubAssetButton, CanAddSubAssetNow() ? "Add a sub-asset under the selected asset." : AddSubAssetBlockedReason());
        actionToolTip.SetToolTip(addTestItemButton, CanAddTestItemNow() ? "Add a test item under the selected asset." : AddTestItemBlockedReason());
        actionToolTip.SetToolTip(renameButton, CanRenameSelectedNow() ? "Rename the selected section, asset, sub-asset, or test item." : RenameBlockedReason());
        actionToolTip.SetToolTip(deleteButton, CanDeleteSelectedNow() ? "Delete or remove the selected structure item." : DeleteBlockedReason());
        actionToolTip.SetToolTip(sheetMarkNotApplicableButton, CanChangeApplicabilityNow() ? "Mark the selected scope item not applicable with a reason." : ApplicabilityBlockedReason());
        actionToolTip.SetToolTip(sheetMarkApplicableButton, CanChangeApplicabilityNow() ? "Return the selected scope item to applicable execution scope." : ApplicabilityBlockedReason());
        actionToolTip.SetToolTip(editProjectMetadataButton, CanEditProjectMetadataNow()
            ? "Edit project metadata. Machine context locks when execution opens; reporting and contact details remain editable until release."
            : ProjectMetadataBlockedReason());
    }

    private bool CanAddSectionNow()
    {
        return project?.State == ProjectState.DraftContractBuilt;
    }

    private bool CanAddAssetNow()
    {
        return project?.State == ProjectState.DraftContractBuilt &&
            workspaceTree.SelectedNode?.Tag is ProjectNodeTag or SectionNodeTag or AssetNodeTag or TestNodeTag;
    }

    private bool CanAddSubAssetNow()
    {
        var selectedAsset = SelectedAsset();
        return project?.State == ProjectState.DraftContractBuilt &&
            selectedAsset is not null &&
            selectedAsset.ParentAssetId is null;
    }

    private bool CanAddTestItemNow()
    {
        return project?.State == ProjectState.DraftContractBuilt && SelectedAsset() is not null;
    }

    private bool CanRenameSelectedNow()
    {
        return project?.State == ProjectState.DraftContractBuilt &&
            workspaceTree.SelectedNode?.Tag is SectionNodeTag or AssetNodeTag or TestNodeTag;
    }

    private bool CanDeleteSelectedNow()
    {
        return project?.State == ProjectState.DraftContractBuilt &&
            workspaceTree.SelectedNode?.Tag is SectionNodeTag or AssetNodeTag or TestNodeTag;
    }

    private bool CanChangeApplicabilityNow()
    {
        var selectedSection = SelectedSection();
        return project?.State is ProjectState.DraftContractBuilt or ProjectState.Executable &&
            selectedSection?.Approval is null &&
            workspaceTree.SelectedNode?.Tag is SectionNodeTag or AssetNodeTag or TestNodeTag;
    }

    private bool CanEditProjectMetadataNow()
    {
        return project is not null && project.State != ProjectState.Released;
    }

    private string StructureBlockedReason()
    {
        if (project is null)
        {
            return "Project is not loaded.";
        }

        return project.State switch
        {
            ProjectState.DraftContractBuilt => "Select a structure item.",
            ProjectState.Executable => "Structure is frozen once execution has started.",
            ProjectState.Released => "Released projects are read-only.",
            _ => "Structure action is not available."
        };
    }

    private string AddAssetBlockedReason()
    {
        if (project?.State != ProjectState.DraftContractBuilt)
        {
            return StructureBlockedReason();
        }

        return CanAddAssetNow()
            ? "Create a global asset or assign one to the selected section."
            : "Select the project or a section first.";
    }

    private string AddSubAssetBlockedReason()
    {
        if (project?.State != ProjectState.DraftContractBuilt)
        {
            return StructureBlockedReason();
        }

        var selectedAsset = SelectedAsset();
        if (selectedAsset is null)
        {
            return "Select a top-level asset first.";
        }

        return selectedAsset.ParentAssetId is not null
            ? "This MVP supports one sub-asset level only."
            : "Cannot add a sub-asset to the current selection.";
    }

    private string AddTestItemBlockedReason()
    {
        if (project?.State != ProjectState.DraftContractBuilt)
        {
            return StructureBlockedReason();
        }

        return SelectedAsset() is null
            ? "Select an asset or sub-asset first."
            : "Cannot add a test item to the current selection.";
    }

    private string RenameBlockedReason()
    {
        if (project?.State != ProjectState.DraftContractBuilt)
        {
            return StructureBlockedReason();
        }

        return "Select a section, asset, sub-asset, or test item.";
    }

    private string ApplicabilityBlockedReason()
    {
        if (project is null)
        {
            return "Project is not loaded.";
        }

        if (project.State == ProjectState.Released)
        {
            return "Released projects are read-only.";
        }

        if (project.State is not ProjectState.DraftContractBuilt and not ProjectState.Executable)
        {
            return "Applicability can only be changed before project release.";
        }

        if (SelectedSection()?.Approval is not null)
        {
            return "Approved sections cannot have applicability changed.";
        }

        return workspaceTree.SelectedNode?.Tag is SectionNodeTag or AssetNodeTag or TestNodeTag
            ? "Applicability is governed by domain rules for this selection."
            : "Select a section, asset, sub-asset, or test item.";
    }

    private string ProjectMetadataBlockedReason()
    {
        if (project is null)
        {
            return "Project is not loaded.";
        }

        return project.State == ProjectState.Released
            ? "Released projects are read-only."
            : project.State == ProjectState.Executable
                ? "Machine context is locked because execution has started. Reporting and contact details can still be edited until release."
                : "Project metadata can be edited before execution, with reporting and contact details remaining editable until release.";
    }

    private string DeleteBlockedReason()
    {
        if (project?.State != ProjectState.DraftContractBuilt)
        {
            return StructureBlockedReason();
        }

        return "Select a section, asset, sub-asset, or test item.";
    }

    private Guid? SelectedSectionId()
    {
        return workspaceTree.SelectedNode?.Tag switch
        {
            SectionNodeTag section => section.SectionId,
            AssetNodeTag asset => asset.SectionId,
            TestNodeTag test => test.SectionId,
            _ => null
        };
    }

    private Section? SelectedSection()
    {
        if (project is null)
        {
            return null;
        }

        var sectionId = SelectedSectionId();
        return sectionId is null
            ? null
            : project.Sections.SingleOrDefault(section => section.SectionId == sectionId.Value);
    }

    private Asset? SelectedAsset()
    {
        if (project is null)
        {
            return null;
        }

        var assetId = workspaceTree.SelectedNode?.Tag switch
        {
            AssetNodeTag asset => asset.AssetId,
            TestNodeTag test => test.AssetId,
            _ => null
        };

        return assetId is null ? null : FindAsset(assetId.Value);
    }

    private TestItem? SelectedTestItem()
    {
        if (project is null || workspaceTree.SelectedNode?.Tag is not TestNodeTag test)
        {
            return null;
        }

        return project.Sections
            .SelectMany(section => section.TestItems)
            .SingleOrDefault(testItem => testItem.TestItemId == test.TestItemId);
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
        return parent is null
            ? asset.Name
            : $"{parent.Name} -> {asset.Name}";
    }

    private string WorkItemText(Section section, TestItem testItem)
    {
        var primary = string.IsNullOrWhiteSpace(testItem.TestDescription)
            ? testItem.TestTitle
            : testItem.TestDescription;
        var titleSuffix = string.Equals(primary, testItem.TestTitle, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : $" | {testItem.TestTitle}";

        return $"{testItem.TestReference}   {primary}{titleSuffix}" +
            Environment.NewLine +
            $"{section.Title} > {AssetHierarchyLabel(testItem.AssetId)}";
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

    private static string EvidenceActionText(TestItem testItem)
    {
        return $"+ Evidence ({testItem.EvidenceRecords.Count})";
    }

    private static string EvidenceCountText(TestItem testItem)
    {
        return $"Evidence {testItem.EvidenceRecords.Count}";
    }

    private static string EvidenceSummaryText(TestItem testItem)
    {
        return testItem.EvidenceRecords.Count == 0
            ? "Evidence 0"
            : $"Evidence {testItem.EvidenceRecords.Count} attached";
    }

    private static ResultEntry? LatestResultEntry(TestItem testItem)
    {
        return testItem.ResultHistory
            .OrderByDescending(result => result.ExecutedAt)
            .FirstOrDefault();
    }

    private static string ExecutionStateText(TestItem testItem, bool notApplicable)
    {
        if (notApplicable)
        {
            return "Status: NOT APPLICABLE";
        }

        if (testItem.ResultHistory.Count == 0)
        {
            return "Status: Not tested";
        }

        var prefix = testItem.ResultHistory.Count > 1 ? "Status: Retested -> " : "Status: ";
        return testItem.LatestResult switch
        {
            TestResult.Pass => prefix + "Passed",
            TestResult.Fail => prefix + "Failed",
            TestResult.NotApplicable => "Legacy not-applicable result",
            _ => "Not tested"
        };
    }

    private static Color ExecutionStateForeColor(TestItem testItem, bool notApplicable)
    {
        if (notApplicable)
        {
            return AppTheme.Current.NotApplicableForeground;
        }

        return testItem.LatestResult switch
        {
            TestResult.Pass => AppTheme.Current.PassForeground,
            TestResult.Fail => AppTheme.Current.FailForeground,
            _ => AppTheme.Current.TextSecondary
        };
    }

    private SectionAsset? SectionAssetForSelection(Section section, Asset asset)
    {
        if (project is null)
        {
            return null;
        }

        var topLevelAssetId = project.TopLevelAssetId(asset.AssetId);
        return section.Assets.SingleOrDefault(sectionAsset => sectionAsset.AssetId == topLevelAssetId);
    }

    private string SelectedApplicabilityTargetLabel()
    {
        return workspaceTree.SelectedNode?.Tag switch
        {
            SectionNodeTag => "Section: " + (SelectedSection()?.Title ?? "Unknown section"),
            AssetNodeTag => "Asset: " + (SelectedAsset()?.Name ?? "Unknown asset"),
            TestNodeTag => "Test item: " + (SelectedTestItem() is { } test ? $"{test.TestReference} - {test.TestTitle}" : "Unknown test"),
            _ => "Selected item"
        };
    }

    private string EffectiveApplicabilityText(Section section, TestItem testItem)
    {
        return project?.EffectiveTestApplicability(section, testItem) == ApplicabilityState.NotApplicable
            ? "Not applicable"
            : "Applicable";
    }

    private static string FormatLatestResult(TestItem testItem)
    {
        return testItem.LatestResult == TestResult.NotApplicable
            ? "Legacy result: Not applicable"
            : FormatTestResult(testItem.LatestResult);
    }

    private static string FormatProjectState(ProjectState state)
    {
        return state switch
        {
            ProjectState.DraftContractBuilt => "Draft Contract",
            ProjectState.Executable => "Ready for Execution",
            ProjectState.Released => "Released",
            _ => state.ToString()
        };
    }

    private static string FormatApplicability(ApplicabilityState applicability)
    {
        return applicability == ApplicabilityState.NotApplicable ? "Not applicable" : "Applicable";
    }

    private static string FormatTestResult(TestResult result)
    {
        return result switch
        {
            TestResult.NotTested => "Not tested",
            TestResult.Pass => "Pass",
            TestResult.Fail => "Fail",
            TestResult.NotApplicable => "Not applicable",
            _ => result.ToString()
        };
    }

    private static string LatestComment(TestItem testItem)
    {
        return testItem.ResultHistory
            .OrderByDescending(result => result.ExecutedAt)
            .FirstOrDefault()
            ?.Comments ?? string.Empty;
    }

    private static string DescribeInputs(TestItem testItem)
    {
        return testItem.Inputs.Count == 0
            ? "No structured inputs defined."
            : string.Join(Environment.NewLine, testItem.Inputs
                .OrderBy(input => input.DisplayOrder)
                .Select(input => "- " + input.DescribeDefinition()));
    }

    private static string DescribeCapturedInputValues(TestItem testItem, ResultEntry? result)
    {
        if (result is null || result.CapturedInputValues.Count == 0)
        {
            return "No captured input values.";
        }

        var definitions = testItem.Inputs.ToDictionary(input => input.TestInputId);
        return string.Join(Environment.NewLine, result.CapturedInputValues.Select(value =>
        {
            if (!definitions.TryGetValue(value.TestInputId, out var input))
            {
                return "- Unknown input: " + value.Value;
            }

            var unit = !string.IsNullOrWhiteSpace(input.Unit) ? $" {input.Unit.Trim()}" : string.Empty;
            return $"- {input.Label}: {FormatCapturedValue(input, value.Value)}{unit}";
        }));
    }

    private static string FormatCapturedValue(TestInput input, string value)
    {
        if (input.InputType == TestInputType.Boolean)
        {
            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return "Yes";
            }

            if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                return "No";
            }
        }

        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static string DescribeEvidenceSummary(TestItem testItem)
    {
        if (testItem.EvidenceRecords.Count == 0)
        {
            return "No evidence attached.";
        }

        var fileNames = testItem.EvidenceRecords
            .OrderByDescending(evidence => evidence.AttachedAt)
            .Select(evidence => $"- {evidence.EvidenceType}: {evidence.StoredFileName}");
        return $"{testItem.EvidenceRecords.Count} evidence record(s)" +
            Environment.NewLine +
            string.Join(Environment.NewLine, fileNames);
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

    private static string HistoryNotes(TestItem testItem, ResultEntry result)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(result.Comments))
        {
            parts.Add(result.Comments.Trim());
        }

        if (result.CapturedInputValues.Count > 0)
        {
            parts.Add(DescribeCapturedInputValues(testItem, result).Replace(Environment.NewLine, "; "));
        }

        return parts.Count == 0 ? "-" : string.Join(" | ", parts);
    }

    private string NextTestReference(Guid sectionId, Guid assetId)
    {
        var selectedSection = project?.Sections.SingleOrDefault(section => section.SectionId == sectionId);
        var asset = FindAsset(assetId);
        var nextNumber = (selectedSection?.TestItems.Count(testItem => testItem.AssetId == assetId) ?? 0) + 1;
        return $"{ReferenceCodeFromSection(selectedSection?.Title)}-{ReferenceCodeFromAsset(asset)}-{nextNumber:000}";
    }

    private void OpenFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = projectFolderPath,
            UseShellExecute = true
        });
    }

    private void OpenFatRunner()
    {
        using var form = new ExecutionRunnerForm(projectFolderPath);
        form.ShowDialog(this);
        LoadProject();
    }

    private void ShowBlocked(string message)
    {
        MessageBox.Show(this, message, "Operation blocked", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private bool ConfirmDestructive(string title, string message)
    {
        var result = MessageBox.Show(
            this,
            message + Environment.NewLine + Environment.NewLine + "This will be recorded in the audit trail and cannot be undone from this screen.",
            title,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        return result == DialogResult.Yes;
    }

    private static Control CreateShellSurface(Control content, Padding padding, bool elevated = false)
    {
        var border = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
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

    private static TableLayoutPanel CreateDetailsTable()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 2,
            Padding = new Padding(8)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return table;
    }

    private static ListView CreateListView(params (string Text, int Width)[] columns)
    {
        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            GridLines = false,
            BorderStyle = BorderStyle.FixedSingle
        };

        foreach (var column in columns)
        {
            list.Columns.Add(column.Text, column.Width);
        }

        return list;
    }

    private static Label CreatePanelTitle(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 10.5f, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 10)
        };
    }

    private static Control CreateMetricCard(string label, string value, string caption, Color backColor, Color foreColor)
    {
        var panel = new TableLayoutPanel
        {
            Width = 210,
            Height = 118,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = backColor,
            Padding = new Padding(12, 10, 12, 10),
            Margin = new Padding(0, 0, 10, 10)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Paint += (_, e) =>
        {
            using var borderPen = new Pen(AppTheme.Current.Border);
            var rect = panel.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            e.Graphics.DrawRectangle(borderPen, rect);
        };

        panel.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            ForeColor = AppTheme.Current.TextSecondary,
            Margin = new Padding(0, 0, 0, 4),
            UseMnemonic = false
        }, 0, 0);
        panel.Controls.Add(new Label
        {
            Text = value,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 34,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Bold),
            ForeColor = foreColor,
            Margin = new Padding(0, 0, 0, 4),
            AutoEllipsis = true,
            UseMnemonic = false
        }, 0, 1);
        panel.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = AppTheme.Current.TextMuted,
            UseMnemonic = false
        }, 0, 2);
        return panel;
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

    private static void SetKeyValues(TableLayoutPanel table, params (string Label, string? Value)[] rows)
    {
        table.SuspendLayout();
        table.Controls.Clear();
        table.RowStyles.Clear();
        table.RowCount = 0;

        for (var index = 0; index < rows.Length; index++)
        {
            table.RowCount++;
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = rows[index].Label,
                AutoSize = true,
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                Margin = new Padding(0, 3, 12, 8)
            };
            var value = new Label
            {
                Text = string.IsNullOrWhiteSpace(rows[index].Value) ? "-" : rows[index].Value,
                AutoSize = true,
                MaximumSize = new Size(900, 0),
                Margin = new Padding(0, 3, 0, 8)
            };

            table.Controls.Add(label, 0, index);
            table.Controls.Add(value, 1, index);
        }

        table.ResumeLayout();
    }

    private static void AddSummaryRow(
        TableLayoutPanel table,
        int row,
        string leftLabel,
        Label leftValue,
        string rightLabel,
        Label rightValue)
    {
        while (table.RowCount <= row)
        {
            table.RowCount++;
        }

        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(CreateSummaryLabel(leftLabel), 0, row);
        leftValue.AutoSize = true;
        leftValue.Anchor = AnchorStyles.Left;
        table.Controls.Add(leftValue, 1, row);
        table.Controls.Add(CreateSummaryLabel(rightLabel), 2, row);
        rightValue.AutoSize = true;
        rightValue.Anchor = AnchorStyles.Left;
        table.Controls.Add(rightValue, 3, row);
    }

    private static Label CreateSummaryLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 3, 8, 3)
        };
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

    private Control CreateHeaderRow()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 10)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        titleLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        titleLabel.Margin = new Padding(0, 0, 16, 0);
        row.Controls.Add(titleLabel, 0, 0);

        var actingPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var actingPrefix = new Label
        {
            Text = "Acting as",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
            Margin = new Padding(0, 0, 6, 6),
            ForeColor = AppTheme.Current.TextSecondary,
            UseMnemonic = false
        };

        actingAsValueLabel.Text = "(none)";
        actingAsValueLabel.AutoSize = true;
        actingAsValueLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        actingAsValueLabel.Margin = new Padding(0, 0, 10, 6);
        actingAsValueLabel.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        actingAsValueLabel.ForeColor = AppTheme.Current.Accent;
        actingAsValueLabel.UseMnemonic = false;

        switchUserButton.Margin = new Padding(0, 0, 0, 2);
        switchUserButton.UseMnemonic = false;
        switchUserButton.Click += (_, _) => OpenSwitchUserDialog();

        actingPanel.Controls.Add(actingPrefix);
        actingPanel.Controls.Add(actingAsValueLabel);
        actingPanel.Controls.Add(switchUserButton);

        row.Controls.Add(actingPanel, 1, 0);
        return row;
    }

    private void EnsureActiveUserSelected(bool promptIfMissing)
    {
        if (project is null)
        {
            return;
        }

        if (activeUser is not null)
        {
            var stillThere = project.Users.FirstOrDefault(user => user.UserId == activeUser.UserId && user.IsActive);
            if (stillThere is not null)
            {
                activeUser = ActiveUserContext.FromUser(stillThere);
                RenderActingAs();
                return;
            }
            activeUser = null;
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
                RenderActingAs();
                return;
            }

            if (promptIfMissing && TryAddOperatorToProject(operatorProfile))
            {
                LoadProject();
                return;
            }
        }

        var defaultUser = ActiveUserContext.PickSensibleDefault(project);
        if (defaultUser is not null)
        {
            activeUser = ActiveUserContext.FromUser(defaultUser);
        }

        RenderActingAs();
    }

    private bool TryAddOperatorToProject(OperatorProfile operatorProfile)
    {
        var prompt = MessageBox.Show(
            this,
            $"You are signed in as '{operatorProfile.DisplayName}', but this project does not have you in its user registry yet." + Environment.NewLine + Environment.NewLine +
            "Add yourself to this project so your actions are properly attributed?",
            "TestTrace - Join Project",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (prompt != DialogResult.Yes)
        {
            return false;
        }

        var usersService = new TestTrace_V1.Workspace.UsersAuthorityService(repository);
        var result = usersService.AddUser(new TestTrace_V1.Contracts.AddUserRequest
        {
            ProjectFolderPath = projectFolderPath,
            DisplayName = operatorProfile.DisplayName,
            Email = operatorProfile.Email,
            Phone = operatorProfile.Phone,
            Organisation = operatorProfile.Organisation,
            Actor = operatorProfile.DisplayName
        });

        if (!result.Succeeded)
        {
            MessageBox.Show(
                this,
                result.ErrorMessage ?? "Could not add you to this project.",
                "TestTrace",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private void RenderActingAs()
    {
        if (activeUser is null)
        {
            actingAsValueLabel.Text = "(none selected)";
            actingAsValueLabel.ForeColor = AppTheme.Current.FailForeground;
            switchUserButton.Text = "Choose User";
        }
        else
        {
            actingAsValueLabel.Text = $"{activeUser.DisplayName} ({activeUser.Initials})";
            actingAsValueLabel.ForeColor = AppTheme.Current.Accent;
            switchUserButton.Text = "Switch";
        }
    }

    private void OpenSwitchUserDialog()
    {
        if (project is null)
        {
            return;
        }

        if (project.Users.Count(user => user.IsActive) == 0)
        {
            var go = MessageBox.Show(
                this,
                "There are no active users in this project. Open Users & Authority to add one?",
                "TestTrace",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (go == DialogResult.Yes)
            {
                OpenUsersAuthorityDialog();
            }
            return;
        }

        using var form = new SwitchUserForm(project, activeUser?.UserId);
        var result = form.ShowDialog(this);
        if (form.ManageUsersRequested)
        {
            OpenUsersAuthorityDialog();
            return;
        }

        if (result == DialogResult.OK)
        {
            var picked = project.Users.FirstOrDefault(user => user.UserId == form.SelectedUserId);
            if (picked is not null)
            {
                activeUser = ActiveUserContext.FromUser(picked);
                RenderActingAs();
                statusLabel.Text = $"Acting as {picked.DisplayName}.";
            }
        }
    }

    private void OpenUsersAuthorityDialog()
    {
        using var form = new UsersAuthorityForm(projectFolderPath, CurrentActor);
        form.ShowDialog(this);
        LoadProject();
        EnsureActiveUserSelected(promptIfMissing: false);
    }

    private static string FormatDate(DateOnly? value)
    {
        return value?.ToString("dd MMM yyyy") ?? "-";
    }

    private static string ReferenceCodeFromSection(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "GEN";
        }

        var normalized = title.Trim().ToUpperInvariant();
        if (normalized.Contains("MECHANICAL"))
        {
            return "MEC";
        }

        if (normalized.Contains("ELECTRICAL"))
        {
            return "ELE";
        }

        if (normalized.Contains("SOFTWARE"))
        {
            return "SFT";
        }

        return InitialCode(title, 3);
    }

    private static string ReferenceCodeFromAsset(Asset? asset)
    {
        if (asset is null)
        {
            return "TST";
        }

        var type = asset.Type?.Trim();
        if (!string.IsNullOrWhiteSpace(type))
        {
            return type.ToUpperInvariant() switch
            {
                "MOTOR" => "MOT",
                "GEARBOX" => "GBX",
                "FEEDER" => "FDR",
                "SENSOR" => "SNS",
                "VALVE" => "VLV",
                "PANEL" => "PNL",
                "SOFTWARE" => "SFT",
                _ => InitialCode(type, 3)
            };
        }

        return InitialCode(asset.Name, 3);
    }

    private static string InitialCode(string text, int length)
    {
        var words = text
            .Split(new[] { ' ', '-', '_', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Any(char.IsLetterOrDigit))
            .ToList();

        var fromWords = new string(words
            .Select(word => char.ToUpperInvariant(word.First(char.IsLetterOrDigit)))
            .Take(length)
            .ToArray());
        if (fromWords.Length == length)
        {
            return fromWords;
        }

        var compact = new string(text.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
        return compact.Length >= length
            ? compact[..length]
            : compact.PadRight(length, 'X');
    }

    private sealed record ProjectNodeTag(Guid ProjectId);
    private sealed record SectionNodeTag(Guid SectionId);
    private sealed record AssetNodeTag(Guid SectionId, Guid AssetId, bool IsSubAsset);
    private sealed record TestNodeTag(Guid SectionId, Guid? AssetId, Guid TestItemId);
    private sealed record InputControlTag(Guid TestInputId, bool Required);
}
