using System.Globalization;
using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class AddTestItemForm : Form
{
    private readonly ComboBox templateComboBox = new();
    private readonly TextBox referenceTextBox = new();
    private readonly TextBox titleTextBox = new();
    private readonly TextBox descriptionTextBox = new();
    private readonly TextBox expectedOutcomeTextBox = new();
    private readonly RadioButton manualPassRadio = new();
    private readonly RadioButton numericPassRadio = new();
    private readonly RadioButton booleanPassRadio = new();
    private readonly Label targetValueLabel = new();
    private readonly Label toleranceLabel = new();
    private readonly Label unitLabel = new();
    private readonly Label booleanGuidanceLabel = new();
    private readonly TextBox targetValueTextBox = new();
    private readonly TextBox toleranceTextBox = new();
    private readonly TextBox unitTextBox = new();
    private readonly CheckBox photoRequiredCheckBox = new();
    private readonly CheckBox measurementRequiredCheckBox = new();
    private readonly CheckBox signatureRequiredCheckBox = new();
    private readonly CheckBox fileUploadRequiredCheckBox = new();
    private readonly CheckBox commentOnFailCheckBox = new();
    private readonly CheckBox commentAlwaysCheckBox = new();
    private readonly CheckBox blockProgressionCheckBox = new();
    private readonly CheckBox allowOverrideCheckBox = new();
    private readonly CheckBox requiresWitnessCheckBox = new();
    private readonly CheckBox useStructuredInputsCheckBox = new();
    private readonly Panel inputsPanel = new();
    private readonly DataGridView inputsGrid = new();
    private readonly Button addInputButton = new();
    private readonly Button removeInputButton = new();
    private readonly TextBox summaryTextBox = new();
    private readonly TextBox validationTextBox = new();

    public string TestReference => referenceTextBox.Text.Trim();
    public string TestTitle => titleTextBox.Text.Trim();
    public string? TestDescription => string.IsNullOrWhiteSpace(descriptionTextBox.Text) ? null : descriptionTextBox.Text.Trim();
    public string? ExpectedOutcome => string.IsNullOrWhiteSpace(expectedOutcomeTextBox.Text) ? null : expectedOutcomeTextBox.Text.Trim();
    public AcceptanceCriteria AcceptanceCriteria => CreateAcceptanceCriteria();
    public EvidenceRequirements EvidenceRequirements => TestTrace_V1.Domain.EvidenceRequirements.Create(
        photoRequiredCheckBox.Checked,
        measurementRequiredCheckBox.Checked,
        signatureRequiredCheckBox.Checked,
        fileUploadRequiredCheckBox.Checked,
        commentOnFailCheckBox.Checked,
        commentAlwaysCheckBox.Checked);
    public TestBehaviourRules BehaviourRules => TestTrace_V1.Domain.TestBehaviourRules.Create(
        blockProgressionCheckBox.Checked,
        allowOverrideCheckBox.Checked,
        requiresWitnessCheckBox.Checked);
    public IReadOnlyList<TestInput> TestInputs => BuildTestInputs();

    public AddTestItemForm(string assetName, string defaultReference)
    {
        Text = "Add Test Item";
        MinimumSize = new Size(760, 700);
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        referenceTextBox.Text = defaultReference;

                InitializeLayout(assetName);
        AppTheme.Apply(this);
        WireSummaryUpdates(this);

        templateComboBox.SelectedItem = "Custom";
        manualPassRadio.Checked = true;
        blockProgressionCheckBox.Checked = true;
        UpdateAcceptanceControls();
        UpdateStructuredInputsVisibility();
        UpdateSummary();
    }

    private void InitializeLayout(string assetName)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(16)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label
        {
            Text = $"Asset: {assetName}",
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12)
        };
        layout.Controls.Add(heading, 0, 0);

        var scroller = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 5
        };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.Controls.Add(CreateDefinitionGroup(), 0, 0);
        content.Controls.Add(CreateAcceptanceGroup(), 0, 1);
        content.Controls.Add(CreateEvidenceGroup(), 0, 2);
        content.Controls.Add(CreateBehaviourGroup(), 0, 3);
        content.Controls.Add(CreateInputsGroup(), 0, 4);
        scroller.Controls.Add(content);
        layout.Controls.Add(scroller, 0, 1);

        summaryTextBox.Dock = DockStyle.Fill;
        summaryTextBox.Multiline = true;
        summaryTextBox.ReadOnly = true;
        summaryTextBox.BackColor = AppTheme.Current.InputReadOnlyBackground;
        summaryTextBox.ScrollBars = ScrollBars.Vertical;
        summaryTextBox.Margin = new Padding(0, 10, 0, 8);
        layout.Controls.Add(summaryTextBox, 0, 2);

        validationTextBox.Dock = DockStyle.Fill;
        validationTextBox.Multiline = true;
        validationTextBox.ReadOnly = true;
        validationTextBox.BackColor = AppTheme.Current.InputReadOnlyBackground;
        validationTextBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(validationTextBox, 0, 3);

        layout.Controls.Add(CreateActions(), 0, 4);
        Controls.Add(layout);
    }

    private Control CreateDefinitionGroup()
    {
        var group = CreateGroup("1. Test Definition");
        var table = CreateFieldTable();
        AddComboRow(table, 0, "Test type", templateComboBox, ["Custom", "Visual Check", "Measurement", "Functional Test"]);
        AddTextRow(table, 1, "Reference", referenceTextBox);
        AddTextRow(table, 2, "Title", titleTextBox);
        AddMultilineRow(table, 3, "Description", descriptionTextBox, 76);
        AddMultilineRow(table, 4, "Expected result", expectedOutcomeTextBox, 76);
        group.Controls.Add(table);
        return group;
    }

    private Control CreateAcceptanceGroup()
    {
        var group = CreateGroup("2. Acceptance Criteria");
        var table = CreateFieldTable();

        manualPassRadio.Text = "Manual confirmation only";
        numericPassRadio.Text = "Numeric value within range";
        booleanPassRadio.Text = "Boolean condition";
        manualPassRadio.AutoSize = true;
        numericPassRadio.AutoSize = true;
        booleanPassRadio.AutoSize = true;

        var radios = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            Margin = new Padding(0, 2, 0, 8)
        };
        radios.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        radios.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        radios.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        radios.Controls.Add(manualPassRadio, 0, 0);
        radios.Controls.Add(numericPassRadio, 1, 0);
        radios.Controls.Add(booleanPassRadio, 2, 0);

        table.Controls.Add(CreateLabel("Pass condition"), 0, 0);
        table.Controls.Add(radios, 1, 0);
        AddLabeledControlRow(table, 1, "Target value", targetValueLabel, targetValueTextBox);
        AddLabeledControlRow(table, 2, "Tolerance +/-", toleranceLabel, toleranceTextBox);
        AddLabeledControlRow(table, 3, "Unit", unitLabel, unitTextBox);

        booleanGuidanceLabel.Text = "Use this when the expected answer is true/false, correct/incorrect, present/missing, or enabled/disabled.";
        booleanGuidanceLabel.AutoSize = true;
        booleanGuidanceLabel.MaximumSize = new Size(560, 0);
        booleanGuidanceLabel.Margin = new Padding(0, 4, 0, 8);
        table.Controls.Add(booleanGuidanceLabel, 0, 4);
        table.SetColumnSpan(booleanGuidanceLabel, 2);
        group.Controls.Add(table);
        return group;
    }

    private Control CreateEvidenceGroup()
    {
        var group = CreateGroup("3. Evidence Requirements");
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            Padding = new Padding(12, 18, 12, 10)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

        ConfigureCheckBox(photoRequiredCheckBox, "Photo");
        ConfigureCheckBox(measurementRequiredCheckBox, "Measurement");
        ConfigureCheckBox(signatureRequiredCheckBox, "Signature");
        ConfigureCheckBox(fileUploadRequiredCheckBox, "File Upload");
        ConfigureCheckBox(commentOnFailCheckBox, "Observation required on fail");
        ConfigureCheckBox(commentAlwaysCheckBox, "Observation always required");

        AddCheck(grid, photoRequiredCheckBox, 0, 0);
        AddCheck(grid, measurementRequiredCheckBox, 1, 0);
        AddCheck(grid, signatureRequiredCheckBox, 2, 0);
        AddCheck(grid, fileUploadRequiredCheckBox, 0, 1);
        AddCheck(grid, commentOnFailCheckBox, 1, 1);
        AddCheck(grid, commentAlwaysCheckBox, 2, 1);

        var note = new Label
        {
            Text = "Observation always required means the operator must record an observation whether the test passes or fails.",
            AutoSize = true,
            MaximumSize = new Size(720, 0),
            Margin = new Padding(12, 2, 12, 0)
        };
        group.Controls.Add(grid);
        group.Controls.Add(note);
        return group;
    }

    private Control CreateBehaviourGroup()
    {
        var group = CreateGroup("4. Behaviour");
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(12, 18, 12, 10)
        };
        grid.ColumnCount = 3;
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

        ConfigureCheckBox(blockProgressionCheckBox, "Block progression if failed");
        ConfigureCheckBox(allowOverrideCheckBox, "Allow override with reason");
        ConfigureCheckBox(requiresWitnessCheckBox, "Witness required");

        AddCheck(grid, blockProgressionCheckBox, 0, 0);
        AddCheck(grid, allowOverrideCheckBox, 1, 0);
        AddCheck(grid, requiresWitnessCheckBox, 2, 0);
        group.Controls.Add(grid);
        return group;
    }

    private Control CreateInputsGroup()
    {
        var group = CreateGroup("5. Structured Inputs");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12, 20, 12, 12)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        useStructuredInputsCheckBox.Text = "Use structured inputs for this test";
        useStructuredInputsCheckBox.AutoSize = true;
        useStructuredInputsCheckBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(useStructuredInputsCheckBox, 0, 0);

        inputsPanel.Dock = DockStyle.Top;
        inputsPanel.AutoSize = true;
        inputsPanel.Visible = false;

        var inputsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 3
        };
        inputsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inputsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inputsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));

        var note = new Label
        {
            Text = "Use inputs for structured capture such as voltages, currents, directions, or observations. These values are captured during execution with the result record.",
            AutoSize = true,
            MaximumSize = new Size(720, 0),
            Margin = new Padding(0, 0, 0, 8)
        };
        inputsLayout.Controls.Add(note, 0, 0);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 0, 0, 8)
        };
        addInputButton.Text = "Add Input";
        addInputButton.AutoSize = true;
        addInputButton.Click += (_, _) => AddInputRow();
        removeInputButton.Text = "Remove Selected";
        removeInputButton.AutoSize = true;
        removeInputButton.Margin = new Padding(8, 0, 0, 0);
        removeInputButton.Click += (_, _) => RemoveSelectedInputRows();
        actions.Controls.Add(addInputButton);
        actions.Controls.Add(removeInputButton);
        inputsLayout.Controls.Add(actions, 0, 1);

        ConfigureInputsGrid();
        inputsLayout.Controls.Add(inputsGrid, 0, 2);

        inputsPanel.Controls.Add(inputsLayout);
        layout.Controls.Add(inputsPanel, 0, 1);

        group.Controls.Add(layout);
        return group;
    }

    private Control CreateActions()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var addButton = new Button { Text = "Add Test Item", AutoSize = true };
        addButton.Click += (_, _) => Accept();
        var cancelButton = new Button { Text = "Cancel", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        actions.Controls.Add(addButton);
        actions.Controls.Add(cancelButton);
        return actions;
    }

    private AcceptanceCriteria CreateAcceptanceCriteria()
    {
        if (numericPassRadio.Checked)
        {
            var target = ParseDecimal(targetValueTextBox.Text, "Target value");
            var tolerance = ParseDecimal(toleranceTextBox.Text, "Tolerance");
            return TestTrace_V1.Domain.AcceptanceCriteria.NumericRange(target, tolerance, unitTextBox.Text);
        }

        return booleanPassRadio.Checked
            ? TestTrace_V1.Domain.AcceptanceCriteria.BooleanCondition()
            : TestTrace_V1.Domain.AcceptanceCriteria.ManualConfirmation();
    }

    private void Accept()
    {
        var issues = ValidateInput();
        if (issues.Count > 0)
        {
            validationTextBox.Text = string.Join(Environment.NewLine, issues);
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private List<string> ValidateInput()
    {
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(referenceTextBox.Text))
        {
            issues.Add("Test reference is required.");
        }

        if (string.IsNullOrWhiteSpace(titleTextBox.Text))
        {
            issues.Add("Test title is required.");
        }

        if (numericPassRadio.Checked)
        {
            TryParseDecimal(targetValueTextBox.Text, "Target value", issues, out _);
            if (TryParseDecimal(toleranceTextBox.Text, "Tolerance", issues, out var tolerance) && tolerance < 0)
            {
                issues.Add("Tolerance cannot be negative.");
            }
        }

        ValidateTestInputs(issues);
        return issues;
    }

    private void ApplySelectedTemplate()
    {
        switch (templateComboBox.SelectedItem?.ToString())
        {
            case "Visual Check":
                manualPassRadio.Checked = true;
                photoRequiredCheckBox.Checked = true;
                measurementRequiredCheckBox.Checked = false;
                signatureRequiredCheckBox.Checked = false;
                fileUploadRequiredCheckBox.Checked = false;
                commentOnFailCheckBox.Checked = true;
                commentAlwaysCheckBox.Checked = false;
                blockProgressionCheckBox.Checked = true;
                allowOverrideCheckBox.Checked = false;
                requiresWitnessCheckBox.Checked = false;
                useStructuredInputsCheckBox.Checked = false;
                break;

            case "Measurement":
                numericPassRadio.Checked = true;
                photoRequiredCheckBox.Checked = false;
                measurementRequiredCheckBox.Checked = true;
                signatureRequiredCheckBox.Checked = false;
                fileUploadRequiredCheckBox.Checked = false;
                commentOnFailCheckBox.Checked = true;
                commentAlwaysCheckBox.Checked = false;
                blockProgressionCheckBox.Checked = true;
                allowOverrideCheckBox.Checked = false;
                requiresWitnessCheckBox.Checked = false;
                useStructuredInputsCheckBox.Checked = true;
                AddTemplateInputIfEmpty("Measured value", TestInputType.Numeric);
                break;

            case "Functional Test":
                booleanPassRadio.Checked = true;
                photoRequiredCheckBox.Checked = false;
                measurementRequiredCheckBox.Checked = false;
                signatureRequiredCheckBox.Checked = false;
                fileUploadRequiredCheckBox.Checked = true;
                commentOnFailCheckBox.Checked = true;
                commentAlwaysCheckBox.Checked = false;
                blockProgressionCheckBox.Checked = true;
                allowOverrideCheckBox.Checked = false;
                requiresWitnessCheckBox.Checked = false;
                useStructuredInputsCheckBox.Checked = false;
                break;
        }

        UpdateAcceptanceControls();
        UpdateStructuredInputsVisibility();
        UpdateSummary();
    }

    private void UpdateAcceptanceControls()
    {
        var numeric = numericPassRadio.Checked;
        targetValueLabel.Visible = numeric;
        targetValueTextBox.Visible = numeric;
        toleranceLabel.Visible = numeric;
        toleranceTextBox.Visible = numeric;
        unitLabel.Visible = numeric;
        unitTextBox.Visible = numeric;
        booleanGuidanceLabel.Visible = booleanPassRadio.Checked;
    }

    private void UpdateStructuredInputsVisibility()
    {
        inputsPanel.Visible = useStructuredInputsCheckBox.Checked;
        if (useStructuredInputsCheckBox.Checked && inputsGrid.Rows.Count == 0)
        {
            AddInputRow();
        }
    }

    private void UpdateSummary()
    {
        var lines = new List<string>
        {
            "Pass condition",
            "  " + AcceptanceSummary(),
            "",
            "Evidence",
            "  " + EvidenceRequirements.Describe(),
            "",
            "Behaviour",
            "  " + BehaviourRules.Describe(),
            "",
            "Structured inputs",
            "  " + InputSummary()
        };

        if (numericPassRadio.Checked && (!TryParseDecimal(targetValueTextBox.Text, "Target value", [], out _) ||
                                        !TryParseDecimal(toleranceTextBox.Text, "Tolerance", [], out _)))
        {
            lines.Add("- Numeric target and tolerance must be completed before the test can be added.");
        }

        summaryTextBox.Text = string.Join(Environment.NewLine, lines);
    }

    private string InputSummary()
    {
        if (!useStructuredInputsCheckBox.Checked)
        {
            return "Structured inputs are off for this test.";
        }

        var inputs = BuildTestInputsIgnoringInvalidRows();
        if (inputs.Count == 0)
        {
            return "No structured inputs defined.";
        }

        var requiredCount = inputs.Count(input => input.Required);
        return $"{inputs.Count} input(s) defined; {requiredCount} required. " +
            string.Join("; ", inputs.Select(input => input.DescribeDefinition()));
    }

    private string AcceptanceSummary()
    {
        if (numericPassRadio.Checked)
        {
            var target = string.IsNullOrWhiteSpace(targetValueTextBox.Text) ? "target not set" : targetValueTextBox.Text.Trim();
            var tolerance = string.IsNullOrWhiteSpace(toleranceTextBox.Text) ? "tolerance not set" : toleranceTextBox.Text.Trim();
            var unit = string.IsNullOrWhiteSpace(unitTextBox.Text) ? string.Empty : " " + unitTextBox.Text.Trim();
            return $"Pass when measured value is {target}{unit} +/- {tolerance}.";
        }

        if (booleanPassRadio.Checked)
        {
            return "Pass when the stated boolean condition is true.";
        }

        return "Pass by manual confirmation against the expected result.";
    }

    private void WireSummaryUpdates(Control root)
    {
        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.TextChanged += (_, _) => UpdateSummary();
                    break;
                case CheckBox checkBox:
                    checkBox.CheckedChanged += (_, _) =>
                    {
                        if (ReferenceEquals(checkBox, useStructuredInputsCheckBox))
                        {
                            UpdateStructuredInputsVisibility();
                        }

                        UpdateSummary();
                    };
                    break;
                case RadioButton radioButton:
                    radioButton.CheckedChanged += (_, _) =>
                    {
                        UpdateAcceptanceControls();
                        UpdateSummary();
                    };
                    break;
                case ComboBox comboBox:
                    comboBox.SelectedIndexChanged += (_, _) => ApplySelectedTemplate();
                    break;
                case DataGridView grid:
                    grid.CellValueChanged += (_, _) => UpdateSummary();
                    grid.RowsAdded += (_, _) => UpdateSummary();
                    grid.RowsRemoved += (_, _) => UpdateSummary();
                    grid.CurrentCellDirtyStateChanged += (_, _) =>
                    {
                        if (grid.IsCurrentCellDirty)
                        {
                            grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                        }
                    };
                    grid.DataError += (_, _) => { };
                    break;
            }

            WireSummaryUpdates(control);
        }
    }

    private void ConfigureInputsGrid()
    {
        inputsGrid.Dock = DockStyle.Fill;
        inputsGrid.AllowUserToAddRows = false;
        inputsGrid.AllowUserToDeleteRows = false;
        inputsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        inputsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        inputsGrid.MultiSelect = true;
        inputsGrid.RowHeadersVisible = false;
        inputsGrid.Height = 160;
        inputsGrid.RowTemplate.Height = 24;
        inputsGrid.Columns.Clear();
        inputsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Label",
            HeaderText = "Label",
            FillWeight = 150
        });
        inputsGrid.Columns.Add(new DataGridViewComboBoxColumn
        {
            Name = "Type",
            HeaderText = "Type",
            DataSource = Enum.GetNames<TestInputType>(),
            FillWeight = 90
        });
        inputsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Target",
            HeaderText = "Target",
            FillWeight = 80
        });
        inputsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Tolerance",
            HeaderText = "+/-",
            FillWeight = 70
        });
        inputsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Unit",
            HeaderText = "Unit",
            FillWeight = 70
        });
        inputsGrid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "Required",
            HeaderText = "Required",
            FillWeight = 70
        });
        inputsGrid.CellValueChanged += (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                UpdateInputRowState(e.RowIndex);
            }
        };
        inputsGrid.SelectionChanged += (_, _) => UpdateInputButtons();
        inputsGrid.RowsAdded += (_, e) =>
        {
            for (var index = e.RowIndex; index < e.RowIndex + e.RowCount; index++)
            {
                UpdateInputRowState(index);
            }

            UpdateInputButtons();
        };
        inputsGrid.RowsRemoved += (_, _) => UpdateInputButtons();
        UpdateInputButtons();
    }

    private void AddInputRow()
    {
        var rowIndex = inputsGrid.Rows.Add();
        inputsGrid.Rows[rowIndex].Cells["Type"].Value = nameof(TestInputType.Numeric);
        inputsGrid.Rows[rowIndex].Cells["Required"].Value = true;
        UpdateInputRowState(rowIndex);
        inputsGrid.CurrentCell = inputsGrid.Rows[rowIndex].Cells["Label"];
        UpdateSummary();
        UpdateInputButtons();
    }

    private void AddTemplateInputIfEmpty(string label, TestInputType inputType)
    {
        if (inputsGrid.Rows.Cast<DataGridViewRow>().Any(row => !IsInputRowEmpty(row)))
        {
            return;
        }

        var rowIndex = inputsGrid.Rows.Add();
        var row = inputsGrid.Rows[rowIndex];
        row.Cells["Label"].Value = label;
        row.Cells["Type"].Value = inputType.ToString();
        row.Cells["Required"].Value = true;
        UpdateInputRowState(rowIndex);
        UpdateSummary();
    }

    private void UpdateInputRowState(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= inputsGrid.Rows.Count)
        {
            return;
        }

        var row = inputsGrid.Rows[rowIndex];
        if (row.IsNewRow)
        {
            return;
        }

        var numeric = string.Equals(CellText(row, "Type"), nameof(TestInputType.Numeric), StringComparison.OrdinalIgnoreCase);
        SetNumericInputCellState(row.Cells["Target"], numeric);
        SetNumericInputCellState(row.Cells["Tolerance"], numeric);
        SetNumericInputCellState(row.Cells["Unit"], numeric);
    }

    private static void SetNumericInputCellState(DataGridViewCell cell, bool enabled)
    {
        cell.ReadOnly = !enabled;
        cell.Style.BackColor = enabled ? AppTheme.Current.InputBackground : AppTheme.Current.InputReadOnlyBackground;
        cell.Style.ForeColor = enabled ? AppTheme.Current.InputForeground : AppTheme.Current.TextMuted;
        if (!enabled)
        {
            cell.Value = null;
        }
    }

    private void RemoveSelectedInputRows()
    {
        foreach (var row in inputsGrid.SelectedRows.Cast<DataGridViewRow>().ToList())
        {
            if (!row.IsNewRow)
            {
                inputsGrid.Rows.Remove(row);
            }
        }

        UpdateSummary();
        UpdateInputButtons();
    }

    private void UpdateInputButtons()
    {
        removeInputButton.Enabled = inputsGrid.SelectedRows.Count > 0;
    }

    private IReadOnlyList<TestInput> BuildTestInputs()
    {
        if (!useStructuredInputsCheckBox.Checked)
        {
            return [];
        }

        var issues = new List<string>();
        var inputs = BuildTestInputs(issues);
        if (issues.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, issues));
        }

        return inputs;
    }

    private IReadOnlyList<TestInput> BuildTestInputsIgnoringInvalidRows()
    {
        if (!useStructuredInputsCheckBox.Checked)
        {
            return [];
        }

        return BuildTestInputs([]);
    }

    private IReadOnlyList<TestInput> BuildTestInputs(List<string> issues)
    {
        var inputs = new List<TestInput>();
        foreach (var row in inputsGrid.Rows.Cast<DataGridViewRow>())
        {
            if (row.IsNewRow || IsInputRowEmpty(row))
            {
                continue;
            }

            var label = CellText(row, "Label");
            if (string.IsNullOrWhiteSpace(label))
            {
                issues.Add("Each test input row needs a label.");
                continue;
            }

            if (!Enum.TryParse<TestInputType>(CellText(row, "Type"), out var inputType))
            {
                issues.Add($"Input '{label}' needs a valid type.");
                continue;
            }

            var target = ParseOptionalDecimal(CellText(row, "Target"), $"Target for input '{label}'", issues);
            var tolerance = ParseOptionalDecimal(CellText(row, "Tolerance"), $"Tolerance for input '{label}'", issues);
            if (tolerance < 0)
            {
                issues.Add($"Tolerance for input '{label}' cannot be negative.");
            }

            var required = row.Cells["Required"].Value is bool value && value;
            inputs.Add(TestInput.Create(
                Guid.NewGuid(),
                label,
                inputType,
                target,
                tolerance,
                CellText(row, "Unit"),
                required,
                inputs.Count + 1));
        }

        return inputs;
    }

    private void ValidateTestInputs(List<string> issues)
    {
        if (!useStructuredInputsCheckBox.Checked)
        {
            return;
        }

        BuildTestInputs(issues);
    }

    private static bool IsInputRowEmpty(DataGridViewRow row)
    {
        return string.IsNullOrWhiteSpace(CellText(row, "Label")) &&
            string.IsNullOrWhiteSpace(CellText(row, "Target")) &&
            string.IsNullOrWhiteSpace(CellText(row, "Tolerance")) &&
            string.IsNullOrWhiteSpace(CellText(row, "Unit"));
    }

    private static string CellText(DataGridViewRow row, string columnName)
    {
        return row.Cells[columnName].Value?.ToString()?.Trim() ?? string.Empty;
    }

    private static decimal? ParseOptionalDecimal(string value, string label, List<string> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return TryParseDecimal(value, label, issues, out var parsed) ? parsed : null;
    }

    private static GroupBox CreateGroup(string title)
    {
        return new GroupBox
        {
            Text = title,
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10),
            Margin = new Padding(0, 0, 10, 12)
        };
    }

    private static TableLayoutPanel CreateFieldTable()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(12, 20, 12, 12)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return table;
    }

    private static void AddComboRow(TableLayoutPanel layout, int row, string label, ComboBox comboBox, string[] values)
    {
        EnsureRow(layout, row);
        layout.Controls.Add(CreateLabel(label), 0, row);
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(values);
        comboBox.Dock = DockStyle.Left;
        comboBox.Width = 220;
        comboBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(comboBox, 1, row);
    }

    private static void AddTextRow(TableLayoutPanel layout, int row, string label, TextBox textBox)
    {
        EnsureRow(layout, row);
        layout.Controls.Add(CreateLabel(label), 0, row);
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(textBox, 1, row);
    }

    private static void AddLabeledControlRow(TableLayoutPanel layout, int row, string label, Label labelControl, TextBox textBox)
    {
        EnsureRow(layout, row);
        ConfigureLabel(labelControl, label);
        layout.Controls.Add(labelControl, 0, row);
        textBox.Dock = DockStyle.Left;
        textBox.Width = 160;
        textBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(textBox, 1, row);
    }

    private static void AddMultilineRow(TableLayoutPanel layout, int row, string label, TextBox textBox, int height)
    {
        EnsureRow(layout, row, height);
        layout.Controls.Add(CreateLabel(label), 0, row);
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.Vertical;
        textBox.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(textBox, 1, row);
    }

    private static void ConfigureCheckBox(CheckBox checkBox, string text)
    {
        checkBox.Text = text;
        checkBox.AutoSize = true;
        checkBox.Margin = new Padding(0, 0, 12, 8);
    }

    private static void AddCheck(TableLayoutPanel layout, CheckBox checkBox, int column, int row)
    {
        EnsureRow(layout, row);
        layout.Controls.Add(checkBox, column, row);
    }

    private static Label CreateLabel(string text)
    {
        var label = new Label();
        ConfigureLabel(label, text);
        return label;
    }

    private static void ConfigureLabel(Label label, string text)
    {
        label.Text = text;
        label.AutoSize = true;
        label.Anchor = AnchorStyles.Left;
        label.Margin = new Padding(0, 4, 12, 8);
    }

    private static void EnsureRow(TableLayoutPanel layout, int row, int? height = null)
    {
        while (layout.RowCount <= row)
        {
            layout.RowCount++;
            layout.RowStyles.Add(height.HasValue
                ? new RowStyle(SizeType.Absolute, height.Value)
                : new RowStyle(SizeType.AutoSize));
        }
    }

    private static bool TryParseDecimal(string value, string label, List<string> issues, out decimal parsed)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed) ||
            decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
        {
            return true;
        }

        issues.Add($"{label} must be a number.");
        return false;
    }

    private static decimal ParseDecimal(string value, string label)
    {
        return TryParseDecimal(value, label, [], out var parsed)
            ? parsed
            : throw new InvalidOperationException($"{label} must be a number.");
    }
}
