using System.Diagnostics;
using TestTrace_V1;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;

namespace TestTrace_V1.UI;

public sealed class MainForm : Form
{
    private readonly JsonProjectRepository repository = new();
    private readonly TextBox projectsRootTextBox = new();
    private readonly ListView projectsList = new();
    private readonly Label statusLabel = new();
    private readonly SplitContainer browserSplit = new();
    private readonly TableLayoutPanel previewDetailsTable = CreateDetailsTable();
    private readonly Label previewTitleLabel = new();
    private readonly Label previewSubtitleLabel = new();
    private readonly Label previewHintLabel = new();
    private readonly Label previewStateBadge = new();
    private readonly Button previewOpenButton = new();
    private readonly Button previewRunButton = new();
    private readonly Button previewFolderButton = new();
    private readonly Label operatorValueLabel = new();
    private readonly Button switchOperatorButton = new() { Text = "Switch...", AutoSize = true, UseMnemonic = false, Margin = new Padding(0, 0, 0, 0) };

    public MainForm()
    {
        Text = $"TestTrace V1 [{TestTraceAppEnvironment.ModeLabel}]";
        MinimumSize = new Size(1120, 720);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

        InitializeLayout();
        AppTheme.Apply(this);
        projectsRootTextBox.Text = DefaultProjectsRoot();
        ReloadProjects();
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

        layout.Controls.Add(CreateHeader(), 0, 0);
        layout.Controls.Add(CreateRootSurface(), 0, 1);

        browserSplit.Dock = DockStyle.Fill;
        browserSplit.Orientation = Orientation.Vertical;
        browserSplit.SizeChanged += (_, _) => EnsureBrowserSplitDistance();
        browserSplit.Panel1.Controls.Add(CreateProjectListSurface());
        browserSplit.Panel2.Controls.Add(CreateProjectPreviewSurface());
        layout.Controls.Add(browserSplit, 0, 2);

        layout.Controls.Add(CreateFooterActions(), 0, 3);

        Controls.Add(layout);
        Shown += (_, _) =>
        {
            EnsureBrowserSplitDistance();
            ResizeProjectColumns();
            UpdateProjectPreview();
        };
        SizeChanged += (_, _) => ResizeProjectColumns();
    }

