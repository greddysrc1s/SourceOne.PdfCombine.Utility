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
            btnExportToCsv = new Button();
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
            // 
            // lblBeginDate
            // 
            lblBeginDate.AutoSize = true;
            lblBeginDate.Location = new Point(20, 75);
            lblBeginDate.Name = "lblBeginDate";
            lblBeginDate.Size = new Size(86, 20);
            lblBeginDate.TabIndex = 2;
            lblBeginDate.Text = "Begin Date:";
            // 
            // dtpBeginDate
            // 
            dtpBeginDate.Format = DateTimePickerFormat.Short;
            dtpBeginDate.Location = new Point(120, 72);
            dtpBeginDate.Name = "dtpBeginDate";
            dtpBeginDate.Size = new Size(150, 27);
            dtpBeginDate.TabIndex = 3;
            // 
            // lblEndDate
            // 
            lblEndDate.AutoSize = true;
            lblEndDate.Location = new Point(300, 75);
            lblEndDate.Name = "lblEndDate";
            lblEndDate.Size = new Size(73, 20);
            lblEndDate.TabIndex = 4;
            lblEndDate.Text = "End Date:";
            // 
            // dtpEndDate
            // 
            dtpEndDate.Format = DateTimePickerFormat.Short;
            dtpEndDate.Location = new Point(380, 72);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(150, 27);
            dtpEndDate.TabIndex = 5;
            // 
            // btnRetrieveRecords
            // 
            btnRetrieveRecords.Location = new Point(560, 32);
            btnRetrieveRecords.Name = "btnRetrieveRecords";
            btnRetrieveRecords.Size = new Size(200, 67);
            btnRetrieveRecords.TabIndex = 6;
            btnRetrieveRecords.Text = "Retrieve && Process Records";
            btnRetrieveRecords.UseVisualStyleBackColor = true;
            btnRetrieveRecords.Click += btnRetrieveRecords_Click;
            // 
            // btnCombinePdfs
            // 
            btnCombinePdfs.Enabled = false;
            btnCombinePdfs.Location = new Point(780, 32);
            btnCombinePdfs.Name = "btnCombinePdfs";
            btnCombinePdfs.Size = new Size(200, 67);
            btnCombinePdfs.TabIndex = 7;
            btnCombinePdfs.Text = "Combine PDFs";
            btnCombinePdfs.UseVisualStyleBackColor = true;
            btnCombinePdfs.Click += btnCombinePdfs_Click;
            // 
            // btnExportToCsv
            // 
            btnExportToCsv.Enabled = false;
            btnExportToCsv.Location = new Point(1000, 32);
            btnExportToCsv.Name = "btnExportToCsv";
            btnExportToCsv.Size = new Size(200, 67);
            btnExportToCsv.TabIndex = 8;
            btnExportToCsv.Text = "Export to CSV";
            btnExportToCsv.UseVisualStyleBackColor = true;
            btnExportToCsv.Click += btnExportToCsv_Click;
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
            dgvRecords.Size = new Size(1394, 572);
            dgvRecords.TabIndex = 9;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, progressBar, lblCombinedFileLink, lblRecordCount });
            statusStrip.Location = new Point(0, 718);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1400, 26);
            statusStrip.TabIndex = 10;
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
            grpParameters.Controls.Add(lblBeginDate);
            grpParameters.Controls.Add(btnExportToCsv);
            grpParameters.Controls.Add(btnCombinePdfs);
            grpParameters.Controls.Add(dtpBeginDate);
            grpParameters.Controls.Add(btnRetrieveRecords);
            grpParameters.Controls.Add(lblEndDate);
            grpParameters.Controls.Add(dtpEndDate);
            grpParameters.Dock = DockStyle.Top;
            grpParameters.Location = new Point(0, 0);
            grpParameters.Name = "grpParameters";
            grpParameters.Size = new Size(1400, 120);
            grpParameters.TabIndex = 11;
            grpParameters.TabStop = false;
            grpParameters.Text = "Parameters";
            grpParameters.Enter += grpParameters_Enter;
            // 
            // grpRecords
            // 
            grpRecords.Controls.Add(dgvRecords);
            grpRecords.Dock = DockStyle.Fill;
            grpRecords.Location = new Point(0, 120);
            grpRecords.Name = "grpRecords";
            grpRecords.Size = new Size(1400, 598);
            grpRecords.TabIndex = 12;
            grpRecords.TabStop = false;
            grpRecords.Text = "Unallocated PDF Records";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 744);
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
        private System.Windows.Forms.Button btnExportToCsv;
        private System.Windows.Forms.DataGridView dgvRecords;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripStatusLabel lblRecordCount;
        private System.Windows.Forms.ToolStripStatusLabel lblCombinedFileLink;
        private System.Windows.Forms.GroupBox grpParameters;
        private System.Windows.Forms.GroupBox grpRecords;
    }
}
