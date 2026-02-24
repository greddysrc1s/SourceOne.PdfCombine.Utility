using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using SourceOne.PdfCombine.Utility.Configuration;
using SourceOne.PdfCombine.Utility.Models;
using SourceOne.PdfCombine.Utility.Services;

namespace SourceOne.PdfCombine.Utility.Forms
{
    public partial class MainForm : Form
    {
        private readonly IConfiguration _configuration;
        private readonly FileStorageSettings _fileStorageSettings;
        private PdfQueryService? _pdfQueryService;
        private FileStorageService? _fileStorageService;
        private PdfCombineServicePdfSharp? _pdfCombineService;
        private List<UnallocatedPdfRecord>? _records;
        private int _savedFileCount = 0;
        private Dictionary<Guid, string> _savedFilePaths = new Dictionary<Guid, string>(); // Track saved file paths
        private string? _combinedFilePath; // Track the combined PDF file path
        private List<Company>? _companies; // Track available companies

        public MainForm()
        {
            InitializeComponent();

            // Build configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Load file storage settings
            _fileStorageSettings = new FileStorageSettings();
            _configuration.GetSection("FileStorage").Bind(_fileStorageSettings);

            // Set default dates - Begin date is 7 days ago, End date is today
            dtpBeginDate.Value = DateTime.Now.AddDays(-7);
            dtpEndDate.Value = DateTime.Now;

            // Initialize DataGridView
            InitializeDataGridView();

            Log.Information("Application initialized successfully");
            Log.Information($"Temporary file path: {_fileStorageSettings.GetFullPath()}");
            Log.Information($"Output file path: {_fileStorageSettings.GetOutputPath()}");

            SetStatus("Ready", false);
        }

        private void InitializeDataGridView()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.Columns.Clear();

            // Add View button column first
            var viewButtonColumn = new DataGridViewButtonColumn
            {
                HeaderText = "View",
                Text = "View PDF",
                UseColumnTextForButtonValue = true,
                Width = 80,
                Name = "ViewColumn"
            };
            dgvRecords.Columns.Add(viewButtonColumn);

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

            // Apply formatting
            dgvRecords.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.Navy;
            dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvRecords.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // Add cell click event handler for View button
            dgvRecords.CellContentClick += DgvRecords_CellContentClick;
        }