    private Control CreateHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 14)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var brandRow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 0, 0, 4),
            WrapContents = false
        };

        var brandBadge = BrandAssets.CreatePictureBox(
            BrandAssets.LoadBadge(),
            new Size(42, 42),
            new Padding(0, 0, 10, 0));
        brandRow.Controls.Add(brandBadge);

        var title = new Label
        {
            Text = TestTraceAppEnvironment.IsSandbox ? "TestTrace - SANDBOX" : "TestTrace",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 19, FontStyle.Bold),
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 5, 0, 0),
            UseMnemonic = false
        };
        brandRow.Controls.Add(title);

        var subtitle = new Label
        {
            Text = TestTraceAppEnvironment.IsSandbox
                ? "Sandbox workspace for governed FAT pilots and UI refinement."
                : "Governed FAT workspace for building, executing, and releasing machine test records.",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 0),
            ForeColor = AppTheme.Current.TextSecondary,
            UseMnemonic = false
        };

        header.Controls.Add(brandRow, 0, 0);
        header.Controls.Add(subtitle, 0, 1);

        var operatorPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            Margin = new Padding(0)
        };

        var prefix = new Label
        {
            Text = "Signed in as",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
            Margin = new Padding(0, 0, 6, 6),
            ForeColor = AppTheme.Current.TextSecondary,
            UseMnemonic = false
        };
        operatorValueLabel.AutoSize = true;
        operatorValueLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        operatorValueLabel.Margin = new Padding(0, 0, 10, 6);
        operatorValueLabel.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        operatorValueLabel.ForeColor = AppTheme.Current.Accent;
        operatorValueLabel.UseMnemonic = false;
        operatorValueLabel.Text = OperatorSession.Current is null
            ? "(none)"
            : $"{OperatorSession.Current.DisplayName} ({OperatorSession.Current.Initials})";

        switchOperatorButton.Click += (_, _) => SwitchOperator();
        operatorPanel.Controls.Add(prefix);
        operatorPanel.Controls.Add(operatorValueLabel);
        operatorPanel.Controls.Add(switchOperatorButton);

        header.Controls.Add(operatorPanel, 1, 0);
        header.SetRowSpan(operatorPanel, 2);

        return header;
    }

    private void SwitchOperator()
    {
        using var picker = new OperatorSelectionForm(OperatorSession.Registry);
        if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedOperator is null)
        {
            return;
        }

        OperatorSession.Replace(picker.SelectedOperator);
        operatorValueLabel.Text = $"{picker.SelectedOperator.DisplayName} ({picker.SelectedOperator.Initials})";
        statusLabel.Text = $"Signed in as {picker.SelectedOperator.DisplayName}.";
    }

    private Control CreateRootSurface()
    {
        var rootPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 5,
            AutoSize = true,
            Margin = new Padding(0)
        };
        rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        rootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var rootLabel = new Label
        {
            Text = TestTraceAppEnvironment.IsSandbox ? "Sandbox projects root" : "Projects root",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 0, 10, 0),
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        };

        projectsRootTextBox.Dock = DockStyle.Fill;
        projectsRootTextBox.Margin = new Padding(0);

        var browseButton = new Button { Text = "Browse", AutoSize = true, Margin = new Padding(10, 0, 0, 0) };
        browseButton.Click += (_, _) => BrowseForRoot();

        var refreshButton = new Button { Text = "Refresh", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        refreshButton.Click += (_, _) => ReloadProjects();

        var openRootButton = new Button { Text = "Open Folder", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        openRootButton.Click += (_, _) => OpenFolder(projectsRootTextBox.Text);

        rootPanel.Controls.Add(rootLabel, 0, 0);
        rootPanel.Controls.Add(projectsRootTextBox, 1, 0);
        rootPanel.Controls.Add(browseButton, 2, 0);
        rootPanel.Controls.Add(refreshButton, 3, 0);
        rootPanel.Controls.Add(openRootButton, 4, 0);

        return CreateSurface(rootPanel, new Padding(12));
    }

    private Control CreateProjectListSurface()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        panel.Controls.Add(CreateSectionHeading("Projects"), 0, 0);

        projectsList.Dock = DockStyle.Fill;
        projectsList.View = View.Details;
        projectsList.FullRowSelect = true;
        projectsList.HideSelection = false;
        projectsList.MultiSelect = false;
        projectsList.Columns.Add("Code", 110);
        projectsList.Columns.Add("Project", 250);
        projectsList.Columns.Add("Customer", 160);
        projectsList.Columns.Add("State", 150);
        projectsList.Columns.Add("Folder", 360);
        projectsList.SelectedIndexChanged += (_, _) => UpdateProjectPreview();
        projectsList.DoubleClick += (_, _) => OpenSelectedProject();
        projectsList.Resize += (_, _) => ResizeProjectColumns();

        panel.Controls.Add(projectsList, 0, 1);
        return CreateSurface(panel, new Padding(12), elevated: true);
    }

    private Control CreateProjectPreviewSurface()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        panel.Controls.Add(CreateSectionHeading("Selected Project"), 0, 0);

        var titleRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };
        titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var titleStack = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            Margin = new Padding(0)
        };

        previewTitleLabel.AutoSize = true;
        previewTitleLabel.Font = new Font(Font.FontFamily, 14, FontStyle.Bold);
        previewTitleLabel.Margin = new Padding(0, 0, 0, 4);

        previewSubtitleLabel.AutoSize = true;
        previewSubtitleLabel.Margin = new Padding(0);
        previewSubtitleLabel.ForeColor = AppTheme.Current.TextSecondary;

        titleStack.Controls.Add(previewTitleLabel, 0, 0);
        titleStack.Controls.Add(previewSubtitleLabel, 0, 1);

        previewStateBadge.AutoSize = true;
        previewStateBadge.Padding = new Padding(10, 5, 10, 5);
        previewStateBadge.Margin = new Padding(12, 2, 0, 0);
        previewStateBadge.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);

        titleRow.Controls.Add(titleStack, 0, 0);
        titleRow.Controls.Add(previewStateBadge, 1, 0);

        previewHintLabel.AutoSize = true;
        previewHintLabel.MaximumSize = new Size(420, 0);
        previewHintLabel.Margin = new Padding(0, 0, 0, 14);
        previewHintLabel.ForeColor = AppTheme.Current.TextSecondary;

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 12, 0, 0)
        };

        previewOpenButton.Text = "Open Project";
        previewOpenButton.AutoSize = true;
        previewOpenButton.Click += (_, _) => OpenSelectedProject();

        previewRunButton.Text = "Start FAT Runner";
        previewRunButton.AutoSize = true;
        previewRunButton.Margin = new Padding(8, 0, 0, 0);
        previewRunButton.Click += (_, _) => OpenSelectedRunner();

        previewFolderButton.Text = "Open Folder";
        previewFolderButton.AutoSize = true;
        previewFolderButton.Margin = new Padding(8, 0, 0, 0);
        previewFolderButton.Click += (_, _) => OpenSelectedProjectFolder();

        actions.Controls.Add(previewOpenButton);
        actions.Controls.Add(previewRunButton);
        actions.Controls.Add(previewFolderButton);

        panel.Controls.Add(titleRow, 0, 1);
        panel.Controls.Add(previewHintLabel, 0, 2);
        panel.Controls.Add(previewDetailsTable, 0, 3);
        panel.Controls.Add(actions, 0, 4);

        return CreateSurface(panel, new Padding(12), elevated: true);
    }

    private Control CreateFooterActions()
    {
        var outer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };

        var newProjectButton = new Button { Text = "New Project", AutoSize = true };
        newProjectButton.Click += (_, _) => ShowNewProjectForm();

        var openProjectButton = new Button { Text = "Open Project", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        openProjectButton.Click += (_, _) => OpenSelectedProject();

        var runnerButton = new Button { Text = "Start FAT Runner", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        runnerButton.Click += (_, _) => OpenSelectedRunner();

        var openSelectedButton = new Button { Text = "Open Selected Folder", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        openSelectedButton.Click += (_, _) => OpenSelectedProjectFolder();

        statusLabel.AutoSize = true;
        statusLabel.Anchor = AnchorStyles.Left;
        statusLabel.Margin = new Padding(16, 6, 0, 0);
        statusLabel.ForeColor = AppTheme.Current.TextSecondary;

        actions.Controls.Add(newProjectButton);
        actions.Controls.Add(openProjectButton);
        actions.Controls.Add(runnerButton);
        actions.Controls.Add(openSelectedButton);
        actions.Controls.Add(statusLabel);

        outer.Controls.Add(actions, 0, 0);
        return outer;
    }

    private void BrowseForRoot()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose the folder where TestTrace projects are stored",
            UseDescriptionForTitle = true,
            SelectedPath = projectsRootTextBox.Text
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            projectsRootTextBox.Text = dialog.SelectedPath;
            ReloadProjects();
        }
    }

    private void ReloadProjects()
    {
        var previouslySelectedFolder = SelectedProjectSummary()?.Folder;
        projectsList.BeginUpdate();
        try
        {
            projectsList.Items.Clear();
            var root = projectsRootTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(root))
            {
                statusLabel.Text = "Choose a projects root.";
                UpdateProjectPreview();
                return;
            }

            if (!Directory.Exists(root))
            {
                statusLabel.Text = TestTraceAppEnvironment.IsSandbox
                    ? "Sandbox projects root does not exist yet. New Project will create it."
                    : "Projects root does not exist yet. New Project will create it.";
                UpdateProjectPreview();
                return;
            }

            var loadedCount = 0;
            foreach (var folder in Directory.EnumerateDirectories(root).OrderByDescending(Directory.GetLastWriteTimeUtc))
            {
                var projectFile = Path.Combine(folder, "project.testtrace.json");
                if (!File.Exists(projectFile))
                {
                    continue;
                }

                ProjectBrowserSummary summary;
                try
                {
                    var project = repository.Load(ProjectLocation.FromProjectFolder(folder));
                    summary = ProjectBrowserSummary.FromProject(project, folder);
                    loadedCount++;
                }
                catch (Exception ex)
                {
                    summary = ProjectBrowserSummary.Invalid(folder, ex.Message);
                }

                var item = new ListViewItem(summary.ProjectCode);
                item.SubItems.Add(summary.ProjectName);
                item.SubItems.Add(summary.CustomerName);
                item.SubItems.Add(summary.StateText);
                item.SubItems.Add(summary.Folder);
                item.Tag = summary;
                projectsList.Items.Add(item);
            }

            ResizeProjectColumns();
            RestoreProjectSelection(previouslySelectedFolder);

            var mode = TestTraceAppEnvironment.IsSandbox ? "Sandbox: " : string.Empty;
            statusLabel.Text = loadedCount == 1 ? $"{mode}1 project loaded." : $"{mode}{loadedCount} projects loaded.";
        }
        finally
        {
            projectsList.EndUpdate();
        }

        UpdateProjectPreview();
    }

    private void RestoreProjectSelection(string? projectFolder)
    {
        if (!string.IsNullOrWhiteSpace(projectFolder))
        {
            SelectProjectFolder(projectFolder);
        }

        if (projectsList.SelectedItems.Count == 0 && projectsList.Items.Count > 0)
        {
            projectsList.Items[0].Selected = true;
            projectsList.Items[0].Focused = true;
        }
    }

    private void ResizeProjectColumns()
    {
        if (projectsList.Columns.Count != 5)
        {
            return;
        }

        var availableWidth = projectsList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 6;
        if (availableWidth <= 0)
        {
            return;
        }

        const int codeWidth = 110;
        const int customerWidth = 150;
        const int stateWidth = 130;
        const int minimumProjectWidth = 220;
        const int minimumFolderWidth = 280;

        var remainingWidth = Math.Max(minimumProjectWidth + minimumFolderWidth, availableWidth - codeWidth - customerWidth - stateWidth);
        var projectWidth = Math.Max(minimumProjectWidth, (int)(remainingWidth * 0.36));
        var folderWidth = Math.Max(minimumFolderWidth, remainingWidth - projectWidth);

        projectsList.Columns[0].Width = codeWidth;
        projectsList.Columns[1].Width = projectWidth;
        projectsList.Columns[2].Width = customerWidth;
        projectsList.Columns[3].Width = stateWidth;
        projectsList.Columns[4].Width = folderWidth;
    }

    private void UpdateProjectPreview()
    {
        previewSubtitleLabel.ForeColor = AppTheme.Current.TextSecondary;
        previewHintLabel.ForeColor = AppTheme.Current.TextSecondary;

        var summary = SelectedProjectSummary();
        if (summary is null)
        {
            previewTitleLabel.Text = "Select a project";
            previewSubtitleLabel.Text = "Choose a project from the list to see project context and quick actions.";
            previewHintLabel.Text = TestTraceAppEnvironment.IsSandbox
                ? "This sandbox browser is now ready for a calmer first-run flow. Pick a project to inspect it, or start a new one in the sandbox root."
                : "Choose an existing project to continue governed FAT work, or create a new one from this shell.";
            SetPreviewBadge("Idle", AppTheme.Current.SurfaceAlt, AppTheme.Current.TextSecondary);
            SetPreviewRows(
                ("Projects root", DisplayValue(projectsRootTextBox.Text)),
                ("Loaded items", projectsList.Items.Count.ToString()),
                ("Mode", TestTraceAppEnvironment.ModeLabel),
                ("Next step", projectsList.Items.Count == 0 ? "Create a new project" : "Open a selected project"));
            previewOpenButton.Enabled = false;
            previewRunButton.Enabled = false;
            previewFolderButton.Enabled = Directory.Exists(projectsRootTextBox.Text);
            return;
        }

        previewFolderButton.Enabled = Directory.Exists(summary.Folder);

        if (!summary.IsValid)
        {
            previewTitleLabel.Text = summary.ProjectCode;
            previewSubtitleLabel.Text = "This project file could not be loaded cleanly.";
            previewHintLabel.Text = "Open the folder to inspect or repair the saved files. The browser is keeping the item visible so it stays discoverable rather than silently disappearing.";
            SetPreviewBadge("Invalid", AppTheme.Current.FailSoftBackground, AppTheme.Current.TextPrimary);
            SetPreviewRows(
                ("Folder", DisplayValue(summary.Folder)),
                ("State", summary.StateText),
                ("Error", DisplayValue(summary.ErrorMessage)),
                ("Recommended action", "Open the folder and inspect project.testtrace.json"));
            previewOpenButton.Enabled = false;
            previewRunButton.Enabled = false;
            return;
        }

        previewTitleLabel.Text = $"{summary.ProjectCode} - {summary.ProjectName}";
        previewSubtitleLabel.Text = $"{summary.ProjectType} | {summary.Machine}";
        previewHintLabel.Text = summary.State switch
        {
            ProjectState.DraftContractBuilt => "The draft contract is ready. Next step is to open the workspace and continue defining sections, assets, and test items.",
            ProjectState.Executable => "Project is ready for governed execution. This is ready for result capture, evidence attachment, approvals, and release work.",
            ProjectState.Released => "Project has been released. Open it to review the final record, evidence, approvals, and exported outputs.",
            _ => "Project is ready to open."
        };

        SetPreviewBadge(summary.StateText, summary.StateBadgeBackColor, summary.StateBadgeForeColor);
        SetPreviewRows(
            ("Customer", summary.CustomerName),
            ("Project Scope", summary.ScopeNarrative),
            ("Lead Test Engineer", summary.LeadTestEngineer),
            ("Release Authority", summary.ReleaseAuthority),
            ("Sections", summary.SectionCount.ToString()),
            ("Assets", $"{summary.TopLevelAssetCount} top-level / {summary.SubAssetCount} sub-assets"),
            ("Tests", summary.TestCount.ToString()),
            ("Evidence", summary.EvidenceCount.ToString()),
            ("Folder", summary.Folder));

        previewOpenButton.Enabled = true;
        previewRunButton.Enabled = true;
    }

    private void SetPreviewBadge(string text, Color backColor, Color foreColor)
    {
        previewStateBadge.Text = text.ToUpperInvariant();
        previewStateBadge.BackColor = backColor;
        previewStateBadge.ForeColor = foreColor;
    }

    private void SetPreviewRows(params (string Label, string? Value)[] rows)
    {
        previewDetailsTable.SuspendLayout();
        previewDetailsTable.Controls.Clear();
        previewDetailsTable.RowStyles.Clear();
        previewDetailsTable.RowCount = 0;

        for (var index = 0; index < rows.Length; index++)
        {
            previewDetailsTable.RowCount++;
            previewDetailsTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = rows[index].Label,
                AutoSize = true,
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                Margin = new Padding(0, 3, 12, 10)
            };
            var value = new Label
            {
                Text = DisplayValue(rows[index].Value),
                AutoSize = true,
                MaximumSize = new Size(420, 0),
                Margin = new Padding(0, 3, 0, 10),
                ForeColor = AppTheme.Current.TextSecondary
            };

            previewDetailsTable.Controls.Add(label, 0, index);
            previewDetailsTable.Controls.Add(value, 1, index);
        }

        previewDetailsTable.ResumeLayout();
    }

    private void EnsureBrowserSplitDistance()
    {
        if (browserSplit.Width <= 0)
        {
            return;
        }

        const int desiredListWidth = 760;
        const int minimumListWidth = 460;
        const int minimumPreviewWidth = 340;

        browserSplit.Panel1MinSize = minimumListWidth;
        browserSplit.Panel2MinSize = minimumPreviewWidth;

        var min = minimumListWidth;
        var max = browserSplit.Width - minimumPreviewWidth;
        if (max < min)
        {
            return;
        }

        var target = Math.Min(desiredListWidth, max);
        target = Math.Max(min, target);
        if (browserSplit.SplitterDistance != target)
        {
            browserSplit.SplitterDistance = target;
        }
    }

    private void ShowNewProjectForm()
    {
        using var form = new NewProjectForm(projectsRootTextBox.Text);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            projectsRootTextBox.Text = form.ProjectsRootDirectory;
            ReloadProjects();
            SelectProjectFolder(form.CreatedProjectFolderPath);
            OpenProject(form.CreatedProjectFolderPath);
        }
    }

    private void SelectProjectFolder(string? projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return;
        }

        foreach (ListViewItem item in projectsList.Items)
        {
            if (item.Tag is ProjectBrowserSummary summary &&
                string.Equals(summary.Folder, projectFolder, StringComparison.OrdinalIgnoreCase))
            {
                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();
                break;
            }
        }
    }

    private void OpenSelectedProjectFolder()
    {
        var summary = SelectedProjectSummary();
        if (summary is null)
        {
            MessageBox.Show(this, "Select a project first.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        OpenFolder(summary.Folder);
    }

    private void OpenSelectedProject()
    {
        var summary = SelectedProjectSummary();
        if (summary is null)
        {
            MessageBox.Show(this, "Select a project first.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!summary.IsValid)
        {
            MessageBox.Show(
                this,
                "This project file could not be loaded cleanly and cannot be opened in the workspace." + Environment.NewLine + Environment.NewLine +
                (string.IsNullOrWhiteSpace(summary.ErrorMessage) ? "Open the folder to inspect or repair project.testtrace.json." : summary.ErrorMessage),
                "TestTrace",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        OpenProject(summary.Folder);
    }

    private void OpenSelectedRunner()
    {
        var summary = SelectedProjectSummary();
        if (summary is null)
        {
            MessageBox.Show(this, "Select a project first.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!summary.IsValid)
        {
            MessageBox.Show(
                this,
                "This project file could not be loaded cleanly and cannot be opened in the FAT Runner." + Environment.NewLine + Environment.NewLine +
                (string.IsNullOrWhiteSpace(summary.ErrorMessage) ? "Open the folder to inspect or repair project.testtrace.json." : summary.ErrorMessage),
                "TestTrace",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        OpenRunner(summary.Folder);
    }

    private ProjectBrowserSummary? SelectedProjectSummary()
    {
        return projectsList.SelectedItems.Count == 0
            ? null
            : projectsList.SelectedItems[0].Tag as ProjectBrowserSummary;
    }

    private void OpenProject(string? projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return;
        }

        if (!File.Exists(Path.Combine(projectFolder, "project.testtrace.json")))
        {
            MessageBox.Show(this, "Selected folder does not contain a TestTrace project file.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var form = new ProjectWorkspaceForm(projectFolder);
        form.ShowDialog(this);
        ReloadProjects();
        SelectProjectFolder(projectFolder);
    }

    private void OpenRunner(string? projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return;
        }

        if (!File.Exists(Path.Combine(projectFolder, "project.testtrace.json")))
        {
            MessageBox.Show(this, "Selected folder does not contain a TestTrace project file.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var form = new ExecutionRunnerForm(projectFolder);
        form.ShowDialog(this);
        ReloadProjects();
        SelectProjectFolder(projectFolder);
    }

    private void OpenFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            MessageBox.Show(this, "Folder does not exist yet.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }

    private static Control CreateSurface(Control content, Padding padding, bool elevated = false)
    {
        var border = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            Margin = new Padding(0, 0, 0, 12),
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

    private static Label CreateSectionHeading(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 10, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 10)
        };
    }

    private static TableLayoutPanel CreateDetailsTable()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Margin = new Padding(0)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return table;
    }

    private static string DisplayValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string DefaultProjectsRoot()
    {
        return TestTraceAppEnvironment.DefaultProjectsRoot();
    }

    private sealed record ProjectBrowserSummary(
        string Folder,
        string ProjectCode,
        string ProjectName,
        string CustomerName,
        ProjectState? State,
        string ProjectType,
        string Machine,
        string ScopeNarrative,
        string LeadTestEngineer,
        string ReleaseAuthority,
        int SectionCount,
        int TopLevelAssetCount,
        int SubAssetCount,
        int TestCount,
        int EvidenceCount,
        bool IsValid,
        string? ErrorMessage)
    {
        public string StateText => State switch
        {
            ProjectState.DraftContractBuilt => "Draft Contract",
            ProjectState.Executable => "Ready for Execution",
            ProjectState.Released => "Released",
            _ => "Invalid"
        };

        public Color StateBadgeBackColor => State switch
        {
            ProjectState.DraftContractBuilt => AppTheme.Current.StatusDraftBackground,
            ProjectState.Executable => AppTheme.Current.StatusExecutableBackground,
            ProjectState.Released => AppTheme.Current.StatusReleasedBackground,
            _ => AppTheme.Current.FailSoftBackground
        };

        public Color StateBadgeForeColor => State switch
        {
            ProjectState.DraftContractBuilt => AppTheme.Current.StatusDraftForeground,
            ProjectState.Executable => AppTheme.Current.StatusExecutableForeground,
            ProjectState.Released => AppTheme.Current.StatusReleasedForeground,
            _ => AppTheme.Current.TextPrimary
        };

        public static ProjectBrowserSummary FromProject(TestTraceProject project, string folder)
        {
            return new ProjectBrowserSummary(
                folder,
                project.ContractRoot.ProjectCode,
                project.ContractRoot.ProjectName,
                project.ContractRoot.CustomerName,
                project.State,
                project.ContractRoot.ProjectType,
                $"{project.ContractRoot.MachineModel} / {project.ContractRoot.MachineSerialNumber}",
                project.ContractRoot.ScopeNarrative,
                project.ContractRoot.LeadTestEngineer,
                project.ContractRoot.ReleaseAuthority,
                project.Sections.Count,
                project.Assets.Count(asset => asset.ParentAssetId is null),
                project.Assets.Count(asset => asset.ParentAssetId is not null),
                project.Sections.Sum(section => section.TestItems.Count),
                project.Sections.Sum(section => section.TestItems.Sum(testItem => testItem.EvidenceRecords.Count)),
                true,
                null);
        }

        public static ProjectBrowserSummary Invalid(string folder, string errorMessage)
        {
            return new ProjectBrowserSummary(
                folder,
                Path.GetFileName(folder),
                "Could not load project",
                string.Empty,
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                0,
                0,
                0,
                0,
                false,
                errorMessage);
        }
    }
}
