using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class EditProjectMetadataForm : Form
{
    private readonly DateTimePicker projectStartDatePicker = new();
    private readonly TextBox customerAddressTextBox = new();
    private readonly TextBox customerCountryTextBox = new();
    private readonly TextBox siteContactNameTextBox = new();
    private readonly TextBox siteContactEmailTextBox = new();
    private readonly TextBox siteContactNumberTextBox = new();
    private readonly TextBox customerProjectReferenceTextBox = new();
    private readonly TextBox machineConfigurationTextBox = new();
    private readonly TextBox controlPlatformTextBox = new();
    private readonly TextBox machineRoleApplicationTextBox = new();
    private readonly TextBox softwareVersionTextBox = new();
    private readonly TextBox leadTestEngineerEmailTextBox = new();
    private readonly TextBox leadTestEngineerPhoneTextBox = new();

    public ReportingMetadata Metadata => new()
    {
        ProjectStartDate = projectStartDatePicker.Checked
            ? DateOnly.FromDateTime(projectStartDatePicker.Value.Date)
            : null,
        CustomerAddress = TrimToNull(customerAddressTextBox.Text),
        CustomerCountry = TrimToNull(customerCountryTextBox.Text),
        SiteContactName = TrimToNull(siteContactNameTextBox.Text),
        SiteContactEmail = TrimToNull(siteContactEmailTextBox.Text),
        SiteContactPhone = TrimToNull(siteContactNumberTextBox.Text),
        CustomerProjectReference = TrimToNull(customerProjectReferenceTextBox.Text),
        MachineConfigurationSpecification = TrimToNull(machineConfigurationTextBox.Text),
        ControlPlatform = TrimToNull(controlPlatformTextBox.Text),
        MachineRoleApplication = TrimToNull(machineRoleApplicationTextBox.Text),
        SoftwareVersion = TrimToNull(softwareVersionTextBox.Text),
        LeadTestEngineerEmail = TrimToNull(leadTestEngineerEmailTextBox.Text),
        LeadTestEngineerPhone = TrimToNull(leadTestEngineerPhoneTextBox.Text)
    };

    public EditProjectMetadataForm(string projectLabel, ReportingMetadata metadata, ProjectState state)
    {
        Text = "Edit Project Metadata";
        MinimumSize = new Size(860, 760);
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;

        InitializeLayout(projectLabel, metadata, state);
        AppTheme.Apply(this);

        if (state == ProjectState.Executable)
        {
            LockMachineContext();
        }
    }

    private void InitializeLayout(string projectLabel, ReportingMetadata metadata, ProjectState state)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(18)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 14)
        };

        var title = new Label
        {
            Text = projectLabel,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 4)
        };
        var subtitle = new Label
        {
            Text = MetadataSubtitle(state),
            AutoSize = true,
            MaximumSize = new Size(760, 0),
            Margin = new Padding(0)
        };

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(subtitle, 0, 1);
        layout.Controls.Add(header, 0, 0);

        var scroller = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var row = 0;
        AddSectionHeading(fields, ref row, "Project Information");
        AddDateRow(fields, ref row, "Project Start Date", projectStartDatePicker, metadata.ProjectStartDate);

        AddSectionHeading(fields, ref row, "Customer Information");
        AddMultilineRow(fields, ref row, "Customer Address", customerAddressTextBox, metadata.CustomerAddress, 108);
        AddTextRow(fields, ref row, "Customer Country", customerCountryTextBox, metadata.CustomerCountry);
        AddTextRow(fields, ref row, "Site Contact Name", siteContactNameTextBox, metadata.SiteContactName);
        AddTextRow(fields, ref row, "Site Contact Email", siteContactEmailTextBox, metadata.SiteContactEmail);
        AddTextRow(fields, ref row, "Site Contact Number", siteContactNumberTextBox, metadata.SiteContactPhone);
        AddTextRow(fields, ref row, "Customer Project Reference Name / Number", customerProjectReferenceTextBox, metadata.CustomerProjectReference);

        AddSectionHeading(fields, ref row, "Machine Information");
        AddMultilineRow(fields, ref row, "Machine Configuration / Specification", machineConfigurationTextBox, metadata.MachineConfigurationSpecification, 118);
        AddTextRow(fields, ref row, "Control Platform", controlPlatformTextBox, metadata.ControlPlatform);
        AddTextRow(fields, ref row, "Machine Role / Application", machineRoleApplicationTextBox, metadata.MachineRoleApplication);
        AddTextRow(fields, ref row, "Software Version", softwareVersionTextBox, metadata.SoftwareVersion);

        if (state == ProjectState.Executable)
        {
            AddSectionNote(
                fields,
                ref row,
                "Machine context is locked because the project is already open for execution. These fields remain visible here for reference only.");
        }

        AddSectionHeading(fields, ref row, "Roles / Responsibilities");
        AddTextRow(fields, ref row, "Lead Test Engineer Email", leadTestEngineerEmailTextBox, metadata.LeadTestEngineerEmail);
        AddTextRow(fields, ref row, "Lead Test Engineer Phone", leadTestEngineerPhoneTextBox, metadata.LeadTestEngineerPhone);

        scroller.Controls.Add(fields);
        layout.Controls.Add(scroller, 0, 1);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft
        };
        var saveButton = new Button { Text = "Save Metadata", AutoSize = true };
        saveButton.Click += (_, _) => DialogResult = DialogResult.OK;
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(saveButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 2);

        Controls.Add(layout);
    }

    private static void AddSectionHeading(TableLayoutPanel layout, ref int row, string text)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var label = new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, row == 0 ? 0 : 16, 0, 10)
        };
        layout.Controls.Add(label, 0, row);
        layout.SetColumnSpan(label, 2);
        row++;
    }

    private static void AddSectionNote(TableLayoutPanel layout, ref int row, string text)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var label = new Label
        {
            Text = text,
            AutoSize = true,
            MaximumSize = new Size(720, 0),
            Margin = new Padding(0, 0, 0, 10)
        };
        layout.Controls.Add(label, 0, row);
        layout.SetColumnSpan(label, 2);
        row++;
    }

    private static void AddTextRow(TableLayoutPanel layout, ref int row, string label, TextBox textBox, string? value)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(CreateLabel(label), 0, row);
        textBox.Text = value ?? string.Empty;
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(textBox, 1, row);
        row++;
    }

    private static void AddMultilineRow(TableLayoutPanel layout, ref int row, string label, TextBox textBox, string? value, int height)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        layout.Controls.Add(CreateLabel(label), 0, row);
        textBox.Text = value ?? string.Empty;
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.Vertical;
        textBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(textBox, 1, row);
        row++;
    }

    private static void AddDateRow(TableLayoutPanel layout, ref int row, string label, DateTimePicker picker, DateOnly? value)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(CreateLabel(label), 0, row);
        picker.Dock = DockStyle.Left;
        picker.Width = 260;
        picker.Format = DateTimePickerFormat.Long;
        picker.ShowCheckBox = true;
        picker.Checked = value is not null;
        picker.Value = value?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
        picker.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(picker, 1, row);
        row++;
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

    private static string? TrimToNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string MetadataSubtitle(ProjectState state)
    {
        return state switch
        {
            ProjectState.DraftContractBuilt =>
                "Edit project metadata that supports reporting and delivery context. Contract identity stays read-only here, but machine context can still be refined before execution begins.",
            ProjectState.Executable =>
                "Contract identity stays read-only here. Machine context is now locked because execution has started. Reporting and contact details can still be updated until release.",
            _ =>
                "Edit project metadata that supports reporting and delivery context. Contract-root identity stays read-only in the workspace."
        };
    }

    private void LockMachineContext()
    {
        SetEditable(machineConfigurationTextBox, false);
        SetEditable(controlPlatformTextBox, false);
        SetEditable(machineRoleApplicationTextBox, false);
        SetEditable(softwareVersionTextBox, false);
    }

    private static void SetEditable(TextBox textBox, bool editable)
    {
        textBox.ReadOnly = !editable;
        if (!editable)
        {
            textBox.BackColor = AppTheme.Current.InputReadOnlyBackground;
            textBox.ForeColor = AppTheme.Current.InputForeground;
        }
    }
}