        private void DgvRecords_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            // Check if the click is on the View button column
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvRecords.Columns["ViewColumn"]!.Index)
            {
                var record = dgvRecords.Rows[e.RowIndex].DataBoundItem as UnallocatedPdfRecord;
                if (record != null && record.UniqueAttchID.HasValue)
                {
                    OpenPdfFile(record.UniqueAttchID.Value, record.OrigFileName);
                }
            }
        }

        private void OpenPdfFile(Guid attachmentId, string? originalFileName)
        {
            try
            {
                // Check if the file has been saved and we have its path
                if (_savedFilePaths.TryGetValue(attachmentId, out string? filePath))
                {
                    if (File.Exists(filePath))
                    {
                        Log.Information($"Opening PDF file: {filePath}");
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show($"PDF file not found at: {filePath}\n\nThe file may have been moved or deleted.",
                            "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Log.Warning($"PDF file not found: {filePath}");
                    }
                }
                else
                {
                    MessageBox.Show($"PDF file has not been downloaded yet.\n\nFile: {originalFileName ?? "Unknown"}\n\nPlease wait for the processing to complete.",
                        "File Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log.Information($"Attempted to open PDF that hasn't been downloaded: {attachmentId}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening PDF file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex, $"Error opening PDF file for attachment: {attachmentId}");
            }
        }

        private void SetStatus(string message, bool showProgress = false)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetStatus(message, showProgress));
                return;
            }

            lblStatus.Text = message;
            progressBar.Visible = showProgress;
            Application.DoEvents();
        }

        private void UpdateRecordCount(int count)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateRecordCount(count));
                return;
            }

            lblRecordCount.Text = $"Total Records: {count}";
        }

        private async void btnRetrieveRecords_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (cboCompany.SelectedValue == null)
            {
                MessageBox.Show("Please select a company", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCompany.Focus();
                return;
            }

            int company = (int)cboCompany.SelectedValue;
            DateTime beginDate = dtpBeginDate.Value;
            DateTime endDate = dtpEndDate.Value;

            if (beginDate > endDate)
            {
                MessageBox.Show("Begin Date cannot be after End Date", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Clear previous data
                _savedFilePaths.Clear();
                _combinedFilePath = null; // Clear combined file path
                lblCombinedFileLink.Visible = false; // Hide link

                // Disable controls and show progress
                SetControlsEnabled(false);
                SetStatus("Retrieving records from database...", true);
                SetCursor(Cursors.WaitCursor);

                // Get connection string
                string? connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    MessageBox.Show("Connection string not found in appsettings.json", "Configuration Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Initialize services
                _pdfQueryService = new PdfQueryService(connectionString);
                _fileStorageService = new FileStorageService(_fileStorageSettings, clearOnStartup: true);
                _pdfCombineService = new PdfCombineServicePdfSharp(_fileStorageSettings);

                Log.Information("===========================================");
                Log.Information($"Querying database for Company {company} from {beginDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                Log.Information("===========================================");

                // Query records
                _records = await _pdfQueryService.GetUnallocatedPdfRecordsAsync(company, beginDate, endDate);

                Log.Information($"Found {_records.Count} record(s)");

                if (_records.Count > 0)
                {
                    // Display records in grid
                    dgvRecords.DataSource = _records;
                    UpdateRecordCount(_records.Count);

                    SetStatus("Processing attachments...", true);

                    // Process attachments
                    await ProcessAttachmentsAsync();

                    SetStatus($"Successfully processed {_savedFileCount} PDF files", false);
                }
                else
                {
                    dgvRecords.DataSource = null;
                    UpdateRecordCount(0);
                    MessageBox.Show("No records found for the specified criteria", "No Records",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SetStatus("No records found", false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving records");
                MessageBox.Show($"Error retrieving records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Error occurred", false);
            }
            finally
            {
                SetCursor(Cursors.Default);
                SetControlsEnabled(true);
            }
        }

        private async Task ProcessAttachmentsAsync()
        {
            if (_records == null || _fileStorageService == null || _pdfQueryService == null)
                return;

            Log.Information("===========================================");
            Log.Information("Retrieving and saving attachment data...");
            Log.Information("===========================================");

            // Collect all unique attachment IDs
            var attachmentIdsToProcess = new HashSet<Guid>();
            foreach (var record in _records)
            {
                if (record.UniqueAttchID.HasValue)
                {
                    attachmentIdsToProcess.Add(record.UniqueAttchID.Value);
                }
            }

            Log.Information($"Found {attachmentIdsToProcess.Count} unique attachment(s) to retrieve");

            int successCount = 0;
            int failCount = 0;
            int skippedCount = 0;
            int sequenceNumber = 1;
            int currentAttachment = 0;

            foreach (var attachmentId in attachmentIdsToProcess)
            {
                currentAttachment++;
                SetStatus($"Processing attachment {currentAttachment} of {attachmentIdsToProcess.Count}...", true);

                try
                {
                    Log.Information($"Processing Attachment ID: {attachmentId}");

                    var attachmentData = await _pdfQueryService.GetAttachmentDataAsync(attachmentId);

                    if (attachmentData != null && attachmentData.FileBytes != null && attachmentData.FileBytes.Length > 0)
                    {
                        if (_fileStorageSettings.IsFileTypeAllowed(attachmentData.AttachmentFileType))
                        {
                            var savedPath = await _fileStorageService.SaveAttachmentAsync(attachmentData, sequenceNumber);
                            if (!string.IsNullOrWhiteSpace(savedPath))
                            {
                                successCount++;
                                sequenceNumber++;

                                // Store the saved file path for later viewing
                                _savedFilePaths[attachmentId] = savedPath;

                                Log.Information($"  ✓ Saved: {Path.GetFileName(savedPath)}");
                            }
                            else
                            {
                                failCount++;
                            }
                        }
                        else
                        {
                            skippedCount++;
                            Log.Information($"  ⊘ Skipped non-PDF file: {attachmentData.AttachmentFileType}");
                        }
                    }
                    else
                    {
                        failCount++;
                        Log.Warning($"  ✗ No attachment data found");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    Log.Error(ex, $"  ✗ Error processing attachment");
                }
            }

            _savedFileCount = successCount;

            Log.Information("===========================================");
            Log.Information("Attachment Processing Summary:");
            Log.Information($"  Total Attachments: {attachmentIdsToProcess.Count}");
            Log.Information($"  PDF Files Saved: {successCount}");
            Log.Information($"  Non-PDF Files Skipped: {skippedCount}");
            Log.Information($"  Failed: {failCount}");
            Log.Information("===========================================");

            if (successCount > 0)
            {
                btnCombinePdfs.Enabled = true;
                MessageBox.Show($"Successfully processed {successCount} PDF files!\n\nFiles are ready to be combined.\n\nYou can now click 'View PDF' button in any row to view individual files.",
                    "Processing Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No PDF files were successfully saved.", "No Files",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetControlsEnabled(enabled));
                return;
            }

            btnRetrieveRecords.Enabled = enabled;
            dtpBeginDate.Enabled = enabled;
            dtpEndDate.Enabled = enabled;

            if (enabled && _records != null && _records.Count > 0)
            {
                btnExportToCsv.Enabled = true;
            }
            else if (!enabled)
            {
                btnExportToCsv.Enabled = false;
                btnCombinePdfs.Enabled = false;
            }
        }

        private async void btnCombinePdfs_Click(object sender, EventArgs e)
        {
            if (_pdfCombineService == null || _fileStorageService == null)
                return;

            try
            {
                SetControlsEnabled(false);
                SetStatus("Combining PDFs...", true);
                SetCursor(Cursors.WaitCursor);

                Log.Information("");
                Log.Information("===========================================");
                Log.Information("Combining PDF Files...");
                Log.Information("===========================================");

                var allPdfFiles = _fileStorageService.GetAllPdfFiles();
                Log.Information($"Total PDF files in temporary directory: {allPdfFiles.Count}");

                var combinedPdfPath = await _pdfCombineService.CombinePdfsByCreationOrderAsync(_fileStorageSettings.GetFullPath());

                // Store the combined file path
                _combinedFilePath = combinedPdfPath;

                Log.Information("✓ PDF combining completed successfully!");

                var pdfInfo = _pdfCombineService.GetCombinedPdfInfo(combinedPdfPath);
                if (pdfInfo != null)
                {
                    Log.Information("Combined PDF Details:");
                    Log.Information($"  File Name: {pdfInfo.FileName}");
                    Log.Information($"  File Size: {pdfInfo.GetFileSizeString()}");
                    Log.Information($"  Total Pages: {pdfInfo.PageCount}");
                    Log.Information($"  Created: {pdfInfo.CreationDate:yyyy-MM-dd HH:mm:ss}");
                }

                Log.Information("===========================================");

                // Update status with clickable link
                SetStatus("PDF combining completed successfully", false);
                UpdateCombinedFileLink(pdfInfo?.FileName ?? Path.GetFileName(combinedPdfPath), pdfInfo?.PageCount ?? 0);

                var result = MessageBox.Show(
                    $"PDF combining completed successfully!\n\n" +
                    $"File: {pdfInfo?.FileName}\n" +
                    $"Size: {pdfInfo?.GetFileSizeString()}\n" +
                    $"Pages: {pdfInfo?.PageCount}\n\n" +
                    $"Location: {combinedPdfPath}\n\n" +
                    $"You can also click the link in the status bar to view the file.\n\n" +
                    $"Do you want to open the file now?",
                    "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    OpenCombinedFile();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error combining PDF files");
                MessageBox.Show($"Error combining PDF files: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Error occurred", false);
            }
            finally
            {
                SetCursor(Cursors.Default);
                SetControlsEnabled(true);
            }
        }

        private void UpdateCombinedFileLink(string fileName, int pageCount)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateCombinedFileLink(fileName, pageCount));
                return;
            }

            lblCombinedFileLink.Text = $"📄 View Combined PDF: {fileName} ({pageCount} pages)";
            lblCombinedFileLink.Visible = true;
            lblCombinedFileLink.IsLink = true;
            lblCombinedFileLink.LinkColor = System.Drawing.Color.Blue;
            lblCombinedFileLink.ActiveLinkColor = System.Drawing.Color.Red;
            lblCombinedFileLink.VisitedLinkColor = System.Drawing.Color.Purple;
        }

        private void lblCombinedFileLink_Click(object? sender, EventArgs e)
        {
            OpenCombinedFile();
        }

        private void OpenCombinedFile()
        {
            if (string.IsNullOrWhiteSpace(_combinedFilePath))
            {
                MessageBox.Show("No combined PDF file is available.", "No File",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (File.Exists(_combinedFilePath))
                {
                    Log.Information($"Opening combined PDF file: {_combinedFilePath}");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _combinedFilePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show($"Combined PDF file not found at:\n{_combinedFilePath}\n\nThe file may have been moved or deleted.",
                        "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log.Warning($"Combined PDF file not found: {_combinedFilePath}");

                    // Hide the link if file doesn't exist
                    lblCombinedFileLink.Visible = false;
                    _combinedFilePath = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening combined PDF file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex, $"Error opening combined PDF file: {_combinedFilePath}");
            }
        }

        private void btnExportToCsv_Click(object sender, EventArgs e)
        {
            if (_records == null || _records.Count == 0)
            {
                MessageBox.Show("No records to export", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

                        Log.Information($"Exported {_records.Count} records to CSV: {saveFileDialog.FileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error exporting data");
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv(string filePath)
        {
            if (_records == null) return;

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

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await LoadCompaniesAsync();
        }

        private async Task LoadCompaniesAsync()
        {
            try
            {
                SetStatus("Loading companies...", true);
                SetCursor(Cursors.WaitCursor);

                // Get connection string
                string? connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    MessageBox.Show("Connection string not found in appsettings.json", "Configuration Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create temporary query service to load companies
                var queryService = new PdfQueryService(connectionString);
                
                // Load companies from database (this runs on background thread)
                _companies = await queryService.GetCompaniesAsync();

                // Update UI on the UI thread
                if (InvokeRequired)
                {
                    Invoke(() => BindCompaniesToComboBox());
                }
                else
                {
                    BindCompaniesToComboBox();
                }

                Log.Information($"Loaded {_companies.Count} companies into dropdown");
                SetStatus("Ready", false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading companies");
                MessageBox.Show($"Error loading companies: {ex.Message}\n\nPlease check your database connection.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Error loading companies", false);
            }
            finally
            {
                SetCursor(Cursors.Default);
            }
        }

        private void SetCursor(Cursor cursor)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetCursor(cursor));
                return;
            }

            this.Cursor = cursor;
        }

        private void BindCompaniesToComboBox()
        {
            if (_companies == null || _companies.Count == 0)
                return;

            // Bind to ComboBox on UI thread
            cboCompany.DataSource = _companies;
            cboCompany.DisplayMember = "Label";
            cboCompany.ValueMember = "JCCo";

            // Try to select company 12 as default
            var company12 = _companies.FirstOrDefault(c => c.JCCo == 12);
            if (company12 != null)
            {
                cboCompany.SelectedValue = 12;
                Log.Information("Set default company to 12");
            }
            else if (_companies.Count > 0)
            {
                // Fallback to first item if company 12 doesn't exist
                cboCompany.SelectedIndex = 0;
                Log.Warning("Company 12 not found, defaulting to first company");
            }
        }

        private void grpParameters_Enter(object sender, EventArgs e)
        {

        }
    }
}
