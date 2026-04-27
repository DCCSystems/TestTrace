using System;
using System.Windows.Forms;

namespace TestTrace.UI
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            // ===== Initialization =====
            InitializeComponent();

            // ===== Flicker Reduction =====
            EnableFlickerReduction();

            // ===== Apply Theme =====
            ThemeManager.ApplyTheme(this);

            // ===== Startup Behavior =====
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
        }

        // Prevent background erase flicker on modal open
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            EnableFlickerReduction();
        }

        // Enables double buffering and disables background erase
        private void EnableFlickerReduction()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        private void btnStartNewProject_Click(object sender, EventArgs e)
        {
            using (var newProjectForm = new NewProjectForm())
            {
                newProjectForm.StartPosition = FormStartPosition.CenterParent;
                newProjectForm.ShowInTaskbar = false;
                newProjectForm.ShowIcon = false;

                // Modal child of MainMenu
                newProjectForm.ShowDialog(this);
            }

            // No Hide/Show needed — Windows manages focus automatically
        }

        private void btnOpenProject_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open Project clicked");
        }

        private void btnExportProject_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Export Project clicked");
        }

        private void btnEditProject_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Edit Project clicked");
        }

        private void btnCloneProject_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Clone Project clicked");
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            // Intentionally empty — theme applied in constructor
        }
    }
}
