namespace TestTrace.UI
{
    partial class MainMenu
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            btnStartNewProject = new Button();
            btnOpenProject = new Button();
            btnExportProject = new Button();
            btnEditProject = new Button();
            btnCloneProject = new Button();
            picMMTestTraceLogo = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)picMMTestTraceLogo).BeginInit();
            SuspendLayout();
            // 
            // btnStartNewProject
            // 
            btnStartNewProject.Location = new Point(108, 70);
            btnStartNewProject.Name = "btnStartNewProject";
            btnStartNewProject.Size = new Size(126, 28);
            btnStartNewProject.TabIndex = 5;
            btnStartNewProject.Text = "Start New Project";
            btnStartNewProject.Click += btnStartNewProject_Click;
            // 
            // btnOpenProject
            // 
            btnOpenProject.Location = new Point(108, 166);
            btnOpenProject.Name = "btnOpenProject";
            btnOpenProject.Size = new Size(126, 28);
            btnOpenProject.TabIndex = 4;
            btnOpenProject.Text = "Open Project";
            btnOpenProject.Click += btnOpenProject_Click;
            // 
            // btnExportProject
            // 
            btnExportProject.Location = new Point(108, 198);
            btnExportProject.Name = "btnExportProject";
            btnExportProject.Size = new Size(126, 28);
            btnExportProject.TabIndex = 3;
            btnExportProject.Text = "Export Project";
            btnExportProject.Click += btnExportProject_Click;
            // 
            // btnEditProject
            // 
            btnEditProject.Location = new Point(108, 102);
            btnEditProject.Name = "btnEditProject";
            btnEditProject.Size = new Size(126, 28);
            btnEditProject.TabIndex = 1;
            btnEditProject.Text = "Edit Project";
            btnEditProject.Click += btnEditProject_Click;
            // 
            // btnCloneProject
            // 
            btnCloneProject.Location = new Point(108, 134);
            btnCloneProject.Name = "btnCloneProject";
            btnCloneProject.Size = new Size(126, 28);
            btnCloneProject.TabIndex = 0;
            btnCloneProject.Text = "Clone Project";
            btnCloneProject.Click += btnCloneProject_Click;
            // 
            // picMMTestTraceLogo
            // 
            picMMTestTraceLogo.Image = Properties.Resources.TestTrace_Header_Logo_No_Background;
            picMMTestTraceLogo.Location = new Point(12, 12);
            picMMTestTraceLogo.Name = "picMMTestTraceLogo";
            picMMTestTraceLogo.Size = new Size(87, 34);
            picMMTestTraceLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picMMTestTraceLogo.TabIndex = 6;
            picMMTestTraceLogo.TabStop = false;
            // 
            // MainMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(335, 245);
            Controls.Add(picMMTestTraceLogo);
            Controls.Add(btnCloneProject);
            Controls.Add(btnEditProject);
            Controls.Add(btnExportProject);
            Controls.Add(btnOpenProject);
            Controls.Add(btnStartNewProject);
            Name = "MainMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TestTrace - Main Menu";
            Load += MainMenu_Load;
            ((System.ComponentModel.ISupportInitialize)picMMTestTraceLogo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnStartNewProject;
        private Button btnOpenProject;
        private Button btnExportProject;
        private Button btnEditProject;
        private Button btnCloneProject;
        private PictureBox picMMTestTraceLogo;
    }
}
