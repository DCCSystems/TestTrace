using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;
using TestTrace_V1.Workspace;

namespace TestTrace_V1.UI;

public sealed class NewProjectForm : Form
{
    private const int StepCount = 5;

    private readonly TextBox projectsRootTextBox = new();
    private readonly ComboBox projectTypeComboBox = new();
    private readonly TextBox projectNameTextBox = new();
    private readonly TextBox projectCodeTextBox = new();
    private readonly TextBox projectScopeSummaryTextBox = new();
    private readonly DateTimePicker projectStartDatePicker = new();

    private readonly TextBox customerNameTextBox = new();
    private readonly TextBox customerAddressTextBox = new();
    private readonly TextBox customerCountryTextBox = new();
    private readonly TextBox siteContactNameTextBox = new();
    private readonly TextBox siteContactEmailTextBox = new();
    private readonly TextBox siteContactNumberTextBox = new();
    private readonly TextBox customerProjectReferenceTextBox = new();

    private readonly TextBox machineModelTextBox = new();
    private readonly TextBox machineConfigurationTextBox = new();
    private readonly TextBox machineSerialNumberTextBox = new();
    private readonly TextBox controlPlatformTextBox = new();
    private readonly TextBox machineRoleApplicationTextBox = new();
    private readonly TextBox softwareVersionTextBox = new();

    private readonly TextBox leadTestEngineerTextBox = new();
    private readonly TextBox leadTestEngineerEmailTextBox = new();
    private readonly TextBox leadTestEngineerPhoneTextBox = new();
    private readonly TextBox releaseAuthorityTextBox = new();
    private readonly TextBox finalApprovalAuthorityTextBox = new();
    private readonly TextBox createdByTextBox = new();

    private readonly Label stepLabel = new();
    private readonly Label stepTitleLabel = new();
    private readonly Label stepPurposeLabel = new();
    private readonly Panel stepHostPanel = new();
    private readonly TableLayoutPanel summaryTable = new();
    private readonly TextBox validationTextBox = new();
    private readonly Button backButton = new();
    private readonly Button nextButton = new();
    private readonly Button buildButton = new();
    private readonly List<Panel> stepPanels = [];

    private int currentStep;

    public string ProjectsRootDirectory => projectsRootTextBox.Text.Trim();
    public string? CreatedProjectFolderPath { get; private set; }

