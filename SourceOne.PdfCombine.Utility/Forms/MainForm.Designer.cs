namespace SourceOne.PdfCombine.Utility.Forms
{
    partial class MainForm
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
            lblCompany = new Label();
            cboCompany = new ComboBox();
            lblBeginDate = new Label();
            dtpBeginDate = new DateTimePicker();
            lblEndDate = new Label();
            dtpEndDate = new DateTimePicker();
            btnRetrieveRecords = new Button();
            btnCombinePdfs = new Button();
            btnDownload = new Button();
            btnExportToExcel = new Button();
            lblVendorGroup = new Label();
            cboVendorGroup = new ComboBox();
            lblVendor = new Label();
            clbVendor = new CheckedListBox();
            chkSelectAllVendors = new CheckBox();
            lblJob = new Label();
            clbJob = new CheckedListBox();
            chkSelectAllJobs = new CheckBox();
            lblGLAccount = new Label();
            txtGLAccount = new TextBox();
            lblDateType = new Label();
            cboDateType = new ComboBox();
            dgvRecords = new DataGridView();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            progressBar = new ToolStripProgressBar();
            lblCombinedFileLink = new ToolStripStatusLabel();
            lblRecordCount = new ToolStripStatusLabel();
            grpParameters = new GroupBox();
            grpRecords = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            statusStrip.SuspendLayout();
            grpParameters.SuspendLayout();
            grpRecords.SuspendLayout();
            SuspendLayout();
            // 
            // lblCompany
            // 
            lblCompany.AutoSize = true;
            lblCompany.Location = new Point(20, 35);
            lblCompany.Name = "lblCompany";
            lblCompany.Size = new Size(75, 20);
            lblCompany.TabIndex = 0;
            lblCompany.Text = "Company:";
            // 
            // cboCompany
            // 
            cboCompany.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCompany.FormattingEnabled = true;
            cboCompany.Location = new Point(120, 32);
            cboCompany.Name = "cboCompany";
            cboCompany.Size = new Size(300, 28);
            cboCompany.TabIndex = 1;
            cboCompany.SelectedIndexChanged += cboCompany_SelectedIndexChanged;
            // 
            // lblBeginDate
            // 
            lblBeginDate.AutoSize = true;
            lblBeginDate.Location = new Point(659, 146);
            lblBeginDate.Name = "lblBeginDate";
            lblBeginDate.Size = new Size(86, 20);
            lblBeginDate.TabIndex = 2;
            lblBeginDate.Text = "Begin Date:";
            // 
            // dtpBeginDate
            // 
            dtpBeginDate.Format = DateTimePickerFormat.Short;
            dtpBeginDate.Location = new Point(759, 143);
            dtpBeginDate.Name = "dtpBeginDate";
            dtpBeginDate.Size = new Size(150, 27);
            dtpBeginDate.TabIndex = 3;
            // 
            // lblEndDate
            // 
            lblEndDate.AutoSize = true;
            lblEndDate.Location = new Point(939, 146);
            lblEndDate.Name = "lblEndDate";
            lblEndDate.Size = new Size(73, 20);
            lblEndDate.TabIndex = 4;
            lblEndDate.Text = "End Date:";
            // 
            // dtpEndDate
            // 
            dtpEndDate.Format = DateTimePickerFormat.Short;
            dtpEndDate.Location = new Point(1019, 143);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(150, 27);
            dtpEndDate.TabIndex = 5;
            // 
            // btnRetrieveRecords
            // 
            btnRetrieveRecords.Location = new Point(198, 310);
            btnRetrieveRecords.Name = "btnRetrieveRecords";
            btnRetrieveRecords.Size = new Size(200, 69);
            btnRetrieveRecords.TabIndex = 6;
            btnRetrieveRecords.Text = "Retrieve && Process Records";
            btnRetrieveRecords.UseVisualStyleBackColor = true;
            btnRetrieveRecords.Click += btnRetrieveRecords_Click;
            // 
            // btnCombinePdfs
            // 
            btnCombinePdfs.Enabled = false;
            btnCombinePdfs.Location = new Point(453, 310);
            btnCombinePdfs.Name = "btnCombinePdfs";
            btnCombinePdfs.Size = new Size(200, 69);
            btnCombinePdfs.TabIndex = 7;
            btnCombinePdfs.Text = "Combine PDFs";
            btnCombinePdfs.UseVisualStyleBackColor = true;
            btnCombinePdfs.Click += btnCombinePdfs_Click;
            // 
            // btnDownload
            // 
            btnDownload.Enabled = false;
            btnDownload.Location = new Point(722, 310);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(200, 69);
            btnDownload.TabIndex = 8;
            btnDownload.Text = "Download PDFs";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // btnExportToExcel
            // 
            btnExportToExcel.Enabled = false;
            btnExportToExcel.Location = new Point(986, 310);
            btnExportToExcel.Name = "btnExportToExcel";
            btnExportToExcel.Size = new Size(200, 69);
            btnExportToExcel.TabIndex = 25;
            btnExportToExcel.Text = "Export to Excel";
            btnExportToExcel.UseVisualStyleBackColor = true;
            btnExportToExcel.Click += btnExportToExcel_Click;
            // 
            // lblVendorGroup
            // 
            lblVendorGroup.AutoSize = true;
            lblVendorGroup.Location = new Point(20, 75);
            lblVendorGroup.Name = "lblVendorGroup";
            lblVendorGroup.Size = new Size(104, 20);
            lblVendorGroup.TabIndex = 9;
            lblVendorGroup.Text = "Vendor Group:";
            // 
            // cboVendorGroup
            // 
            cboVendorGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            cboVendorGroup.FormattingEnabled = true;
            cboVendorGroup.Location = new Point(120, 72);
            cboVendorGroup.Name = "cboVendorGroup";
            cboVendorGroup.Size = new Size(300, 28);
            cboVendorGroup.TabIndex = 10;
            cboVendorGroup.SelectedIndexChanged += cboVendorGroup_SelectedIndexChanged;
            // 
            // lblVendor
            // 
            lblVendor.AutoSize = true;
            lblVendor.Location = new Point(20, 115);
            lblVendor.Name = "lblVendor";
            lblVendor.Size = new Size(59, 20);
            lblVendor.TabIndex = 11;
            lblVendor.Text = "Vendor:";
            // 
            // clbVendor
            // 
            clbVendor.CheckOnClick = true;
            clbVendor.FormattingEnabled = true;
            clbVendor.Location = new Point(120, 112);
            clbVendor.Name = "clbVendor";
            clbVendor.Size = new Size(300, 70);
            clbVendor.TabIndex = 12;
            clbVendor.ItemCheck += clbVendor_ItemCheck;
            // 
            // chkSelectAllVendors
            // 
            chkSelectAllVendors.AutoSize = true;
            chkSelectAllVendors.Location = new Point(430, 115);
            chkSelectAllVendors.Name = "chkSelectAllVendors";
            chkSelectAllVendors.Size = new Size(93, 24);
            chkSelectAllVendors.TabIndex = 23;
            chkSelectAllVendors.Text = "Select All";
            chkSelectAllVendors.UseVisualStyleBackColor = true;
            chkSelectAllVendors.CheckedChanged += chkSelectAllVendors_CheckedChanged;
            // 
            // lblJob
            // 
            lblJob.AutoSize = true;
            lblJob.Location = new Point(20, 205);
            lblJob.Name = "lblJob";
            lblJob.Size = new Size(35, 20);
            lblJob.TabIndex = 13;
            lblJob.Text = "Job:";
            // 
            // clbJob
            // 
            clbJob.CheckOnClick = true;
            clbJob.FormattingEnabled = true;
            clbJob.Location = new Point(120, 202);
            clbJob.Name = "clbJob";
            clbJob.Size = new Size(300, 70);
            clbJob.TabIndex = 14;
            // 
            // chkSelectAllJobs
            // 
            chkSelectAllJobs.AutoSize = true;
            chkSelectAllJobs.Location = new Point(430, 205);
            chkSelectAllJobs.Name = "chkSelectAllJobs";
            chkSelectAllJobs.Size = new Size(93, 24);
            chkSelectAllJobs.TabIndex = 24;
            chkSelectAllJobs.Text = "Select All";
            chkSelectAllJobs.UseVisualStyleBackColor = true;
            chkSelectAllJobs.CheckedChanged += chkSelectAllJobs_CheckedChanged;
            // 
            // lblGLAccount
            // 
            lblGLAccount.AutoSize = true;
            lblGLAccount.Location = new Point(659, 38);
            lblGLAccount.Name = "lblGLAccount";
            lblGLAccount.Size = new Size(87, 20);
            lblGLAccount.TabIndex = 21;
            lblGLAccount.Text = "GL Account:";
            // 
            // txtGLAccount
            // 
            txtGLAccount.Location = new Point(759, 35);
            txtGLAccount.Name = "txtGLAccount";
            txtGLAccount.Size = new Size(300, 27);
            txtGLAccount.TabIndex = 22;
            // 
            // lblDateType
            // 
            lblDateType.AutoSize = true;
            lblDateType.Location = new Point(659, 97);
            lblDateType.Name = "lblDateType";
            lblDateType.Size = new Size(79, 20);
            lblDateType.TabIndex = 15;
            lblDateType.Text = "Date Type:";
            // 
            // cboDateType
            // 
            cboDateType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDateType.FormattingEnabled = true;
            cboDateType.Location = new Point(759, 94);
            cboDateType.Name = "cboDateType";
            cboDateType.Size = new Size(300, 28);
            cboDateType.TabIndex = 16;
            // 
            // dgvRecords
            // 
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRecords.Dock = DockStyle.Fill;
            dgvRecords.Location = new Point(3, 23);
            dgvRecords.Name = "dgvRecords";
            dgvRecords.ReadOnly = true;
            dgvRecords.RowHeadersWidth = 51;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.Size = new Size(1394, 463);
            dgvRecords.TabIndex = 17;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, progressBar, lblCombinedFileLink, lblRecordCount });
            statusStrip.Location = new Point(0, 878);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1400, 26);
            statusStrip.TabIndex = 18;
            statusStrip.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1271, 20);
            lblStatus.Spring = true;
            lblStatus.Text = "Ready";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(200, 18);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = false;
            // 
            // lblCombinedFileLink
            // 
            lblCombinedFileLink.IsLink = true;
            lblCombinedFileLink.Name = "lblCombinedFileLink";
            lblCombinedFileLink.Size = new Size(0, 20);
            lblCombinedFileLink.Visible = false;
            lblCombinedFileLink.Click += lblCombinedFileLink_Click;
            // 
            // lblRecordCount
            // 
            lblRecordCount.Name = "lblRecordCount";
            lblRecordCount.Size = new Size(114, 20);
            lblRecordCount.Text = "Total Records: 0";
            lblRecordCount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // grpParameters
            // 
            grpParameters.Controls.Add(lblCompany);
            grpParameters.Controls.Add(cboCompany);
            grpParameters.Controls.Add(lblVendorGroup);
            grpParameters.Controls.Add(cboVendorGroup);
            grpParameters.Controls.Add(lblVendor);
            grpParameters.Controls.Add(clbVendor);
            grpParameters.Controls.Add(chkSelectAllVendors);
            grpParameters.Controls.Add(lblJob);
            grpParameters.Controls.Add(clbJob);
            grpParameters.Controls.Add(chkSelectAllJobs);
            grpParameters.Controls.Add(lblGLAccount);
            grpParameters.Controls.Add(txtGLAccount);
            grpParameters.Controls.Add(lblDateType);
            grpParameters.Controls.Add(cboDateType);
            grpParameters.Controls.Add(lblBeginDate);
            grpParameters.Controls.Add(btnDownload);
            grpParameters.Controls.Add(btnExportToExcel);
            grpParameters.Controls.Add(btnCombinePdfs);
            grpParameters.Controls.Add(dtpBeginDate);
            grpParameters.Controls.Add(btnRetrieveRecords);
            grpParameters.Controls.Add(lblEndDate);
            grpParameters.Controls.Add(dtpEndDate);
            grpParameters.Dock = DockStyle.Top;
            grpParameters.Location = new Point(0, 0);
            grpParameters.Name = "grpParameters";
            grpParameters.Size = new Size(1400, 389);
            grpParameters.TabIndex = 19;
            grpParameters.TabStop = false;
            grpParameters.Text = "Parameters";
            grpParameters.Enter += grpParameters_Enter;
            // 
            // grpRecords
            // 
            grpRecords.Controls.Add(dgvRecords);
            grpRecords.Dock = DockStyle.Fill;
            grpRecords.Location = new Point(0, 389);
            grpRecords.Name = "grpRecords";
            grpRecords.Size = new Size(1400, 489);
            grpRecords.TabIndex = 20;
            grpRecords.TabStop = false;
            grpRecords.Text = "Unallocated PDF Records";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 904);
            Controls.Add(grpRecords);
            Controls.Add(grpParameters);
            Controls.Add(statusStrip);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PDF Combine Utility";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvRecords).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            grpParameters.ResumeLayout(false);
            grpParameters.PerformLayout();
            grpRecords.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCompany;
        private System.Windows.Forms.ComboBox cboCompany;
        private System.Windows.Forms.Label lblBeginDate;
        private System.Windows.Forms.DateTimePicker dtpBeginDate;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Button btnRetrieveRecords;
        private System.Windows.Forms.Button btnCombinePdfs;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnExportToExcel;
        private System.Windows.Forms.Label lblVendorGroup;
        private System.Windows.Forms.ComboBox cboVendorGroup;
        private System.Windows.Forms.Label lblVendor;
        private System.Windows.Forms.CheckedListBox clbVendor;
        private System.Windows.Forms.CheckBox chkSelectAllVendors;
        private System.Windows.Forms.Label lblJob;
        private System.Windows.Forms.CheckedListBox clbJob;
        private System.Windows.Forms.CheckBox chkSelectAllJobs;
        private System.Windows.Forms.Label lblGLAccount;
        private System.Windows.Forms.TextBox txtGLAccount;
        private System.Windows.Forms.Label lblDateType;
        private System.Windows.Forms.ComboBox cboDateType;
        private System.Windows.Forms.DataGridView dgvRecords;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripStatusLabel lblCombinedFileLink;
        private System.Windows.Forms.ToolStripStatusLabel lblRecordCount;
        private System.Windows.Forms.GroupBox grpParameters;
        private System.Windows.Forms.GroupBox grpRecords;
    }
}
