using System.Text;
using SourceOne.PdfCombine.Utility.Models;

namespace SourceOne.PdfCombine.Utility.Forms
{
    public partial class DisplayAttachmentForm : Form
    {
        private readonly List<UnallocatedPdfRecord> _records;

        public DisplayAttachmentForm(List<UnallocatedPdfRecord> records)
        {
            InitializeComponent();
            _records = records ?? new List<UnallocatedPdfRecord>();
            LoadRecords();
        }

        private void LoadRecords()
        {
            // Set up DataGridView
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.Columns.Clear();

            // Add columns
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Vendor",
                DataPropertyName = "Vendor",
                Width = 80
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Vendor Name",
                DataPropertyName = "Name",
                Width = 200
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Invoice",
                DataPropertyName = "Invoice",
                Width = 120
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Invoice Date",
                DataPropertyName = "Invoice_Date",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" }
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Job",
                DataPropertyName = "Job",
                Width = 100
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Expense Account",
                DataPropertyName = "Expense_Account",
                Width = 120
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Amount",
                DataPropertyName = "Amount",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Item",
                DataPropertyName = "Item",
                Width = 200
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Attachment ID",
                DataPropertyName = "UniqueAttchID",
                Width = 250
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "File Name",
                DataPropertyName = "OrigFileName",
                Width = 200
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Add Date",
                DataPropertyName = "AddDate",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" }
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Date Entered",
                DataPropertyName = "DateEntered",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" }
            });

            // Bind data
            dgvRecords.DataSource = _records;

            // Update record count
            lblRecordCount.Text = $"Total Records: {_records.Count}";

            // Apply formatting
            dgvRecords.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.Navy;
            dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvRecords.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    saveFileDialog.Title = "Export to CSV";
                    saveFileDialog.FileName = $"UnallocatedPdfRecords_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportToCsv(saveFileDialog.FileName);
                        MessageBox.Show($"Data exported successfully to:\n{saveFileDialog.FileName}", 
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv(string filePath)
        {
            var sb = new StringBuilder();

            // Add header
            sb.AppendLine("Vendor,Vendor Name,Invoice,Invoice Date,Job,Expense Account,Amount,Item,Attachment ID,File Name,Add Date,Date Entered");

            // Add data rows
            foreach (var record in _records)
            {
                sb.AppendLine($"{EscapeCsvField(record.Vendor.ToString())}," +
                             $"{EscapeCsvField(record.Name)}," +
                             $"{EscapeCsvField(record.Invoice)}," +
                             $"{EscapeCsvField(record.Invoice_Date?.ToString("yyyy-MM-dd"))}," +
                             $"{EscapeCsvField(record.Job)}," +
                             $"{EscapeCsvField(record.Expense_Account)}," +
                             $"{EscapeCsvField(record.Amount.ToString("F2"))}," +
                             $"{EscapeCsvField(record.Item)}," +
                             $"{EscapeCsvField(record.UniqueAttchID?.ToString())}," +
                             $"{EscapeCsvField(record.OrigFileName)}," +
                             $"{EscapeCsvField(record.AddDate?.ToString("yyyy-MM-dd"))}," +
                             $"{EscapeCsvField(record.DateEntered?.ToString("yyyy-MM-dd HH:mm:ss"))}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // If field contains comma, quote, or newline, wrap it in quotes and escape quotes
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }
    }
}