    public NewProjectForm(string projectsRootDirectory)
    {
        Text = "Create TestTrace Project";
        MinimumSize = new Size(1120, 780);
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        createdByTextBox.ReadOnly = true;

        InitializeLayout();
        AppTheme.Apply(this);
        projectsRootTextBox.Text = projectsRootDirectory;
        projectTypeComboBox.SelectedItem = "FAT";
        projectStartDatePicker.Value = DateTime.Today;
        createdByTextBox.Text = CurrentActor();
        leadTestEngineerTextBox.Text = CurrentActor();
        releaseAuthorityTextBox.Text = CurrentActor();
        createdByTextBox.BackColor = AppTheme.Current.InputReadOnlyBackground;
        createdByTextBox.ForeColor = AppTheme.Current.InputForeground;

        ShowStep(0);
        UpdateLiveSummary();
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(18)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = CreateHeader();
        layout.Controls.Add(header, 0, 0);
        layout.SetColumnSpan(header, 2);

        stepHostPanel.Dock = DockStyle.Fill;
        stepHostPanel.Padding = new Padding(0, 12, 12, 0);
        CreateStepPanels();
        layout.Controls.Add(stepHostPanel, 0, 1);

        layout.Controls.Add(CreateSummaryPanel(), 1, 1);

        validationTextBox.Dock = DockStyle.Fill;
        validationTextBox.Multiline = true;
        validationTextBox.ReadOnly = true;
        validationTextBox.ScrollBars = ScrollBars.Vertical;
        validationTextBox.BackColor = AppTheme.Current.InputReadOnlyBackground;
        validationTextBox.Margin = new Padding(0, 12, 12, 0);
        layout.Controls.Add(validationTextBox, 0, 2);

        var reviewNote = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Required contract fields freeze when the project is created. Machine context freezes when execution opens. Reporting and contact details remain editable until release.",
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 12, 0, 0)
        };
        layout.Controls.Add(reviewNote, 1, 2);

        var actions = CreateActions();
        layout.Controls.Add(actions, 0, 3);
        layout.SetColumnSpan(actions, 2);

        Controls.Add(layout);
    }

    private Control CreateHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            AutoSize = true
        };
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        stepLabel.AutoSize = true;
        stepLabel.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        stepLabel.Margin = new Padding(0, 0, 0, 4);

        stepTitleLabel.AutoSize = true;
        stepTitleLabel.Font = new Font(Font.FontFamily, 18, FontStyle.Bold);
        stepTitleLabel.Margin = new Padding(0, 0, 0, 4);

        stepPurposeLabel.AutoSize = true;
        stepPurposeLabel.MaximumSize = new Size(920, 0);
        stepPurposeLabel.Margin = new Padding(0, 0, 0, 8);

        header.Controls.Add(stepLabel, 0, 0);
        header.Controls.Add(stepTitleLabel, 0, 1);
        header.Controls.Add(stepPurposeLabel, 0, 2);
        return header;
    }

    private Control CreateSummaryPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(14),
            BackColor = AppTheme.Current.SurfaceElevated
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = "Live Contract Summary",
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12)
        };

        summaryTable.Dock = DockStyle.Top;
        summaryTable.AutoSize = true;
        summaryTable.ColumnCount = 2;
        summaryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 182));
        summaryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        panel.Controls.Add(title, 0, 0);
        panel.Controls.Add(summaryTable, 0, 1);
        return panel;
    }

    private Control CreateActions()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 14, 0, 0)
        };

        buildButton.Text = "Create Project";
        buildButton.AutoSize = true;
        buildButton.Click += (_, _) => BuildContract();

        nextButton.Text = "Next";
        nextButton.AutoSize = true;
        nextButton.Margin = new Padding(8, 0, 0, 0);
        nextButton.Click += (_, _) => MoveNext();

        backButton.Text = "Back";
        backButton.AutoSize = true;
        backButton.Margin = new Padding(8, 0, 0, 0);
        backButton.Click += (_, _) => ShowStep(currentStep - 1);

        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        actions.Controls.Add(buildButton);
        actions.Controls.Add(nextButton);
        actions.Controls.Add(backButton);
        actions.Controls.Add(cancelButton);
        return actions;
    }

    private void CreateStepPanels()
    {
        stepPanels.Add(CreateProjectInformationStep());
        stepPanels.Add(CreateCustomerInformationStep());
        stepPanels.Add(CreateMachineInformationStep());
        stepPanels.Add(CreateRolesStep());
        stepPanels.Add(CreateReviewStep());

        foreach (var panel in stepPanels)
        {
            panel.Dock = DockStyle.Fill;
            WireSummaryUpdates(panel);
        }
    }

    private Panel CreateProjectInformationStep()
    {
        var panel = CreateStepPanel();
        var fields = CreateFieldTable();

        AddRootRow(fields, 0);
        AddComboRow(fields, 1, "Project Type", projectTypeComboBox, ["FAT", "SAT", "InternalValidation", "Investigation"]);
        AddTextRow(fields, 2, "Project Name", projectNameTextBox);
        AddTextRow(fields, 3, "Project Code", projectCodeTextBox);
        AddMultilineRow(fields, 4, "Project Scope Summary", projectScopeSummaryTextBox, 132);
        AddDateRow(fields, 5, "Project Start Date", projectStartDatePicker);

        panel.Controls.Add(fields);
        return panel;
    }

    private Panel CreateCustomerInformationStep()
    {
        var panel = CreateStepPanel();
        var fields = CreateFieldTable();

        AddTextRow(fields, 0, "Customer Name", customerNameTextBox);
        AddMultilineRow(fields, 1, "Customer Address", customerAddressTextBox, 108);
        AddTextRow(fields, 2, "Customer Country", customerCountryTextBox);
        AddTextRow(fields, 3, "Site Contact Name", siteContactNameTextBox);
        AddTextRow(fields, 4, "Site Contact Email", siteContactEmailTextBox);
        AddTextRow(fields, 5, "Site Contact Number", siteContactNumberTextBox);
        AddTextRow(fields, 6, "Customer Project Reference Name / Number", customerProjectReferenceTextBox);

        panel.Controls.Add(fields);
        return panel;
    }

    private Panel CreateMachineInformationStep()
    {
        var panel = CreateStepPanel();
        var fields = CreateFieldTable();

        AddTextRow(fields, 0, "Machine Model", machineModelTextBox);
        AddMultilineRow(fields, 1, "Machine Configuration / Specification", machineConfigurationTextBox, 118);
        AddTextRow(fields, 2, "Machine Serial Number", machineSerialNumberTextBox);
        AddTextRow(fields, 3, "Control Platform", controlPlatformTextBox);
        AddTextRow(fields, 4, "Machine Role / Application", machineRoleApplicationTextBox);
        AddTextRow(fields, 5, "Software Version", softwareVersionTextBox);

        panel.Controls.Add(fields);
        return panel;
    }

    private Panel CreateRolesStep()
    {
        var panel = CreateStepPanel();
        var fields = CreateFieldTable();

        AddTextRow(fields, 0, "Lead Test Engineer", leadTestEngineerTextBox);
        AddTextRow(fields, 1, "Lead Test Engineer Email", leadTestEngineerEmailTextBox);
        AddTextRow(fields, 2, "Lead Test Engineer Phone", leadTestEngineerPhoneTextBox);
        AddTextRow(fields, 3, "Release Authority", releaseAuthorityTextBox);
        AddTextRow(fields, 4, "Final Approval Authority", finalApprovalAuthorityTextBox);
        AddTextRow(fields, 5, "Created By", createdByTextBox);

        var note = CreateNoteLabel("Created By is the contract-root author. Lead Test Engineer is the accountable FAT lead and remains distinct.");
        fields.Controls.Add(note, 0, 6);
        fields.SetColumnSpan(note, 3);

        panel.Controls.Add(fields);
        return panel;
    }

    private Panel CreateReviewStep()
    {
        var panel = CreateStepPanel();
        var review = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 7
        };
        review.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        review.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        review.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        review.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        review.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        review.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        review.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        review.Controls.Add(CreateReviewHeading("Review before Create Project"), 0, 0);
        review.Controls.Add(CreateNoteLabel("You are about to create a governed TestTrace project folder and freeze the required contract identity fields."), 0, 1);
        review.Controls.Add(CreateNoteLabel("Frozen when the project is created: Project Type, Project Name, Project Code, Customer Name, Project Scope Summary, Machine Model, Machine Serial Number, Lead Test Engineer, Release Authority, Final Approval Authority, Created By, and Created At."), 0, 2);
        review.Controls.Add(CreateNoteLabel("Frozen when execution opens: Machine Configuration / Specification, Control Platform, Machine Role / Application, and Software Version."), 0, 3);
        review.Controls.Add(CreateNoteLabel("Editable until release: Project Start Date, customer/site details, contact details, and customer reference metadata."), 0, 4);

        var warning = new Label
        {
            Text = "Create Project creates the record. It does not open execution until you explicitly choose Open for FAT Execution in the workspace.",
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            MaximumSize = new Size(720, 0),
            Margin = new Padding(0, 16, 0, 0)
        };
        review.Controls.Add(warning, 0, 5);

        var timestampNote = new Label
        {
            Text = "Created At is generated automatically when the contract root is built.",
            AutoSize = true,
            MaximumSize = new Size(720, 0),
            Margin = new Padding(0, 8, 0, 0)
        };
        review.Controls.Add(timestampNote, 0, 6);

        panel.Controls.Add(review);
        return panel;
    }

    private void ShowStep(int step)
    {
        if (step < 0 || step >= StepCount)
        {
            return;
        }

        currentStep = step;
        validationTextBox.Clear();
        stepHostPanel.Controls.Clear();
        stepHostPanel.Controls.Add(stepPanels[currentStep]);

        stepLabel.Text = $"Step {currentStep + 1} of {StepCount}";
        stepTitleLabel.Text = StepTitle(currentStep);
        stepPurposeLabel.Text = StepPurpose(currentStep);

        backButton.Enabled = currentStep > 0;
        nextButton.Visible = currentStep < StepCount - 1;
        buildButton.Visible = currentStep == StepCount - 1;
        UpdateLiveSummary();
    }

    private void MoveNext()
    {
        var issues = CurrentStepIssues();
        if (issues.Count > 0)
        {
            validationTextBox.Text = string.Join(Environment.NewLine, issues);
            return;
        }

        ShowStep(currentStep + 1);
    }

    private List<string> CurrentStepIssues()
    {
        return currentStep switch
        {
            0 => ValidateProjectInformationStep(),
            1 => ValidateCustomerInformationStep(),
            2 => ValidateMachineInformationStep(),
            3 => ValidateRolesStep(),
            _ => []
        };
    }

    private List<string> ValidateProjectInformationStep()
    {
        var issues = new List<string>();
        AddRequiredIssue(issues, projectsRootTextBox.Text, "Choose a Projects root.");
        AddRequiredIssue(issues, projectTypeComboBox.SelectedItem?.ToString(), "Choose a Project Type.");
        AddRequiredIssue(issues, projectNameTextBox.Text, "Enter the Project Name.");
        AddRequiredIssue(issues, projectCodeTextBox.Text, "Enter the Project Code.");

        if (string.IsNullOrWhiteSpace(projectScopeSummaryTextBox.Text) || projectScopeSummaryTextBox.Text.Trim().Length < 20)
        {
            issues.Add("Write a Project Scope Summary of at least 20 characters.");
        }

        return issues;
    }

    private List<string> ValidateCustomerInformationStep()
    {
        var issues = new List<string>();
        AddRequiredIssue(issues, customerNameTextBox.Text, "Enter the Customer Name.");
        return issues;
    }

    private List<string> ValidateMachineInformationStep()
    {
        var issues = new List<string>();
        AddRequiredIssue(issues, machineModelTextBox.Text, "Enter the Machine Model.");
        AddRequiredIssue(issues, machineSerialNumberTextBox.Text, "Enter the Machine Serial Number.");
        return issues;
    }

    private List<string> ValidateRolesStep()
    {
        var issues = new List<string>();
        AddRequiredIssue(issues, leadTestEngineerTextBox.Text, "Enter the Lead Test Engineer.");
        AddRequiredIssue(issues, releaseAuthorityTextBox.Text, "Enter the Release Authority.");
        AddRequiredIssue(issues, finalApprovalAuthorityTextBox.Text, "Enter the Final Approval Authority.");
        AddRequiredIssue(issues, createdByTextBox.Text, "Created By must not be empty.");
        return issues;
    }

    private static void AddRequiredIssue(List<string> issues, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(message);
        }
    }

    private static string StepTitle(int step)
    {
        return step switch
        {
            0 => "Project Information",
            1 => "Customer Information",
            2 => "Machine Information",
            3 => "Roles & Responsibilities",
            4 => "Review & Create",
            _ => string.Empty
        };
    }

    private static string StepPurpose(int step)
    {
        return step switch
        {
            0 => "Define the job, intent, and primary scope of the TestTrace contract.",
            1 => "Capture reporting-only customer and site context that can still be refined later.",
            2 => "Bind the project to the tested machine instance and capture machine context that locks when execution opens.",
            3 => "Separate FAT accountability from contract authorship and release control.",
            4 => "Review what freezes at project creation, what locks at execution, and what remains editable until release.",
            _ => string.Empty
        };
    }

    private void UpdateLiveSummary()
    {
        SetSummaryRows(
            ("Project Type", projectTypeComboBox.SelectedItem?.ToString()),
            ("Project Name", projectNameTextBox.Text),
            ("Project Code", projectCodeTextBox.Text),
            ("Customer Name", customerNameTextBox.Text),
            ("Machine", $"{ValueOrDash(machineModelTextBox.Text)} / {ValueOrDash(machineSerialNumberTextBox.Text)}"),
            ("Project Scope Summary", projectScopeSummaryTextBox.Text),
            ("Lead Test Engineer", leadTestEngineerTextBox.Text),
            ("Release Authority", releaseAuthorityTextBox.Text),
            ("Final Approval Authority", finalApprovalAuthorityTextBox.Text),
            ("Project Start Date", ProjectStartDateText()),
            ("Control Platform", controlPlatformTextBox.Text),
            ("Machine Role / Application", machineRoleApplicationTextBox.Text),
            ("Software Version", softwareVersionTextBox.Text),
            ("Site Contact Name", siteContactNameTextBox.Text),
            ("Customer Project Reference Name / Number", customerProjectReferenceTextBox.Text),
            ("Created By", createdByTextBox.Text),
            ("Projects root", projectsRootTextBox.Text));
    }

    private void SetSummaryRows(params (string Label, string? Value)[] rows)
    {
        summaryTable.SuspendLayout();
        summaryTable.Controls.Clear();
        summaryTable.RowStyles.Clear();
        summaryTable.RowCount = 0;

        for (var index = 0; index < rows.Length; index++)
        {
            summaryTable.RowCount++;
            summaryTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = rows[index].Label,
                AutoSize = true,
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                Margin = new Padding(0, 3, 10, 8)
            };
            var value = new Label
            {
                Text = ValueOrDash(rows[index].Value),
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                Margin = new Padding(0, 3, 0, 8)
            };

            summaryTable.Controls.Add(label, 0, index);
            summaryTable.Controls.Add(value, 1, index);
        }

        summaryTable.ResumeLayout();
    }

    private void WireSummaryUpdates(Control root)
    {
        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.TextChanged += (_, _) => UpdateLiveSummary();
                    break;
                case ComboBox comboBox:
                    comboBox.SelectedIndexChanged += (_, _) => UpdateLiveSummary();
                    comboBox.TextChanged += (_, _) => UpdateLiveSummary();
                    break;
                case DateTimePicker picker:
                    picker.ValueChanged += (_, _) => UpdateLiveSummary();
                    break;
            }

            WireSummaryUpdates(control);
        }
    }

    private void AddRootRow(TableLayoutPanel fields, int row)
    {
        EnsureRow(fields, row);
        fields.Controls.Add(CreateLabel("Projects root"), 0, row);
        projectsRootTextBox.Dock = DockStyle.Fill;
        projectsRootTextBox.Margin = new Padding(0, 0, 0, 10);
        fields.Controls.Add(projectsRootTextBox, 1, row);

        var browseButton = new Button { Text = "Browse", AutoSize = true, Margin = new Padding(8, 0, 0, 10) };
        browseButton.Click += (_, _) => BrowseForRoot();
        fields.Controls.Add(browseButton, 2, row);
    }

    private static void AddDateRow(TableLayoutPanel fields, int row, string label, DateTimePicker picker)
    {
        EnsureRow(fields, row);
        fields.Controls.Add(CreateLabel(label), 0, row);
        picker.Dock = DockStyle.Left;
        picker.Width = 240;
        picker.Format = DateTimePickerFormat.Long;
        picker.ShowCheckBox = true;
        picker.Checked = false;
        picker.Margin = new Padding(0, 0, 0, 10);
        fields.Controls.Add(picker, 1, row);
        fields.SetColumnSpan(picker, 2);
    }

    private static Panel CreateStepPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
    }

    private static TableLayoutPanel CreateFieldTable()
    {
        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        return fields;
    }

    private static void AddTextRow(TableLayoutPanel fields, int row, string label, TextBox textBox)
    {
        EnsureRow(fields, row);
        fields.Controls.Add(CreateLabel(label), 0, row);
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 0, 0, 10);
        fields.Controls.Add(textBox, 1, row);
        fields.SetColumnSpan(textBox, 2);
    }

    private static void AddMultilineRow(TableLayoutPanel fields, int row, string label, TextBox textBox, int height)
    {
        EnsureRow(fields, row, height);
        fields.Controls.Add(CreateLabel(label), 0, row);
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.Vertical;
        textBox.Margin = new Padding(0, 0, 0, 10);
        fields.Controls.Add(textBox, 1, row);
        fields.SetColumnSpan(textBox, 2);
    }

    private static void AddComboRow(TableLayoutPanel fields, int row, string label, ComboBox comboBox, string[] values)
    {
        EnsureRow(fields, row);
        fields.Controls.Add(CreateLabel(label), 0, row);
        comboBox.Dock = DockStyle.Left;
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Width = 260;
        comboBox.Margin = new Padding(0, 0, 0, 10);
        comboBox.Items.AddRange(values);
        fields.Controls.Add(comboBox, 1, row);
        fields.SetColumnSpan(comboBox, 2);
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 12, 10)
        };
    }

    private static Label CreateNoteLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            MaximumSize = new Size(740, 0),
            Margin = new Padding(0, 8, 0, 8)
        };
    }

    private static Label CreateReviewHeading(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 10)
        };
    }

    private static void EnsureRow(TableLayoutPanel fields, int row, int? height = null)
    {
        while (fields.RowCount <= row)
        {
            fields.RowCount++;
        }

        fields.RowStyles.Add(height.HasValue
            ? new RowStyle(SizeType.Absolute, height.Value)
            : new RowStyle(SizeType.AutoSize));
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
        }
    }

    private void BuildContract()
    {
        validationTextBox.Clear();

        var service = new BuildContractService(new JsonProjectRepository());
        var result = service.Build(CreateBuildRequest());

        if (!result.Succeeded)
        {
            validationTextBox.Text = BuildErrorText(result);
            return;
        }

        CreatedProjectFolderPath = result.ProjectFolderPath;
        MessageBox.Show(
            this,
            "Project created. Required contract fields are now frozen. Machine context stays editable until you open for execution. Reporting and contact details can still be updated until release.",
            "TestTrace",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
    }

    private BuildContractRequest CreateBuildRequest()
    {
        return new BuildContractRequest
        {
            ProjectsRootDirectory = projectsRootTextBox.Text,
            ProjectType = projectTypeComboBox.SelectedItem?.ToString() ?? string.Empty,
            ProjectName = projectNameTextBox.Text,
            ProjectCode = projectCodeTextBox.Text,
            MachineModel = machineModelTextBox.Text,
            MachineSerialNumber = machineSerialNumberTextBox.Text,
            CustomerName = customerNameTextBox.Text,
            ScopeNarrative = projectScopeSummaryTextBox.Text,
            LeadTestEngineer = leadTestEngineerTextBox.Text,
            ReleaseAuthority = releaseAuthorityTextBox.Text,
            FinalApprovalAuthority = finalApprovalAuthorityTextBox.Text,
            CreatedBy = createdByTextBox.Text,
            ReportingMetadata = new ReportingMetadata
            {
                ProjectStartDate = projectStartDatePicker.Checked
                    ? DateOnly.FromDateTime(projectStartDatePicker.Value.Date)
                    : null,
                CustomerAddress = TrimToNull(customerAddressTextBox.Text),
                CustomerCountry = TrimToNull(customerCountryTextBox.Text),
                CustomerProjectReference = TrimToNull(customerProjectReferenceTextBox.Text),
                SiteContactName = TrimToNull(siteContactNameTextBox.Text),
                SiteContactEmail = TrimToNull(siteContactEmailTextBox.Text),
                SiteContactPhone = TrimToNull(siteContactNumberTextBox.Text),
                MachineConfigurationSpecification = TrimToNull(machineConfigurationTextBox.Text),
                ControlPlatform = TrimToNull(controlPlatformTextBox.Text),
                MachineRoleApplication = TrimToNull(machineRoleApplicationTextBox.Text),
                SoftwareVersion = TrimToNull(softwareVersionTextBox.Text),
                LeadTestEngineerEmail = TrimToNull(leadTestEngineerEmailTextBox.Text),
                LeadTestEngineerPhone = TrimToNull(leadTestEngineerPhoneTextBox.Text)
            }
        };
    }

    private static string BuildErrorText(BuildContractResult result)
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

    private string ProjectStartDateText()
    {
        return projectStartDatePicker.Checked
            ? projectStartDatePicker.Value.Date.ToString("d MMM yyyy")
            : "-";
    }

    private static string ValueOrDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static string? TrimToNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string CurrentActor()
    {
        return string.IsNullOrWhiteSpace(Environment.UserName)
            ? "Local Operator"
            : Environment.UserName;
    }
}
