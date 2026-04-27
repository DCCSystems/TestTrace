using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class AssetMetadataForm : Form
{
    private readonly ComboBox typeComboBox = new();
    private readonly TextBox manufacturerTextBox = new();
    private readonly TextBox modelTextBox = new();
    private readonly TextBox serialTextBox = new();
    private readonly TextBox notesTextBox = new();
    private readonly GroupBox motorDataPlateGroup = new();
    private readonly TextBox ratedVoltageTextBox = new();
    private readonly TextBox ratedCurrentTextBox = new();
    private readonly TextBox powerRatingTextBox = new();
    private readonly TextBox frequencyTextBox = new();
    private readonly TextBox speedRpmTextBox = new();
    private readonly TextBox phaseTextBox = new();

    public string? AssetType => TrimToNull(typeComboBox.Text);
    public string? Manufacturer => TrimToNull(manufacturerTextBox.Text);
    public string? Model => TrimToNull(modelTextBox.Text);
    public string? SerialNumber => TrimToNull(serialTextBox.Text);
    public string? Notes => MotorDataPlateSerializer.Compose(
        TrimToNull(notesTextBox.Text),
        new MotorDataPlateMetadata
        {
            RatedVoltage = TrimToNull(ratedVoltageTextBox.Text),
            RatedCurrent = TrimToNull(ratedCurrentTextBox.Text),
            PowerRating = TrimToNull(powerRatingTextBox.Text),
            Frequency = TrimToNull(frequencyTextBox.Text),
            SpeedRpm = TrimToNull(speedRpmTextBox.Text),
            Phase = TrimToNull(phaseTextBox.Text)
        });

    public AssetMetadataForm(Asset asset)
    {
        Text = "Edit Asset Metadata";
        MinimumSize = new Size(620, 430);
        StartPosition = FormStartPosition.CenterParent;
        InitializeLayout(asset);
        AppTheme.Apply(this);
    }

    private void InitializeLayout(Asset asset)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(18)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label
        {
            Text = asset.Name,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 14)
        };
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);

        AddLabel(layout, "Type", 1);
        typeComboBox.Dock = DockStyle.Fill;
        typeComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        typeComboBox.Items.AddRange(new object[] { "Component", "Sub-component", "Motor", "Gearbox", "Feeder", "Sensor", "Valve", "Panel", "Software" });
        typeComboBox.Text = asset.Type;
        typeComboBox.Margin = new Padding(0, 0, 0, 8);
        typeComboBox.TextChanged += (_, _) => motorDataPlateGroup.Visible = typeComboBox.Text.Contains("motor", StringComparison.OrdinalIgnoreCase);
        typeComboBox.SelectedIndexChanged += (_, _) => motorDataPlateGroup.Visible = typeComboBox.Text.Contains("motor", StringComparison.OrdinalIgnoreCase);
        layout.Controls.Add(typeComboBox, 1, 1);

        AddTextRow(layout, 2, "Manufacturer", manufacturerTextBox, asset.Manufacturer);
        AddTextRow(layout, 3, "Model", modelTextBox, asset.Model);
        AddTextRow(layout, 4, "Serial", serialTextBox, asset.SerialNumber);

        ConfigureMotorDataPlateGroup();
        layout.Controls.Add(motorDataPlateGroup, 0, 5);
        layout.SetColumnSpan(motorDataPlateGroup, 2);

        AddLabel(layout, "Notes", 6);
        var parsed = MotorDataPlateSerializer.Parse(asset.Notes);
        notesTextBox.Text = parsed.Notes ?? string.Empty;
        ratedVoltageTextBox.Text = parsed.DataPlate.RatedVoltage ?? string.Empty;
        ratedCurrentTextBox.Text = parsed.DataPlate.RatedCurrent ?? string.Empty;
        powerRatingTextBox.Text = parsed.DataPlate.PowerRating ?? string.Empty;
        frequencyTextBox.Text = parsed.DataPlate.Frequency ?? string.Empty;
        speedRpmTextBox.Text = parsed.DataPlate.SpeedRpm ?? string.Empty;
        phaseTextBox.Text = parsed.DataPlate.Phase ?? string.Empty;
        notesTextBox.Dock = DockStyle.Fill;
        notesTextBox.Multiline = true;
        notesTextBox.ScrollBars = ScrollBars.Vertical;
        notesTextBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(notesTextBox, 1, 6);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var saveButton = new Button { Text = "Save Metadata", AutoSize = true };
        saveButton.Click += (_, _) => DialogResult = DialogResult.OK;
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(saveButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 7);
        layout.SetColumnSpan(actions, 2);

        motorDataPlateGroup.Visible = asset.Type.Contains("motor", StringComparison.OrdinalIgnoreCase);

        Controls.Add(layout);
    }

    private void ConfigureMotorDataPlateGroup()
    {
        motorDataPlateGroup.Text = "Motor Data Plate (optional)";
        motorDataPlateGroup.Dock = DockStyle.Top;
        motorDataPlateGroup.AutoSize = true;
        motorDataPlateGroup.Padding = new Padding(10);
        motorDataPlateGroup.Margin = new Padding(0, 0, 0, 10);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        AddMotorTextRow(layout, 0, "Rated Voltage", ratedVoltageTextBox, "Rated Current", ratedCurrentTextBox);
        AddMotorTextRow(layout, 1, "Power Rating", powerRatingTextBox, "Frequency", frequencyTextBox);
        AddMotorTextRow(layout, 2, "Speed (RPM)", speedRpmTextBox, "Phase", phaseTextBox);

        motorDataPlateGroup.Controls.Add(layout);
    }

    private static void AddTextRow(TableLayoutPanel layout, int row, string label, TextBox textBox, string? value)
    {
        AddLabel(layout, label, row);
        textBox.Text = value ?? string.Empty;
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(textBox, 1, row);
    }

    private static void AddLabel(TableLayoutPanel layout, string text, int row)
    {
        layout.Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 12, 10)
        }, 0, row);
    }

    private static void AddMotorTextRow(
        TableLayoutPanel layout,
        int row,
        string leftLabel,
        TextBox leftTextBox,
        string rightLabel,
        TextBox rightTextBox)
    {
        AddLabel(layout, leftLabel, row);
        leftTextBox.Dock = DockStyle.Fill;
        leftTextBox.Margin = new Padding(0, 0, 10, 8);
        layout.Controls.Add(leftTextBox, 1, row);

        layout.Controls.Add(new Label
        {
            Text = rightLabel,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 12, 10)
        }, 2, row);
        rightTextBox.Dock = DockStyle.Fill;
        rightTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(rightTextBox, 3, row);
    }

    private static string? TrimToNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
