namespace TestTrace.UI
{
    partial class NewProjectForm
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
            lblTitle = new Label();
            pnlNewProjectTitle = new Panel();
            lblTitleNewProject = new Label();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            pnlNewProjectBase = new Panel();
            pnlNPFFooterActions = new Panel();
            btnNPFCancel = new Button();
            btnBuildContract = new Button();
            pnlContent = new Panel();
            pnlNPFScrollSpacer = new Panel();
            grpRolesResponsibilities = new GroupBox();
            tblRolesResponsibilities = new TableLayoutPanel();
            lblLeadEngineerPhone = new Label();
            txtLeadEngineerPhone = new TextBox();
            lblLeadTestEng = new Label();
            txtLeadTestEng = new TextBox();
            lblLeadEngineerEmail = new Label();
            txtLeadEngineerEmail = new TextBox();
            grpMachineInfo = new GroupBox();
            tblMachineInfo = new TableLayoutPanel();
            lblMachineRole = new Label();
            lblMachineNumber = new Label();
            txtMachineRole = new TextBox();
            txtMachineConfig = new TextBox();
            lblMachineConfig = new Label();
            lblMachineModel = new Label();
            txtMachineModel = new TextBox();
            lblControlPlatform = new Label();
            txtControlPlatform = new TextBox();
            txtMachineNumber = new TextBox();
            grpCustomerInfo = new GroupBox();
            tblCustomerInfo = new TableLayoutPanel();
            lblCustProjRef = new Label();
            lblSiteContNumber = new Label();
            lblSiteContEmail = new Label();
            lblCustomerName = new Label();
            lblCustomerAddress = new Label();
            txtCustomerName = new TextBox();
            txtCustomerLocation = new TextBox();
            lblCustomerCountry = new Label();
            txtCustomerCountry = new TextBox();
            txtSiteContName = new TextBox();
            txtSiteContEmail = new TextBox();
            txtSiteContNumber = new TextBox();
            txtCustProjRef = new TextBox();
            lblSiteContName = new Label();
            grpProjectInfo = new GroupBox();
            tblProjectInfo = new TableLayoutPanel();
            txtProjectScope = new TextBox();
            lblProjectScope = new Label();
            lblProjectType = new Label();
            comboBox1 = new ComboBox();
            lblStartDate = new Label();
            dateTimePicker1 = new DateTimePicker();
            pnlNewProjectTitle.SuspendLayout();
            pnlNewProjectBase.SuspendLayout();
            pnlNPFFooterActions.SuspendLayout();
            pnlContent.SuspendLayout();
            grpRolesResponsibilities.SuspendLayout();
            tblRolesResponsibilities.SuspendLayout();
            grpMachineInfo.SuspendLayout();
            tblMachineInfo.SuspendLayout();
            grpCustomerInfo.SuspendLayout();
            tblCustomerInfo.SuspendLayout();
            grpProjectInfo.SuspendLayout();
            tblProjectInfo.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(388, 9);
            lblTitle.Margin = new Padding(2, 0, 2, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(119, 32);
            lblTitle.TabIndex = 4;
            lblTitle.Text = "TestTrace";
            // 
            // pnlNewProjectTitle
            // 
            pnlNewProjectTitle.Controls.Add(lblTitleNewProject);
            pnlNewProjectTitle.Controls.Add(lblTitle);
            pnlNewProjectTitle.Dock = DockStyle.Top;
            pnlNewProjectTitle.Location = new Point(0, 0);
            pnlNewProjectTitle.Name = "pnlNewProjectTitle";
            pnlNewProjectTitle.Size = new Size(884, 50);
            pnlNewProjectTitle.TabIndex = 16;
            // 
            // lblTitleNewProject
            // 
            lblTitleNewProject.AutoSize = true;
            lblTitleNewProject.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitleNewProject.Location = new Point(12, 9);
            lblTitleNewProject.Name = "lblTitleNewProject";
            lblTitleNewProject.Size = new Size(172, 25);
            lblTitleNewProject.TabIndex = 0;
            lblTitleNewProject.Text = "New Project Form";
            // 
            // pnlNewProjectBase
            // 
            pnlNewProjectBase.Controls.Add(pnlNPFFooterActions);
            pnlNewProjectBase.Controls.Add(pnlContent);
            pnlNewProjectBase.Controls.Add(pnlNewProjectTitle);
            pnlNewProjectBase.Dock = DockStyle.Fill;
            pnlNewProjectBase.Location = new Point(0, 0);
            pnlNewProjectBase.Name = "pnlNewProjectBase";
            pnlNewProjectBase.Size = new Size(884, 1048);
            pnlNewProjectBase.TabIndex = 17;
            // 
            // pnlNPFFooterActions
            // 
            pnlNPFFooterActions.Controls.Add(btnNPFCancel);
            pnlNPFFooterActions.Controls.Add(btnBuildContract);
            pnlNPFFooterActions.Dock = DockStyle.Bottom;
            pnlNPFFooterActions.Location = new Point(0, 998);
            pnlNPFFooterActions.Name = "pnlNPFFooterActions";
            pnlNPFFooterActions.Padding = new Padding(0, 0, 0, 8);
            pnlNPFFooterActions.Size = new Size(884, 50);
            pnlNPFFooterActions.TabIndex = 1;
            // 
            // btnNPFCancel
            // 
            btnNPFCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNPFCancel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNPFCancel.Location = new Point(39, 9);
            btnNPFCancel.Name = "btnNPFCancel";
            btnNPFCancel.Size = new Size(100, 30);
            btnNPFCancel.TabIndex = 1;
            btnNPFCancel.Text = "Cancel";
            btnNPFCancel.UseVisualStyleBackColor = true;
            btnNPFCancel.Click += btnNPFCancel_Click;
            // 
            // btnBuildContract
            // 
            btnBuildContract.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnBuildContract.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBuildContract.Location = new Point(733, 10);
            btnBuildContract.Name = "btnBuildContract";
            btnBuildContract.Size = new Size(100, 30);
            btnBuildContract.TabIndex = 0;
            btnBuildContract.Text = "Build Contract";
            btnBuildContract.UseVisualStyleBackColor = true;
            btnBuildContract.Click += btnBuildContract_click;
            // 
            // pnlContent
            // 
            pnlContent.AutoScroll = true;
            pnlContent.Controls.Add(pnlNPFScrollSpacer);
            pnlContent.Controls.Add(grpRolesResponsibilities);
            pnlContent.Controls.Add(grpMachineInfo);
            pnlContent.Controls.Add(grpCustomerInfo);
            pnlContent.Controls.Add(grpProjectInfo);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 50);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(0, 0, 0, 60);
            pnlContent.Size = new Size(884, 998);
            pnlContent.TabIndex = 21;
            // 
            // pnlNPFScrollSpacer
            // 
            pnlNPFScrollSpacer.Dock = DockStyle.Top;
            pnlNPFScrollSpacer.Location = new Point(0, 1068);
            pnlNPFScrollSpacer.Name = "pnlNPFScrollSpacer";
            pnlNPFScrollSpacer.Size = new Size(867, 60);
            pnlNPFScrollSpacer.TabIndex = 2;
            // 
            // grpRolesResponsibilities
            // 
            grpRolesResponsibilities.Controls.Add(tblRolesResponsibilities);
            grpRolesResponsibilities.Dock = DockStyle.Top;
            grpRolesResponsibilities.Location = new Point(0, 910);
            grpRolesResponsibilities.Name = "grpRolesResponsibilities";
            grpRolesResponsibilities.Padding = new Padding(12);
            grpRolesResponsibilities.Size = new Size(867, 158);
            grpRolesResponsibilities.TabIndex = 21;
            grpRolesResponsibilities.TabStop = false;
            grpRolesResponsibilities.Text = "Roles & Responsibilities";
            // 
            // tblRolesResponsibilities
            // 
            tblRolesResponsibilities.ColumnCount = 2;
            tblRolesResponsibilities.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblRolesResponsibilities.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tblRolesResponsibilities.Controls.Add(lblLeadEngineerPhone, 0, 2);
            tblRolesResponsibilities.Controls.Add(txtLeadEngineerPhone, 1, 2);
            tblRolesResponsibilities.Controls.Add(lblLeadTestEng, 0, 0);
            tblRolesResponsibilities.Controls.Add(txtLeadTestEng, 1, 0);
            tblRolesResponsibilities.Controls.Add(lblLeadEngineerEmail, 0, 1);
            tblRolesResponsibilities.Controls.Add(txtLeadEngineerEmail, 1, 1);
            tblRolesResponsibilities.Dock = DockStyle.Fill;
            tblRolesResponsibilities.Location = new Point(12, 28);
            tblRolesResponsibilities.Name = "tblRolesResponsibilities";
            tblRolesResponsibilities.Padding = new Padding(6);
            tblRolesResponsibilities.RowCount = 3;
            tblRolesResponsibilities.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblRolesResponsibilities.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblRolesResponsibilities.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblRolesResponsibilities.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblRolesResponsibilities.Size = new Size(843, 118);
            tblRolesResponsibilities.TabIndex = 0;
            // 
            // lblLeadEngineerPhone
            // 
            lblLeadEngineerPhone.Anchor = AnchorStyles.Right;
            lblLeadEngineerPhone.AutoSize = true;
            lblLeadEngineerPhone.Location = new Point(151, 85);
            lblLeadEngineerPhone.Name = "lblLeadEngineerPhone";
            lblLeadEngineerPhone.Size = new Size(142, 15);
            lblLeadEngineerPhone.TabIndex = 23;
            lblLeadEngineerPhone.Text = "Lead Test Engineer Phone";
            // 
            // txtLeadEngineerPhone
            // 
            txtLeadEngineerPhone.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtLeadEngineerPhone.Location = new Point(299, 81);
            txtLeadEngineerPhone.Name = "txtLeadEngineerPhone";
            txtLeadEngineerPhone.Size = new Size(535, 23);
            txtLeadEngineerPhone.TabIndex = 22;
            // 
            // lblLeadTestEng
            // 
            lblLeadTestEng.Anchor = AnchorStyles.Right;
            lblLeadTestEng.AutoSize = true;
            lblLeadTestEng.Location = new Point(189, 15);
            lblLeadTestEng.Margin = new Padding(2, 0, 2, 0);
            lblLeadTestEng.Name = "lblLeadTestEng";
            lblLeadTestEng.Size = new Size(105, 15);
            lblLeadTestEng.TabIndex = 21;
            lblLeadTestEng.Text = "Lead Test Engineer";
            // 
            // txtLeadTestEng
            // 
            txtLeadTestEng.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtLeadTestEng.Location = new Point(298, 11);
            txtLeadTestEng.Margin = new Padding(2);
            txtLeadTestEng.Name = "txtLeadTestEng";
            txtLeadTestEng.Size = new Size(537, 23);
            txtLeadTestEng.TabIndex = 17;
            // 
            // lblLeadEngineerEmail
            // 
            lblLeadEngineerEmail.Anchor = AnchorStyles.Right;
            lblLeadEngineerEmail.AutoSize = true;
            lblLeadEngineerEmail.Location = new Point(156, 49);
            lblLeadEngineerEmail.Name = "lblLeadEngineerEmail";
            lblLeadEngineerEmail.Size = new Size(137, 15);
            lblLeadEngineerEmail.TabIndex = 16;
            lblLeadEngineerEmail.Text = "Lead Test Engineer Email";
            // 
            // txtLeadEngineerEmail
            // 
            txtLeadEngineerEmail.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtLeadEngineerEmail.Location = new Point(299, 45);
            txtLeadEngineerEmail.Name = "txtLeadEngineerEmail";
            txtLeadEngineerEmail.Size = new Size(535, 23);
            txtLeadEngineerEmail.TabIndex = 12;
            // 
            // grpMachineInfo
            // 
            grpMachineInfo.Controls.Add(tblMachineInfo);
            grpMachineInfo.Dock = DockStyle.Top;
            grpMachineInfo.Location = new Point(0, 579);
            grpMachineInfo.Name = "grpMachineInfo";
            grpMachineInfo.Padding = new Padding(0, 10, 12, 12);
            grpMachineInfo.Size = new Size(867, 331);
            grpMachineInfo.TabIndex = 18;
            grpMachineInfo.TabStop = false;
            grpMachineInfo.Text = "Machine Information";
            // 
            // tblMachineInfo
            // 
            tblMachineInfo.ColumnCount = 2;
            tblMachineInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblMachineInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tblMachineInfo.Controls.Add(lblMachineRole, 0, 4);
            tblMachineInfo.Controls.Add(lblMachineNumber, 0, 2);
            tblMachineInfo.Controls.Add(txtMachineRole, 1, 4);
            tblMachineInfo.Controls.Add(txtMachineConfig, 1, 1);
            tblMachineInfo.Controls.Add(lblMachineConfig, 0, 1);
            tblMachineInfo.Controls.Add(lblMachineModel, 0, 0);
            tblMachineInfo.Controls.Add(txtMachineModel, 1, 0);
            tblMachineInfo.Controls.Add(lblControlPlatform, 0, 3);
            tblMachineInfo.Controls.Add(txtControlPlatform, 1, 3);
            tblMachineInfo.Controls.Add(txtMachineNumber, 1, 2);
            tblMachineInfo.Dock = DockStyle.Fill;
            tblMachineInfo.Location = new Point(0, 26);
            tblMachineInfo.Name = "tblMachineInfo";
            tblMachineInfo.Padding = new Padding(6);
            tblMachineInfo.RowCount = 5;
            tblMachineInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblMachineInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tblMachineInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblMachineInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblMachineInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblMachineInfo.Size = new Size(855, 293);
            tblMachineInfo.TabIndex = 0;
            // 
            // lblMachineRole
            // 
            lblMachineRole.Anchor = AnchorStyles.Right;
            lblMachineRole.AutoSize = true;
            lblMachineRole.Location = new Point(148, 240);
            lblMachineRole.Margin = new Padding(2, 0, 2, 0);
            lblMachineRole.Name = "lblMachineRole";
            lblMachineRole.Size = new Size(151, 15);
            lblMachineRole.TabIndex = 19;
            lblMachineRole.Text = "Machine Role / Application";
            lblMachineRole.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblMachineNumber
            // 
            lblMachineNumber.Anchor = AnchorStyles.Right;
            lblMachineNumber.AutoSize = true;
            lblMachineNumber.Location = new Point(168, 149);
            lblMachineNumber.Margin = new Padding(2, 0, 2, 0);
            lblMachineNumber.Name = "lblMachineNumber";
            lblMachineNumber.Size = new Size(131, 15);
            lblMachineNumber.TabIndex = 10;
            lblMachineNumber.Text = "Machine Serial Number";
            lblMachineNumber.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtMachineRole
            // 
            txtMachineRole.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtMachineRole.Location = new Point(304, 236);
            txtMachineRole.Margin = new Padding(3, 6, 10, 6);
            txtMachineRole.Name = "txtMachineRole";
            txtMachineRole.Size = new Size(535, 23);
            txtMachineRole.TabIndex = 20;
            // 
            // txtMachineConfig
            // 
            txtMachineConfig.AcceptsReturn = true;
            txtMachineConfig.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtMachineConfig.Location = new Point(304, 46);
            txtMachineConfig.Margin = new Padding(3, 6, 10, 6);
            txtMachineConfig.MaxLength = 5000;
            txtMachineConfig.Multiline = true;
            txtMachineConfig.Name = "txtMachineConfig";
            txtMachineConfig.ScrollBars = ScrollBars.Vertical;
            txtMachineConfig.Size = new Size(535, 88);
            txtMachineConfig.TabIndex = 17;
            // 
            // lblMachineConfig
            // 
            lblMachineConfig.Anchor = AnchorStyles.Right;
            lblMachineConfig.AutoSize = true;
            lblMachineConfig.Location = new Point(90, 82);
            lblMachineConfig.Margin = new Padding(2, 0, 2, 0);
            lblMachineConfig.Name = "lblMachineConfig";
            lblMachineConfig.Size = new Size(209, 15);
            lblMachineConfig.TabIndex = 18;
            lblMachineConfig.Text = "Machine Configuration / Specification";
            lblMachineConfig.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblMachineModel
            // 
            lblMachineModel.Anchor = AnchorStyles.Right;
            lblMachineModel.AutoSize = true;
            lblMachineModel.Location = new Point(209, 15);
            lblMachineModel.Margin = new Padding(2, 0, 2, 0);
            lblMachineModel.Name = "lblMachineModel";
            lblMachineModel.Size = new Size(90, 15);
            lblMachineModel.TabIndex = 8;
            lblMachineModel.Text = "Machine Model";
            lblMachineModel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtMachineModel
            // 
            txtMachineModel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtMachineModel.Location = new Point(304, 12);
            txtMachineModel.Margin = new Padding(3, 6, 10, 6);
            txtMachineModel.Name = "txtMachineModel";
            txtMachineModel.Size = new Size(535, 23);
            txtMachineModel.TabIndex = 7;
            // 
            // lblControlPlatform
            // 
            lblControlPlatform.Anchor = AnchorStyles.Right;
            lblControlPlatform.AutoSize = true;
            lblControlPlatform.Location = new Point(203, 183);
            lblControlPlatform.Margin = new Padding(2, 0, 2, 0);
            lblControlPlatform.Name = "lblControlPlatform";
            lblControlPlatform.Size = new Size(96, 15);
            lblControlPlatform.TabIndex = 16;
            lblControlPlatform.Text = "Control Platform";
            lblControlPlatform.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtControlPlatform
            // 
            txtControlPlatform.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtControlPlatform.Location = new Point(304, 180);
            txtControlPlatform.Margin = new Padding(3, 6, 10, 6);
            txtControlPlatform.Name = "txtControlPlatform";
            txtControlPlatform.Size = new Size(535, 23);
            txtControlPlatform.TabIndex = 17;
            // 
            // txtMachineNumber
            // 
            txtMachineNumber.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtMachineNumber.Location = new Point(304, 146);
            txtMachineNumber.Margin = new Padding(3, 6, 10, 6);
            txtMachineNumber.Name = "txtMachineNumber";
            txtMachineNumber.Size = new Size(535, 23);
            txtMachineNumber.TabIndex = 11;
            // 
            // grpCustomerInfo
            // 
            grpCustomerInfo.Controls.Add(tblCustomerInfo);
            grpCustomerInfo.Dock = DockStyle.Top;
            grpCustomerInfo.Location = new Point(0, 226);
            grpCustomerInfo.Name = "grpCustomerInfo";
            grpCustomerInfo.Padding = new Padding(12);
            grpCustomerInfo.Size = new Size(867, 353);
            grpCustomerInfo.TabIndex = 17;
            grpCustomerInfo.TabStop = false;
            grpCustomerInfo.Text = "Customer Information";
            // 
            // tblCustomerInfo
            // 
            tblCustomerInfo.ColumnCount = 2;
            tblCustomerInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblCustomerInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tblCustomerInfo.Controls.Add(lblCustProjRef, 0, 6);
            tblCustomerInfo.Controls.Add(lblSiteContNumber, 0, 5);
            tblCustomerInfo.Controls.Add(lblSiteContEmail, 0, 4);
            tblCustomerInfo.Controls.Add(lblCustomerName, 0, 0);
            tblCustomerInfo.Controls.Add(lblCustomerAddress, 0, 1);
            tblCustomerInfo.Controls.Add(txtCustomerName, 1, 0);
            tblCustomerInfo.Controls.Add(txtCustomerLocation, 1, 1);
            tblCustomerInfo.Controls.Add(lblCustomerCountry, 0, 2);
            tblCustomerInfo.Controls.Add(txtCustomerCountry, 1, 2);
            tblCustomerInfo.Controls.Add(txtSiteContName, 1, 3);
            tblCustomerInfo.Controls.Add(txtSiteContEmail, 1, 4);
            tblCustomerInfo.Controls.Add(txtSiteContNumber, 1, 5);
            tblCustomerInfo.Controls.Add(txtCustProjRef, 1, 6);
            tblCustomerInfo.Controls.Add(lblSiteContName, 0, 3);
            tblCustomerInfo.Dock = DockStyle.Fill;
            tblCustomerInfo.Location = new Point(12, 28);
            tblCustomerInfo.Name = "tblCustomerInfo";
            tblCustomerInfo.Padding = new Padding(6);
            tblCustomerInfo.RowCount = 8;
            tblCustomerInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblCustomerInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tblCustomerInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblCustomerInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblCustomerInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblCustomerInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblCustomerInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblCustomerInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblCustomerInfo.Size = new Size(843, 313);
            tblCustomerInfo.TabIndex = 0;
            // 
            // lblCustProjRef
            // 
            lblCustProjRef.Anchor = AnchorStyles.Right;
            lblCustProjRef.AutoSize = true;
            lblCustProjRef.Location = new Point(49, 285);
            lblCustProjRef.Name = "lblCustProjRef";
            lblCustProjRef.Size = new Size(244, 15);
            lblCustProjRef.TabIndex = 16;
            lblCustProjRef.Text = "Customer Project Reference Name / Number";
            // 
            // lblSiteContNumber
            // 
            lblSiteContNumber.Anchor = AnchorStyles.Right;
            lblSiteContNumber.AutoSize = true;
            lblSiteContNumber.Location = new Point(175, 251);
            lblSiteContNumber.Name = "lblSiteContNumber";
            lblSiteContNumber.Size = new Size(118, 15);
            lblSiteContNumber.TabIndex = 15;
            lblSiteContNumber.Text = "Site Contact Number";
            // 
            // lblSiteContEmail
            // 
            lblSiteContEmail.Anchor = AnchorStyles.Right;
            lblSiteContEmail.AutoSize = true;
            lblSiteContEmail.Location = new Point(190, 217);
            lblSiteContEmail.Name = "lblSiteContEmail";
            lblSiteContEmail.Size = new Size(103, 15);
            lblSiteContEmail.TabIndex = 14;
            lblSiteContEmail.Text = "Site Contact Email";
            // 
            // lblCustomerName
            // 
            lblCustomerName.Anchor = AnchorStyles.Right;
            lblCustomerName.AutoSize = true;
            lblCustomerName.Location = new Point(200, 15);
            lblCustomerName.Margin = new Padding(2, 0, 2, 0);
            lblCustomerName.Name = "lblCustomerName";
            lblCustomerName.Size = new Size(94, 15);
            lblCustomerName.TabIndex = 1;
            lblCustomerName.Text = "Customer Name";
            lblCustomerName.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblCustomerAddress
            // 
            lblCustomerAddress.Anchor = AnchorStyles.Right;
            lblCustomerAddress.AutoSize = true;
            lblCustomerAddress.Location = new Point(190, 82);
            lblCustomerAddress.Margin = new Padding(2, 0, 2, 0);
            lblCustomerAddress.Name = "lblCustomerAddress";
            lblCustomerAddress.Size = new Size(104, 15);
            lblCustomerAddress.TabIndex = 6;
            lblCustomerAddress.Text = "Customer Address";
            lblCustomerAddress.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtCustomerName
            // 
            txtCustomerName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtCustomerName.Location = new Point(299, 12);
            txtCustomerName.Margin = new Padding(3, 6, 10, 6);
            txtCustomerName.Name = "txtCustomerName";
            txtCustomerName.Size = new Size(528, 23);
            txtCustomerName.TabIndex = 0;
            // 
            // txtCustomerLocation
            // 
            txtCustomerLocation.AcceptsReturn = true;
            txtCustomerLocation.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtCustomerLocation.Location = new Point(299, 46);
            txtCustomerLocation.Margin = new Padding(3, 6, 10, 6);
            txtCustomerLocation.MaxLength = 500;
            txtCustomerLocation.Multiline = true;
            txtCustomerLocation.Name = "txtCustomerLocation";
            txtCustomerLocation.ScrollBars = ScrollBars.Vertical;
            txtCustomerLocation.Size = new Size(528, 88);
            txtCustomerLocation.TabIndex = 5;
            // 
            // lblCustomerCountry
            // 
            lblCustomerCountry.Anchor = AnchorStyles.Right;
            lblCustomerCountry.AutoSize = true;
            lblCustomerCountry.Location = new Point(188, 149);
            lblCustomerCountry.Name = "lblCustomerCountry";
            lblCustomerCountry.Size = new Size(105, 15);
            lblCustomerCountry.TabIndex = 7;
            lblCustomerCountry.Text = "Customer Country";
            // 
            // txtCustomerCountry
            // 
            txtCustomerCountry.Dock = DockStyle.Fill;
            txtCustomerCountry.Location = new Point(299, 143);
            txtCustomerCountry.Name = "txtCustomerCountry";
            txtCustomerCountry.Size = new Size(535, 23);
            txtCustomerCountry.TabIndex = 8;
            // 
            // txtSiteContName
            // 
            txtSiteContName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSiteContName.Location = new Point(299, 179);
            txtSiteContName.Name = "txtSiteContName";
            txtSiteContName.Size = new Size(535, 23);
            txtSiteContName.TabIndex = 9;
            // 
            // txtSiteContEmail
            // 
            txtSiteContEmail.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSiteContEmail.Location = new Point(299, 213);
            txtSiteContEmail.Name = "txtSiteContEmail";
            txtSiteContEmail.Size = new Size(535, 23);
            txtSiteContEmail.TabIndex = 10;
            // 
            // txtSiteContNumber
            // 
            txtSiteContNumber.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSiteContNumber.Location = new Point(299, 247);
            txtSiteContNumber.Name = "txtSiteContNumber";
            txtSiteContNumber.Size = new Size(535, 23);
            txtSiteContNumber.TabIndex = 11;
            // 
            // txtCustProjRef
            // 
            txtCustProjRef.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtCustProjRef.Location = new Point(299, 281);
            txtCustProjRef.Name = "txtCustProjRef";
            txtCustProjRef.Size = new Size(535, 23);
            txtCustProjRef.TabIndex = 12;
            // 
            // lblSiteContName
            // 
            lblSiteContName.Anchor = AnchorStyles.Right;
            lblSiteContName.AutoSize = true;
            lblSiteContName.Location = new Point(187, 183);
            lblSiteContName.Name = "lblSiteContName";
            lblSiteContName.Size = new Size(106, 15);
            lblSiteContName.TabIndex = 13;
            lblSiteContName.Text = "Site Contact Name";
            // 
            // grpProjectInfo
            // 
            grpProjectInfo.Controls.Add(tblProjectInfo);
            grpProjectInfo.Dock = DockStyle.Top;
            grpProjectInfo.Location = new Point(0, 0);
            grpProjectInfo.Name = "grpProjectInfo";
            grpProjectInfo.Padding = new Padding(12);
            grpProjectInfo.Size = new Size(867, 226);
            grpProjectInfo.TabIndex = 19;
            grpProjectInfo.TabStop = false;
            grpProjectInfo.Text = "Project Information";
            // 
            // tblProjectInfo
            // 
            tblProjectInfo.ColumnCount = 2;
            tblProjectInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblProjectInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tblProjectInfo.Controls.Add(txtProjectScope, 1, 1);
            tblProjectInfo.Controls.Add(lblProjectScope, 0, 1);
            tblProjectInfo.Controls.Add(lblProjectType, 0, 0);
            tblProjectInfo.Controls.Add(comboBox1, 1, 0);
            tblProjectInfo.Controls.Add(lblStartDate, 0, 2);
            tblProjectInfo.Controls.Add(dateTimePicker1, 1, 2);
            tblProjectInfo.Dock = DockStyle.Fill;
            tblProjectInfo.Location = new Point(12, 28);
            tblProjectInfo.Name = "tblProjectInfo";
            tblProjectInfo.Padding = new Padding(6);
            tblProjectInfo.RowCount = 3;
            tblProjectInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblProjectInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tblProjectInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblProjectInfo.Size = new Size(843, 186);
            tblProjectInfo.TabIndex = 0;
            // 
            // txtProjectScope
            // 
            txtProjectScope.AcceptsReturn = true;
            txtProjectScope.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtProjectScope.Location = new Point(299, 46);
            txtProjectScope.Margin = new Padding(3, 6, 10, 6);
            txtProjectScope.MaxLength = 5000;
            txtProjectScope.Multiline = true;
            txtProjectScope.Name = "txtProjectScope";
            txtProjectScope.ScrollBars = ScrollBars.Vertical;
            txtProjectScope.Size = new Size(528, 88);
            txtProjectScope.TabIndex = 17;
            // 
            // lblProjectScope
            // 
            lblProjectScope.Anchor = AnchorStyles.Right;
            lblProjectScope.AutoSize = true;
            lblProjectScope.Location = new Point(161, 82);
            lblProjectScope.Margin = new Padding(2, 0, 2, 0);
            lblProjectScope.Name = "lblProjectScope";
            lblProjectScope.Size = new Size(133, 15);
            lblProjectScope.TabIndex = 18;
            lblProjectScope.Text = "Project Scope Summary";
            lblProjectScope.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblProjectType
            // 
            lblProjectType.Anchor = AnchorStyles.Right;
            lblProjectType.AutoSize = true;
            lblProjectType.Location = new Point(222, 15);
            lblProjectType.Margin = new Padding(2, 0, 2, 0);
            lblProjectType.Name = "lblProjectType";
            lblProjectType.Size = new Size(72, 15);
            lblProjectType.TabIndex = 8;
            lblProjectType.Text = "Project Type";
            lblProjectType.TextAlign = ContentAlignment.MiddleRight;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(299, 9);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 23);
            comboBox1.TabIndex = 19;
            // 
            // lblStartDate
            // 
            lblStartDate.Anchor = AnchorStyles.Right;
            lblStartDate.AutoSize = true;
            lblStartDate.Location = new Point(196, 152);
            lblStartDate.Margin = new Padding(2, 0, 2, 0);
            lblStartDate.Name = "lblStartDate";
            lblStartDate.Size = new Size(98, 15);
            lblStartDate.TabIndex = 15;
            lblStartDate.Text = "Project Start Date";
            lblStartDate.TextAlign = ContentAlignment.MiddleRight;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            dateTimePicker1.Location = new Point(298, 148);
            dateTimePicker1.Margin = new Padding(2);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(537, 23);
            dateTimePicker1.TabIndex = 14;
            // 
            // NewProjectForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 1048);
            Controls.Add(pnlNewProjectBase);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Location = new Point(108, 46);
            Margin = new Padding(2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "NewProjectForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NewProjectForm.cs";
            Load += NewProjectForm_Load;
            pnlNewProjectTitle.ResumeLayout(false);
            pnlNewProjectTitle.PerformLayout();
            pnlNewProjectBase.ResumeLayout(false);
            pnlNPFFooterActions.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            grpRolesResponsibilities.ResumeLayout(false);
            tblRolesResponsibilities.ResumeLayout(false);
            tblRolesResponsibilities.PerformLayout();
            grpMachineInfo.ResumeLayout(false);
            tblMachineInfo.ResumeLayout(false);
            tblMachineInfo.PerformLayout();
            grpCustomerInfo.ResumeLayout(false);
            tblCustomerInfo.ResumeLayout(false);
            tblCustomerInfo.PerformLayout();
            grpProjectInfo.ResumeLayout(false);
            tblProjectInfo.ResumeLayout(false);
            tblProjectInfo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitle;
        private Panel pnlNewProjectTitle;
        private Label lblTitleNewProject;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Panel pnlNewProjectBase;
        private Panel pnlContent;
        private GroupBox grpMachineInfo;
        private TableLayoutPanel tblMachineInfo;
        private Label lblMachineRole;
        private TextBox txtMachineRole;
        private TextBox txtMachineConfig;
        private Label lblMachineConfig;
        private Label lblMachineModel;
        private TextBox txtMachineModel;
        private Label lblControlPlatform;
        private TextBox txtControlPlatform;
        private Label lblMachineNumber;
        private TextBox txtMachineNumber;
        private GroupBox grpCustomerInfo;
        private TableLayoutPanel tblCustomerInfo;
        private Label lblCustProjRef;
        private Label lblSiteContNumber;
        private Label lblSiteContEmail;
        private Label lblCustomerName;
        private Label lblCustomerAddress;
        private TextBox txtCustomerName;
        private TextBox txtCustomerLocation;
        private Label lblCustomerCountry;
        private TextBox txtCustomerCountry;
        private TextBox txtSiteContName;
        private TextBox txtSiteContEmail;
        private TextBox txtSiteContNumber;
        private TextBox txtCustProjRef;
        private Label lblSiteContName;
        private GroupBox grpProjectInfo;
        private TableLayoutPanel tblProjectInfo;
        private TextBox txtProjectScope;
        private Label lblProjectScope;
        private Label lblProjectType;
        private ComboBox comboBox1;
        private Label lblStartDate;
        private DateTimePicker dateTimePicker1;
        private GroupBox grpRolesResponsibilities;
        private TableLayoutPanel tblRolesResponsibilities;
        private Label lblLeadEngineerEmail;
        private TextBox txtLeadEngineerEmail;
        private TextBox txtLeadTestEng;
        private Label lblLeadTestEng;
        private Panel pnlNPFFooterActions;
        private Button btnNPFCancel;
        private Button btnBuildContract;
        private Label lblLeadEngineerPhone;
        private TextBox txtLeadEngineerPhone;
        private Panel pnlNPFScrollSpacer;
    }
}
