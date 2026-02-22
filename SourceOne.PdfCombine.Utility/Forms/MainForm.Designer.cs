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
            this.lblCompany = new System.Windows.Forms.Label();
            this.txtCompany = new System.Windows.Forms.TextBox();
            this.lblBeginDate = new System.Windows.Forms.Label();
            this.dtpBeginDate = new System.Windows.Forms.DateTimePicker();
            this.lblEndDate = new System.Windows.Forms.Label();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.btnRetrieveRecords = new System.Windows.Forms.Button();
            this.btnCombinePdfs = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.grpParameters = new System.Windows.Forms.GroupBox();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.statusStrip.SuspendLayout();
            this.grpParameters.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCompany
            // 
            this.lblCompany.AutoSize = true;
            this.lblCompany.Location = new System.Drawing.Point(20, 35);
            this.lblCompany.Name = "lblCompany";
            this.lblCompany.Size = new System.Drawing.Size(72, 20);
            this.lblCompany.TabIndex = 0;
            this.lblCompany.Text = "Company:";
            // 
            // txtCompany
            // 
            this.txtCompany.Location = new System.Drawing.Point(120, 32);
            this.txtCompany.Name = "txtCompany";
            this.txtCompany.Size = new System.Drawing.Size(150, 27);
            this.txtCompany.TabIndex = 1;
            // 
            // lblBeginDate
            // 
            this.lblBeginDate.AutoSize = true;
            this.lblBeginDate.Location = new System.Drawing.Point(20, 75);
            this.lblBeginDate.Name = "lblBeginDate";
            this.lblBeginDate.Size = new System.Drawing.Size(83, 20);
            this.lblBeginDate.TabIndex = 2;
            this.lblBeginDate.Text = "Begin Date:";
            // 
            // dtpBeginDate
            // 
            this.dtpBeginDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpBeginDate.Location = new System.Drawing.Point(120, 72);
            this.dtpBeginDate.Name = "dtpBeginDate";
            this.dtpBeginDate.Size = new System.Drawing.Size(150, 27);
            this.dtpBeginDate.TabIndex = 3;
            // 
            // lblEndDate
            // 
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Location = new System.Drawing.Point(300, 75);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new System.Drawing.Size(70, 20);
            this.lblEndDate.TabIndex = 4;
            this.lblEndDate.Text = "End Date:";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEndDate.Location = new System.Drawing.Point(380, 72);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(150, 27);
            this.dtpEndDate.TabIndex = 5;
            // 
            // btnRetrieveRecords
            // 
            this.btnRetrieveRecords.Location = new System.Drawing.Point(560, 32);
            this.btnRetrieveRecords.Name = "btnRetrieveRecords";
            this.btnRetrieveRecords.Size = new System.Drawing.Size(200, 67);
            this.btnRetrieveRecords.TabIndex = 6;
            this.btnRetrieveRecords.Text = "Retrieve && Display Records";
            this.btnRetrieveRecords.UseVisualStyleBackColor = true;
            this.btnRetrieveRecords.Click += new System.EventHandler(this.btnRetrieveRecords_Click);
            // 
            // btnCombinePdfs
            // 
            this.btnCombinePdfs.Enabled = false;
            this.btnCombinePdfs.Location = new System.Drawing.Point(780, 32);
            this.btnCombinePdfs.Name = "btnCombinePdfs";
            this.btnCombinePdfs.Size = new System.Drawing.Size(200, 67);
            this.btnCombinePdfs.TabIndex = 7;
            this.btnCombinePdfs.Text = "Combine PDFs";
            this.btnCombinePdfs.UseVisualStyleBackColor = true;
            this.btnCombinePdfs.Click += new System.EventHandler(this.btnCombinePdfs_Click);
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.Location = new System.Drawing.Point(3, 23);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(994, 394);
            this.txtLog.TabIndex = 8;
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.progressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 568);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1024, 26);
            this.statusStrip.TabIndex = 9;
            this.statusStrip.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(807, 20);
            this.lblStatus.Spring = true;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(200, 18);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.Visible = false;
            // 
            // grpParameters
            // 
            this.grpParameters.Controls.Add(this.lblCompany);
            this.grpParameters.Controls.Add(this.txtCompany);
            this.grpParameters.Controls.Add(this.lblBeginDate);
            this.grpParameters.Controls.Add(this.btnCombinePdfs);
            this.grpParameters.Controls.Add(this.dtpBeginDate);
            this.grpParameters.Controls.Add(this.btnRetrieveRecords);
            this.grpParameters.Controls.Add(this.lblEndDate);
            this.grpParameters.Controls.Add(this.dtpEndDate);
            this.grpParameters.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpParameters.Location = new System.Drawing.Point(0, 0);
            this.grpParameters.Name = "grpParameters";
            this.grpParameters.Size = new System.Drawing.Size(1024, 120);
            this.grpParameters.TabIndex = 10;
            this.grpParameters.TabStop = false;
            this.grpParameters.Text = "Parameters";
            // 
            // grpLog
            // 
            this.grpLog.Controls.Add(this.txtLog);
            this.grpLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLog.Location = new System.Drawing.Point(0, 120);
            this.grpLog.Name = "grpLog";
            this.grpLog.Size = new System.Drawing.Size(1000, 420);
            this.grpLog.TabIndex = 11;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Log";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 594);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.grpParameters);
            this.Controls.Add(this.statusStrip);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PDF Combine Utility";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.grpParameters.ResumeLayout(false);
            this.grpParameters.PerformLayout();
            this.grpLog.ResumeLayout(false);
            this.grpLog.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCompany;
        private System.Windows.Forms.TextBox txtCompany;
        private System.Windows.Forms.Label lblBeginDate;
        private System.Windows.Forms.DateTimePicker dtpBeginDate;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Button btnRetrieveRecords;
        private System.Windows.Forms.Button btnCombinePdfs;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.GroupBox grpParameters;
        private System.Windows.Forms.GroupBox grpLog;
    }
}
