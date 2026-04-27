namespace TestTrace.UI
{
    partial class MainWorkspace
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            PanelHeaderMW = new Panel();
            btnMinimiseApp = new Button();
            btnMaxRestoreApp = new Button();
            btnExitApp = new Button();
            lblHeaderTitleMW = new Label();
            lblPanelLauncher = new Panel();
            btnTestTraceLaunch = new Button();
            PanelHeaderMW.SuspendLayout();
            lblPanelLauncher.SuspendLayout();
            SuspendLayout();
            // 
            // PanelHeaderMW
            // 
            PanelHeaderMW.BackColor = SystemColors.Control;
            PanelHeaderMW.Controls.Add(btnMinimiseApp);
            PanelHeaderMW.Controls.Add(btnMaxRestoreApp);
            PanelHeaderMW.Controls.Add(btnExitApp);
            PanelHeaderMW.Controls.Add(lblHeaderTitleMW);
            PanelHeaderMW.Dock = DockStyle.Top;
            PanelHeaderMW.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            PanelHeaderMW.Location = new Point(0, 0);
            PanelHeaderMW.Margin = new Padding(5, 6, 5, 6);
            PanelHeaderMW.Name = "PanelHeaderMW";
            PanelHeaderMW.Size = new Size(1264, 40);
            PanelHeaderMW.TabIndex = 0;
            PanelHeaderMW.MouseDown += PanelHeaderMW_MouseDown;
            // 
            // btnMinimiseApp
            // 
            btnMinimiseApp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimiseApp.FlatAppearance.BorderSize = 0;
            btnMinimiseApp.FlatStyle = FlatStyle.Flat;
            btnMinimiseApp.Font = new Font("Segoe MDL2 Assets", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnMinimiseApp.ImageAlign = ContentAlignment.BottomCenter;
            btnMinimiseApp.Location = new Point(1134, 6);
            btnMinimiseApp.Name = "btnMinimiseApp";
            btnMinimiseApp.Size = new Size(26, 28);
            btnMinimiseApp.TabIndex = 3;
            btnMinimiseApp.Text = "—";
            btnMinimiseApp.UseVisualStyleBackColor = true;
            btnMinimiseApp.Click += btnMinimiseApp_Click;
            // 
            // btnMaxRestoreApp
            // 
            btnMaxRestoreApp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaxRestoreApp.FlatAppearance.BorderSize = 0;
            btnMaxRestoreApp.FlatStyle = FlatStyle.Flat;
            btnMaxRestoreApp.Font = new Font("Segoe MDL2 Assets", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnMaxRestoreApp.Location = new Point(1179, 6);
            btnMaxRestoreApp.Name = "btnMaxRestoreApp";
            btnMaxRestoreApp.Size = new Size(26, 28);
            btnMaxRestoreApp.TabIndex = 2;
            btnMaxRestoreApp.Text = "❐";
            btnMaxRestoreApp.UseVisualStyleBackColor = true;
            btnMaxRestoreApp.Click += btnMaxRestoreApp_Click;
            // 
            // btnExitApp
            // 
            btnExitApp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExitApp.FlatAppearance.BorderSize = 0;
            btnExitApp.FlatStyle = FlatStyle.Flat;
            btnExitApp.Font = new Font("Segoe MDL2 Assets", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnExitApp.Location = new Point(1224, 6);
            btnExitApp.Name = "btnExitApp";
            btnExitApp.Size = new Size(26, 28);
            btnExitApp.TabIndex = 1;
            btnExitApp.Text = "✕";
            btnExitApp.UseVisualStyleBackColor = true;
            btnExitApp.Click += btnExitApp_Click;
            // 
            // lblHeaderTitleMW
            // 
            lblHeaderTitleMW.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblHeaderTitleMW.Location = new Point(5, 6);
            lblHeaderTitleMW.Margin = new Padding(5, 0, 5, 0);
            lblHeaderTitleMW.Name = "lblHeaderTitleMW";
            lblHeaderTitleMW.Size = new Size(146, 30);
            lblHeaderTitleMW.TabIndex = 0;
            lblHeaderTitleMW.Text = "DCC Systems";
            // 
            // lblPanelLauncher
            // 
            lblPanelLauncher.Controls.Add(btnTestTraceLaunch);
            lblPanelLauncher.Dock = DockStyle.Fill;
            lblPanelLauncher.Location = new Point(0, 40);
            lblPanelLauncher.Name = "lblPanelLauncher";
            lblPanelLauncher.Size = new Size(1264, 721);
            lblPanelLauncher.TabIndex = 1;
            // 
            // btnTestTraceLaunch
            // 
            btnTestTraceLaunch.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnTestTraceLaunch.Location = new Point(112, 79);
            btnTestTraceLaunch.Name = "btnTestTraceLaunch";
            btnTestTraceLaunch.Size = new Size(200, 50);
            btnTestTraceLaunch.TabIndex = 0;
            btnTestTraceLaunch.Tag = "Primary";
            btnTestTraceLaunch.Text = "TestTrace";
            btnTestTraceLaunch.UseVisualStyleBackColor = true;
            btnTestTraceLaunch.Click += btnTestTraceLaunch_Click;
            // 
            // MainWorkspace
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1264, 761);
            Controls.Add(lblPanelLauncher);
            Controls.Add(PanelHeaderMW);
            Font = new Font("Segoe UI", 16F);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainWorkspace";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainWorkspace";
            WindowState = FormWindowState.Maximized;
            PanelHeaderMW.ResumeLayout(false);
            lblPanelLauncher.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel PanelHeaderMW;
        private Label lblHeaderTitleMW;
        private Panel lblPanelLauncher;
        private Button btnTestTraceLaunch;
        private Button btnExitApp;
        private Button btnMaxRestoreApp;
        private Button btnMinimiseApp;
    }
}