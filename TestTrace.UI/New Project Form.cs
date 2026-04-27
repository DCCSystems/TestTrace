using System;
using System.Windows.Forms;

namespace TestTrace.UI
{
    public partial class NewProjectForm : Form
    {
        // ===== Dirty State Tracking =====
        private bool _isDirty = false;

        public NewProjectForm()
        {
            // ===== Initialization =====
            InitializeComponent();

            // ===== Flicker Reduction =====
            EnableFlickerReduction();

            // ===== Theme =====
            ThemeManager.ApplyTheme(this);

            // ===== Modal dialog behaviour =====
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;

            // ===== Dirty tracking =====
            WireDirtyTracking(this);
        }

        // Ensure double buffering is active as soon as the handle exists
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            EnableFlickerReduction();
        }

        // Centralised flicker control
        private void EnableFlickerReduction()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        private void NewProjectForm_Load(object sender, EventArgs e)
        {
            // Intentionally empty
            // Theme already applied in constructor to avoid repaint
        }

        // ===== Dirty Tracking Wiring =====
        private void WireDirtyTracking(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox tb)
                    tb.TextChanged += (_, __) => _isDirty = true;

                if (c is ComboBox cb)
                    cb.SelectedIndexChanged += (_, __) => _isDirty = true;

                if (c is DateTimePicker dp)
                    dp.ValueChanged += (_, __) => _isDirty = true;

                if (c.HasChildren)
                    WireDirtyTracking(c);
            }
        }

        // ===== Save Button =====
        private void btnNPFSave_click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            // TODO (next phase):
            // Build ProjectDefinition
            // Persist project
            // Transition to workspace

            MessageBox.Show(
                "Project definition validated successfully.",
                "New Project",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ===== Cancel Button =====
        private void btnNPFCancel_Click(object sender, EventArgs e)
        {
            if (_isDirty)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to cancel?\n\nAny unsaved project data will be lost.",
                    "Discard New Project?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                    return;
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ===== Validation =====
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show(
                    "Customer Name is required.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                txtCustomerName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLeadTestEng.Text))
            {
                MessageBox.Show(
                    "Lead Test Engineer is required.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                txtLeadTestEng.Focus();
                return false;
            }

            return true;
        }

        private void btnBuildContract_click(object sender, EventArgs e)
        {

        }
    }
}
