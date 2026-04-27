using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class AddComponentForm : Form
{
    private readonly ComboBox assetComboBox = new();
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
    private readonly Label helpLabel = new();
    private readonly List<Asset> existingAssets;

    public string AssetName => assetComboBox.Text.Trim();
    public string? AssetType => string.IsNullOrWhiteSpace(typeComboBox.Text) ? null : typeComboBox.Text.Trim();
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

    public string ComponentName => AssetName;
    public string? ComponentDescription => Notes;

    public AddComponentForm(string contextTitle, IEnumerable<Asset> availableAssets, string title = "Add Asset")
    {
        existingAssets = availableAssets
            .OrderBy(asset => asset.Name)
            .ToList();

        Text = title;
        MinimumSize = new Size(720, 520);
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        InitializeLayout(contextTitle, title);
        AppTheme.Apply(this);
    }

    private void InitializeLayout(string contextTitle, string title)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 10,
            Padding = new Padding(18)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
            Text = contextTitle,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 14)
        };
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);

        AddLabel(layout, "Asset", 1);
        assetComboBox.Dock = DockStyle.Fill;
        assetComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        assetComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        assetComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
        assetComboBox.Margin = new Padding(0, 0, 0, 8);
        assetComboBox.DisplayMember = nameof(Asset.Name);
        assetComboBox.Items.AddRange(existingAssets.Cast<object>().ToArray());
        assetComboBox.SelectedIndexChanged += (_, _) => PopulateSelectedAsset();
        assetComboBox.TextChanged += (_, _) =>
        {
            UpdateHelpText();
            UpdateTypeSpecificFields();
        };
        layout.Controls.Add(assetComboBox, 1, 1);

        helpLabel.AutoSize = true;
        helpLabel.Margin = new Padding(0, 0, 0, 12);
        layout.Controls.Add(helpLabel, 1, 2);

        AddLabel(layout, "Type", 3);
        typeComboBox.Dock = DockStyle.Fill;
        typeComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        typeComboBox.Items.AddRange(new object[] { "Component", "Motor", "Gearbox", "Feeder", "Sensor", "Valve", "Panel", "Software" });
        typeComboBox.Text = title.Contains("Sub", StringComparison.OrdinalIgnoreCase) ? "Sub-component" : "Component";
        typeComboBox.Margin = new Padding(0, 0, 0, 8);
        typeComboBox.TextChanged += (_, _) => UpdateTypeSpecificFields();
        typeComboBox.SelectedIndexChanged += (_, _) => UpdateTypeSpecificFields();
        layout.Controls.Add(typeComboBox, 1, 3);

        AddTextRow(layout, 4, "Manufacturer", manufacturerTextBox);
        AddTextRow(layout, 5, "Model", modelTextBox);
        AddTextRow(layout, 6, "Serial", serialTextBox);

        ConfigureMotorDataPlateGroup();
        layout.Controls.Add(motorDataPlateGroup, 0, 7);
        layout.SetColumnSpan(motorDataPlateGroup, 2);

        AddLabel(layout, "Notes", 8);
        notesTextBox.Dock = DockStyle.Fill;
        notesTextBox.Multiline = true;
        notesTextBox.ScrollBars = ScrollBars.Vertical;
        notesTextBox.Margin = new Padding(0, 0, 0, 10);
        layout.Controls.Add(notesTextBox, 1, 8);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var addButton = new Button { Text = title, AutoSize = true };
        addButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(addButton);
        actions.Controls.Add(cancelButton);
        layout.Controls.Add(actions, 0, 9);
        layout.SetColumnSpan(actions, 2);

        UpdateHelpText();
        UpdateTypeSpecificFields();
        Controls.Add(layout);
    }

    private void PopulateSelectedAsset()
    {
        if (assetComboBox.SelectedItem is not Asset asset)
        {
            return;
        }

        typeComboBox.Text = asset.Type;
        manufacturerTextBox.Text = asset.Manufacturer ?? string.Empty;
        modelTextBox.Text = asset.Model ?? string.Empty;
        serialTextBox.Text = asset.SerialNumber ?? string.Empty;
        var parsed = MotorDataPlateSerializer.Parse(asset.Notes);
        notesTextBox.Text = parsed.Notes ?? string.Empty;
        ratedVoltageTextBox.Text = parsed.DataPlate.RatedVoltage ?? string.Empty;
        ratedCurrentTextBox.Text = parsed.DataPlate.RatedCurrent ?? string.Empty;
        powerRatingTextBox.Text = parsed.DataPlate.PowerRating ?? string.Empty;
        frequencyTextBox.Text = parsed.DataPlate.Frequency ?? string.Empty;
        speedRpmTextBox.Text = parsed.DataPlate.SpeedRpm ?? string.Empty;
        phaseTextBox.Text = parsed.DataPlate.Phase ?? string.Empty;
        UpdateHelpText();
        UpdateTypeSpecificFields();
    }

    private void UpdateHelpText()
    {
        var enteredName = assetComboBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(enteredName))
        {
            helpLabel.Text = existingAssets.Count == 0
                ? "Type a new asset name."
                : "Choose an existing asset or type a new one.";
            return;
        }

        var existing = existingAssets.Any(asset =>
            string.Equals(asset.Name, enteredName, StringComparison.OrdinalIgnoreCase));
        helpLabel.Text = existing
            ? "Existing global asset will be linked into this section."
            : "New global asset will be created and linked into this section.";
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        AddMotorTextRow(layout, 0, "Rated Voltage", ratedVoltageTextBox, "Rated Current", ratedCurrentTextBox);
        AddMotorTextRow(layout, 1, "Power Rating", powerRatingTextBox, "Frequency", frequencyTextBox);
        AddMotorTextRow(layout, 2, "Speed (RPM)", speedRpmTextBox, "Phase", phaseTextBox);

        var note = new Label
        {
            Text = "Use these when the asset is a motor and you want the standard nameplate details captured at creation time.",
            AutoSize = true,
            MaximumSize = new Size(600, 0),
            Margin = new Padding(0, 8, 0, 0)
        };
        layout.Controls.Add(note, 0, 3);
        layout.SetColumnSpan(note, 4);

        motorDataPlateGroup.Controls.Add(layout);
    }

    private void UpdateTypeSpecificFields()
    {
        motorDataPlateGroup.Visible =
            typeComboBox.Text.Contains("motor", StringComparison.OrdinalIgnoreCase) ||
            assetComboBox.Text.Contains("motor", StringComparison.OrdinalIgnoreCase);
    }

    private void Accept()
    {
        if (string.IsNullOrWhiteSpace(assetComboBox.Text))
        {
            MessageBox.Show(this, "Asset name is required.", "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private static void AddTextRow(TableLayoutPanel layout, int row, string label, TextBox textBox)
    {
        AddLabel(layout, label, row);
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(textBox, 1, row);
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

        AddLabel(layout, rightLabel, row, 2);
        rightTextBox.Dock = DockStyle.Fill;
        rightTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(rightTextBox, 3, row);
    }

    private static void AddLabel(TableLayoutPanel layout, string text, int row, int column = 0)
    {
        layout.Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 12, 10)
        }, column, row);
    }

    private static string? TrimToNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
