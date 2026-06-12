using System.Text;
using System.ComponentModel;
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
        private List<VendorGroup>? _vendorGroups; // Track available vendor groups
        private List<Vendor>? _vendors; // Track available vendors
        private List<Job>? _jobs; // Track available job

        public MainForm()
        {
            InitializeComponent();

            // Build configuration from application directory
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
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

            // Initialize Date Type dropdown
            InitializeDateTypeDropdown();

            Log.Information("Application initialized successfully");
            Log.Information($"Application Directory: {AppContext.BaseDirectory}");
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

            // Add Download button column
            var downloadButtonColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Download",
                Text = "Download",
                UseColumnTextForButtonValue = true,
                Width = 80,
                Name = "DownloadColumn"
            };
            dgvRecords.Columns.Add(downloadButtonColumn);

            // Add columns
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Vendor",
                DataPropertyName = "Vendor",
                Width = 80,
                Name = "VendorColumn",
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Vendor Name",
                DataPropertyName = "Name",
                Width = 200,
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Invoice",
                DataPropertyName = "Invoice",
                Width = 120,
                Name = "InvoiceColumn",
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Invoice Date",
                DataPropertyName = "Invoice_Date",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" },
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Job",
                DataPropertyName = "JobName",
                Width = 100,
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Expense Account",
                DataPropertyName = "Expense_Account",
                Width = 120,
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Amount",
                DataPropertyName = "Amount",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" },
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Item",
                DataPropertyName = "Item",
                Width = 200,
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Attachment ID",
                DataPropertyName = "UniqueAttchID",
                Width = 250,
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "File Name",
                DataPropertyName = "OrigFileName",
                Width = 200,
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            // AddDate column removed from display

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Date Entered",
                DataPropertyName = "DateEntered",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" },
                SortMode = DataGridViewColumnSortMode.Automatic
            });

            // Apply formatting
            dgvRecords.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.Navy;
            dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvRecords.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // Add cell click event handler for View button
            dgvRecords.CellContentClick += DgvRecords_CellContentClick;

            // Add column header click event handler for sorting
            dgvRecords.ColumnHeaderMouseClick += DgvRecords_ColumnHeaderMouseClick;
        }

        private void DgvRecords_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            // Check if row index is valid
            if (e.RowIndex < 0)
                return;

            var record = dgvRecords.Rows[e.RowIndex].DataBoundItem as UnallocatedPdfRecord;
            if (record == null || !record.UniqueAttchID.HasValue)
                return;

            // Check if the click is on the View button column
            if (e.ColumnIndex == dgvRecords.Columns["ViewColumn"]!.Index)
            {
                OpenPdfFile(record.UniqueAttchID.Value, record.OrigFileName);
            }
            // Check if the click is on the Download button column
            else if (e.ColumnIndex == dgvRecords.Columns["DownloadColumn"]!.Index)
            {
                DownloadSinglePdfFile(record.UniqueAttchID.Value, record.OrigFileName);
            }
        }

        private void DgvRecords_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            // Skip if clicked on View or Download button columns
            if (dgvRecords.Columns[e.ColumnIndex].Name == "ViewColumn" ||
                dgvRecords.Columns[e.ColumnIndex].Name == "DownloadColumn")
                return;

            if (_records == null || _records.Count == 0)
                return;

            var column = dgvRecords.Columns[e.ColumnIndex];
            var propertyName = column.DataPropertyName;

            if (string.IsNullOrWhiteSpace(propertyName))
                return;

            // Determine sort direction
            ListSortDirection direction;
            if (dgvRecords.SortedColumn == column && dgvRecords.SortOrder == SortOrder.Ascending)
            {
                direction = ListSortDirection.Descending;
            }
            else
            {
                direction = ListSortDirection.Ascending;
            }

            // Apply sort
            ApplySorting(propertyName, direction);

            Log.Information($"Grid sorted by {propertyName} ({direction})");
        }

        private void ApplySorting(string propertyName, ListSortDirection direction)
        {
            if (_records == null || _records.Count == 0)
                return;

            try
            {
                var sortedRecords = direction == ListSortDirection.Ascending
                    ? _records.OrderBy(r => GetPropertyValue(r, propertyName)).ToList()
                    : _records.OrderByDescending(r => GetPropertyValue(r, propertyName)).ToList();

                // Update the binding
                dgvRecords.DataSource = null;
                dgvRecords.DataSource = sortedRecords;

                // Update the _records reference
                _records = sortedRecords;

                // Set sort glyph
                var column = dgvRecords.Columns.Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => c.DataPropertyName == propertyName);

                if (column != null)
                {
                    dgvRecords.Sort(column, direction);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error sorting grid by {propertyName}");
            }
        }

        private object? GetPropertyValue(UnallocatedPdfRecord record, int propertyIndex)
        {
            // Use reflection to get the property name from the column index
            var property = typeof(UnallocatedPdfRecord).GetProperties()[propertyIndex];
            return property.GetValue(record);
        }

        private object? GetPropertyValue(UnallocatedPdfRecord record, string propertyName)
        {
            var property = typeof(UnallocatedPdfRecord).GetProperty(propertyName);
            return property?.GetValue(record);
        }

        private void ApplyDefaultSorting()
        {
            if (_records == null || _records.Count == 0)
                return;

            try
            {
                // Sort by Vendor (ascending) then by Invoice (ascending)
                _records = _records
                    .OrderBy(r => r.Vendor)
                    .ThenBy(r => r.Invoice)
                    .ToList();

                // Update the binding
                dgvRecords.DataSource = null;
                dgvRecords.DataSource = _records;

                // Set sort glyph on Vendor column
                var vendorColumn = dgvRecords.Columns["VendorColumn"];
                if (vendorColumn != null)
                {
                    dgvRecords.Sort(vendorColumn, ListSortDirection.Ascending);
                }

                Log.Information("Applied default sorting: Vendor (Ascending), Invoice (Ascending)");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying default sorting");
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

        private void DownloadSinglePdfFile(Guid attachmentId, string? originalFileName)
        {
            try
            {
                // Check if the file has been saved and we have its path
                if (!_savedFilePaths.TryGetValue(attachmentId, out string? sourceFilePath))
                {
                    MessageBox.Show($"PDF file has not been processed yet.\n\nFile: {originalFileName ?? "Unknown"}\n\nPlease wait for the processing to complete.",
                        "File Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log.Information($"Attempted to download PDF that hasn't been processed: {attachmentId}");
                    return;
                }

                if (!File.Exists(sourceFilePath))
                {
                    MessageBox.Show($"PDF file not found at: {sourceFilePath}\n\nThe file may have been moved or deleted.",
                        "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log.Warning($"PDF file not found: {sourceFilePath}");
                    return;
                }

                // Show save file dialog
                using var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                    Title = "Save PDF File",
                    FileName = originalFileName ?? Path.GetFileName(sourceFilePath),
                    DefaultExt = "pdf",
                    AddExtension = true,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    var destinationPath = saveFileDialog.FileName;

                    // Copy the file to the selected location
                    File.Copy(sourceFilePath, destinationPath, overwrite: true);

                    Log.Information($"Downloaded PDF file: {originalFileName} to {destinationPath}");

                    var result = MessageBox.Show(
                        $"PDF file downloaded successfully!\n\n" +
                        $"File: {Path.GetFileName(destinationPath)}\n" +
                        $"Location: {destinationPath}\n\n" +
                        $"Do you want to open the file now?",
                        "Download Complete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = destinationPath,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    Log.Information("User cancelled PDF download");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading PDF file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex, $"Error downloading PDF file for attachment: {attachmentId}");
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

        private void UpdateRecordCount(int recordCount, int? pdfFileCount = null)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateRecordCount(recordCount, pdfFileCount));
                return;
            }

            if (pdfFileCount.HasValue)
            {
                lblRecordCount.Text = $"Total Records: {recordCount} | PDF Files: {pdfFileCount.Value}";
            }
            else
            {
                lblRecordCount.Text = $"Total Records: {recordCount}";
            }
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

            // Get optional filter values
            string? dateType = cboDateType.SelectedValue as string;
            string? vendorGroup = null;
            string? vendor = null;
            string? job = null;

            // Get vendor group if selected and valid (not "All")
            if (cboVendorGroup.Enabled && cboVendorGroup.SelectedValue != null && cboVendorGroup.SelectedValue is string vg && !string.IsNullOrWhiteSpace(vg))
            {
                vendorGroup = vg;
            }

            // Collect all checked vendors as comma-separated vendor numbers
            var checkedVendors = clbVendor.CheckedItems
                .Cast<Vendor>()
                .Where(v => v.VendorNumber > 0)
                .Select(v => v.VendorNumber.ToString())
                .ToList();
            if (checkedVendors.Count > 0)
            {
                vendor = string.Join(",", checkedVendors);
            }

            // Collect all checked jobs as comma-separated job numbers
            var checkedJobs = clbJob.CheckedItems
                .Cast<Job>()
                .Where(j => !string.IsNullOrWhiteSpace(j.JobNumber))
                .Select(j => j.JobNumber!)
                .ToList();
            if (checkedJobs.Count > 0)
            {
                job = string.Join(",", checkedJobs);
            }

            // Get GL Account filter value
            string? glAccount = string.IsNullOrWhiteSpace(txtGLAccount.Text) ? null : txtGLAccount.Text.Trim();

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
                Log.Information($"Filters - DateType: {dateType}, VendorGroup: {vendorGroup}, Vendor: {vendor ?? "(all)"}, Job: {job ?? "(all)"}, GLAccount: {glAccount}");
                Log.Information("===========================================");

                // Query records with all parameters
                _records = await _pdfQueryService.GetUnallocatedPdfRecordsAsync(
                    company,
                    beginDate,
                    endDate,
                    dateType,
                    vendorGroup,
                    vendor,
                    job,
                    glAccount);

                Log.Information($"Found {_records.Count} record(s)");

                if (_records.Count > 0)
                {
                    // Apply default sorting (Vendor, then Invoice)
                    ApplyDefaultSorting();

                    // Display records in grid
                    UpdateRecordCount(_records.Count);

                    SetStatus("Processing attachments...", true);

                    // Process attachments
                    await ProcessAttachmentsAsync();

                    // Update the record count label to show both records and PDF files
                    UpdateRecordCount(_records.Count, _savedFileCount);

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
                btnExportToExcel.Enabled = true;
            }
            else if (!enabled)
            {
                btnExportToExcel.Enabled = false;
                btnCombinePdfs.Enabled = false;
                btnDownload.Enabled = false;
            }
        }

        private async Task ProcessAttachmentsAsync()
        {
            if (_records == null || _fileStorageService == null || _pdfQueryService == null)
            {
                Log.Warning("ProcessAttachmentsAsync called with null dependencies");
                return;
            }

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

            Log.Information($"Found {attachmentIdsToProcess.Count} unique attachment(s) to retrieve from {_records.Count} records");

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

                    if (attachmentData == null)
                    {
                        failCount++;
                        Log.Warning($"  ✗ GetAttachmentDataAsync returned null for {attachmentId}");
                        continue;
                    }

                    if (attachmentData.FileBytes == null || attachmentData.FileBytes.Length == 0)
                    {
                        failCount++;
                        Log.Warning($"  ✗ No file bytes found for {attachmentId} (FileName: {attachmentData.OrigFileName})");
                        continue;
                    }

                    Log.Information($"  Retrieved {attachmentData.FileBytes.Length} bytes for {attachmentData.OrigFileName} (Type: {attachmentData.AttachmentFileType})");

                    if (_fileStorageSettings.IsFileTypeAllowed(attachmentData.AttachmentFileType))
                    {
                        try
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
                                Log.Warning($"  ✗ SaveAttachmentAsync returned null/empty path for {attachmentData.OrigFileName}");
                            }
                        }
                        catch (Exception saveEx)
                        {
                            failCount++;
                            Log.Error(saveEx, $"  ✗ Exception while saving {attachmentData.OrigFileName}");
                        }
                    }
                    else
                    {
                        skippedCount++;
                        Log.Information($"  ⊘ Skipped non-PDF file: {attachmentData.AttachmentFileType} ({attachmentData.OrigFileName})");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    Log.Error(ex, $"  ✗ Error processing attachment {attachmentId}");
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
                btnDownload.Enabled = true;
                MessageBox.Show($"Successfully processed {successCount} PDF files!\n\nFiles are ready to be combined or downloaded.\n\nYou can now click 'View PDF' button in any row to view individual files.",
                    "Processing Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"No PDF files were successfully saved.");
                errorMessage.AppendLine();
                errorMessage.AppendLine($"Summary:");
                errorMessage.AppendLine($"- Total attachments processed: {attachmentIdsToProcess.Count}");
                errorMessage.AppendLine($"- Non-PDF files skipped: {skippedCount}");
                errorMessage.AppendLine($"- Failed: {failCount}");
                errorMessage.AppendLine();
                errorMessage.AppendLine($"Please check the log file at:");
                errorMessage.AppendLine($"Logs\\PdfCombine-{DateTime.Now:yyyyMMdd}.log");
                errorMessage.AppendLine();
                errorMessage.AppendLine("Common issues:");
                errorMessage.AppendLine("- Attachment data is null or empty");
                errorMessage.AppendLine("- File type is not PDF");
                errorMessage.AppendLine("- Permission issues writing to temp folder");

                MessageBox.Show(errorMessage.ToString(), "No Files Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void lblCombinedFileLink_Click(object? sender, EventArgs e)
        {
            OpenCombinedFile();
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

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (_fileStorageService == null || _savedFileCount == 0)
            {
                MessageBox.Show("No PDF files are available to download.", "No Files",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Log.Information("Download button clicked - attempting to show folder dialog");

                string? destinationFolder = ShowFolderSelectionDialog();

                if (string.IsNullOrWhiteSpace(destinationFolder))
                {
                    Log.Information("No folder selected or dialog cancelled");
                    return;
                }

                Log.Information($"Folder selected: {destinationFolder}");

                // Now perform the async file operations
                _ = PerformDownloadAsync(destinationFolder);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in download button click handler");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string? ShowFolderSelectionDialog()
        {
            try
            {
                Log.Information("Showing FolderBrowserDialog...");
                Log.Information($"Current thread apartment state: {Thread.CurrentThread.GetApartmentState()}");
                
                using var folderDialog = new FolderBrowserDialog
                {
                    Description = "Select a folder to save the PDF files",
                    ShowNewFolderButton = true,
                    UseDescriptionForTitle = true,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                // Show the dialog - this should block until user responds
                DialogResult result = folderDialog.ShowDialog(this);
                
                Log.Information($"Dialog result: {result}");
                
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    Log.Information($"Folder selected: {folderDialog.SelectedPath}");
                    return folderDialog.SelectedPath;
                }
                
                Log.Information("User cancelled folder selection");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception in ShowFolderSelectionDialog");
                throw;
            }
        }

        private async Task PerformDownloadAsync(string destinationFolder)
        {
            if (_fileStorageService == null)
                return;

            try
            {
                SetControlsEnabled(false);
                SetStatus("Preparing to download PDF files...", true);
                SetCursor(Cursors.WaitCursor);

                Log.Information("===========================================");
                Log.Information($"Downloading PDF files to: {destinationFolder}");
                Log.Information("===========================================");

                // Get all PDF files from temporary directory
                var pdfFiles = _fileStorageService.GetAllPdfFiles();
                
                Log.Information($"Found {pdfFiles.Count} PDF file(s) to download");

                if (pdfFiles.Count == 0)
                {
                    MessageBox.Show("No PDF files found to download.", "No Files",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int copiedCount = 0;
                int failedCount = 0;
                int totalFiles = pdfFiles.Count;

                // Copy files one at a time with progress updates
                await Task.Run(() =>
                {
                    foreach (var sourceFile in pdfFiles)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(sourceFile);
                              
                            // Update status on UI thread
                            Invoke(() => SetStatus($"Downloading file {copiedCount + 1} of {totalFiles}: {fileName}", true));

                            var destinationPath = Path.Combine(destinationFolder, fileName);

                            // Handle duplicate filenames in destination
                            destinationPath = GetUniqueDestinationPath(destinationPath);

                            File.Copy(sourceFile, destinationPath, overwrite: false);
                            copiedCount++;

                            Log.Information($"  ✓ Copied {copiedCount}/{totalFiles}: {fileName}");
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            Log.Error(ex, $"  ✗ Failed to copy: {Path.GetFileName(sourceFile)}");
                        }
                    }
                });

                Log.Information("===========================================");
                Log.Information($"Download Summary: {copiedCount} files copied, {failedCount} failed");
                Log.Information("===========================================");

                SetStatus($"Successfully downloaded {copiedCount} PDF files", false);

                var result = MessageBox.Show(
                    $"Successfully downloaded {copiedCount} PDF file(s) to:\n\n{destinationFolder}\n\n" +
                    (failedCount > 0 ? $"{failedCount} file(s) failed to copy.\n\n" : "") +
                    "Do you want to open the folder?",
                    "Download Complete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = destinationFolder,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error downloading PDF files");
                MessageBox.Show($"Error downloading PDF files: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Error occurred during download", false);
            }
            finally
            {
                SetCursor(Cursors.Default);
                SetControlsEnabled(true);
            }
        }

        /// <summary>
        /// Gets a unique file path in the destination by appending a number if the file already exists
        /// </summary>
        private string GetUniqueDestinationPath(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            int counter = 1;

            string newPath;
            do
            {
                newPath = Path.Combine(directory, $"{fileNameWithoutExtension}_copy{counter}{extension}");
                counter++;
            }
            while (File.Exists(newPath));

            Log.Debug("File already exists in destination, using unique name: {NewPath}", Path.GetFileName(newPath));
            return newPath;
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

                // Trigger loading of vendor groups and jobs for the default company
                _ = LoadVendorGroupsAndJobsForCompanyAsync(12);
            }
            else if (_companies.Count > 0)
            {
                // Fallback to first item if company 12 doesn't exist
                cboCompany.SelectedIndex = 0;
                Log.Warning("Company 12 not found, defaulting to first company");

                // Trigger loading for the first company
                var firstCompany = _companies[0];
                _ = LoadVendorGroupsAndJobsForCompanyAsync(firstCompany.JCCo);
            }
        }

        private async Task LoadVendorGroupsAndJobsForCompanyAsync(int company)
        {
            try
            {
                // Load both vendor groups and jobs for the company
                await Task.WhenAll(
                    LoadVendorGroupsAsync(company),
                    LoadJobsAsync(company)
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading vendor groups and jobs for company {company}");
            }
        }

        private async void cboCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCompany.SelectedValue == null || !(cboCompany.SelectedValue is int))
                return;

            int selectedCompany = (int)cboCompany.SelectedValue;

            // Load both vendor groups and jobs for the selected company
            await Task.WhenAll(
                LoadVendorGroupsAsync(selectedCompany),
                LoadJobsAsync(selectedCompany)
            );
        }

        private async Task LoadVendorGroupsAsync(int company)
        {
            try
            {
                SetStatus($"Loading vendor groups for company {company}...", true);

                // Get connection string
                string? connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    Log.Warning("Connection string not found, cannot load vendor groups");
                    return;
                }

                // Create temporary query service to load vendor groups
                var queryService = new PdfQueryService(connectionString);

                // Load vendor groups from database
                _vendorGroups = await queryService.GetVendorGroupsAsync(company);

                // Update UI on the UI thread
                if (InvokeRequired)
                {
                    Invoke(() => BindVendorGroupsToComboBox());
                }
                else
                {
                    BindVendorGroupsToComboBox();
                }

                Log.Information($"Loaded {_vendorGroups.Count} vendor groups for company {company}");
                SetStatus("Ready", false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading vendor groups for company {company}");
                cboVendorGroup.DataSource = null;
                cboVendorGroup.Items.Clear();
                SetStatus("Error loading vendor groups", false);
            }
        }

        private async Task LoadJobsAsync(int company, string? vendorGroup = null, string? vendor = null)
        {
            try
            {
                SetStatus($"Loading jobs for company {company}...", true);

                // Get connection string
                string? connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    Log.Warning("Connection string not found, cannot load jobs");
                    return;
                }

                // Create temporary query service to load jobs
                var queryService = new PdfQueryService(connectionString);

                // Load jobs from database with optional filters
                _jobs = await queryService.GetJobsAsync(company, vendorGroup, vendor);

                // Update UI on the UI thread
                if (InvokeRequired)
                {
                    Invoke(() => BindJobsToCheckedListBox());
                }
                else
                {
                    BindJobsToCheckedListBox();
                }

                var filterInfo = vendorGroup != null || vendor != null
                    ? $" (filtered by VendorGroup={vendorGroup ?? "(all)"}, Vendors={vendor ?? "(all)"})" 
                    : "";
                Log.Information($"Loaded {_jobs.Count} jobs for company {company}{filterInfo}");
                SetStatus("Ready", false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading jobs for company {company}");
                clbJob.DataSource = null;
                clbJob.Items.Clear();
                SetStatus("Error loading jobs", false);
            }
        }

        private void BindVendorGroupsToComboBox()
        {
            // Create a list with "All" option
            var vendorGroupsWithAll = new List<VendorGroup>();

            // Add "All" option at the beginning
            vendorGroupsWithAll.Add(new VendorGroup { VendorGroupCode = "" });

            if (_vendorGroups != null && _vendorGroups.Count > 0)
            {
                vendorGroupsWithAll.AddRange(_vendorGroups);
            }

            // Bind to ComboBox on UI thread
            cboVendorGroup.Enabled = true;
            cboVendorGroup.DataSource = vendorGroupsWithAll;
            cboVendorGroup.DisplayMember = "VendorGroupCode";
            cboVendorGroup.ValueMember = "VendorGroupCode";

            // Select "All" by default
            cboVendorGroup.SelectedIndex = 0;

            Log.Information($"Loaded {_vendorGroups?.Count ?? 0} vendor groups (plus 'All' option)");
        }

        private void BindJobsToCheckedListBox()
        {
            clbJob.Items.Clear();
            chkSelectAllJobs.Checked = false;

            if (_jobs != null && _jobs.Count > 0)
            {
                foreach (var job in _jobs)
                {
                    clbJob.Items.Add(job, false);
                }
            }

            Log.Information($"Loaded {_jobs?.Count ?? 0} jobs into multi-select list");
        }

        private async void cboVendorGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboVendorGroup.SelectedValue == null || !(cboVendorGroup.SelectedValue is string))
                return;

            string selectedVendorGroup = (string)cboVendorGroup.SelectedValue;

            // If "All" is selected (empty string), don't load vendors - show all vendors option
            if (string.IsNullOrEmpty(selectedVendorGroup))
            {
                // Clear vendors list
                _vendors = new List<Vendor>();
                BindVendorsToCheckedListBox();

                // Also reload jobs for just the company
                if (cboCompany.SelectedValue is int company)
                {
                    await LoadJobsAsync(company);
                }
                return;
            }

            // Load vendors for the selected vendor group
            await LoadVendorsAsync(selectedVendorGroup);
            
            // Reload jobs filtered by vendor group (but no specific vendor yet)
            if (cboCompany.SelectedValue is int selectedCompany)
            {
                await LoadJobsAsync(selectedCompany);
            }
        }

        private async Task LoadVendorsAsync(string vendorGroup)
        {
            try
            {
                SetStatus($"Loading vendors for vendor group {vendorGroup}...", true);

                // Get connection string
                string? connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    Log.Warning("Connection string not found, cannot load vendors");
                    return;
                }

                // Create temporary query service to load vendors
                var queryService = new PdfQueryService(connectionString);

                // Load vendors from database
                _vendors = await queryService.GetVendorsAsync(vendorGroup);

                // Update UI on the UI thread
                if (InvokeRequired)
                {
                    Invoke(() => BindVendorsToCheckedListBox());
                }
                else
                {
                    BindVendorsToCheckedListBox();
                }

                Log.Information($"Loaded {_vendors.Count} vendors for vendor group {vendorGroup}");
                SetStatus("Ready", false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading vendors for vendor group {vendorGroup}");
                clbVendor.Items.Clear();
                SetStatus("Error loading vendors", false);
            }
        }

        private void BindVendorsToCheckedListBox()
        {
            clbVendor.Items.Clear();
            chkSelectAllVendors.Checked = false;

            if (_vendors != null && _vendors.Count > 0)
            {
                foreach (var vendor in _vendors)
                {
                    clbVendor.Items.Add(vendor, false);
                }
            }

            Log.Information($"Loaded {_vendors?.Count ?? 0} vendors into multi-select list");
        }

        private async void clbVendor_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            // Reload jobs when vendor check state changes.
            // ItemCheck fires before the state is committed, so we compute the
            // effective checked set by combining the current checked items with
            // the item whose state is about to change.
            if (cboCompany.SelectedValue is not int company)
                return;

            string? vendorGroup = null;
            if (cboVendorGroup.SelectedValue is string vg && !string.IsNullOrWhiteSpace(vg))
                vendorGroup = vg;

            // Build the effective vendor list after the state change
            var effectiveChecked = clbVendor.CheckedItems
                .Cast<Vendor>()
                .Where(v => v.VendorNumber > 0)
                .Select(v => v.VendorNumber)
                .ToHashSet();

            if (clbVendor.Items[e.Index] is Vendor toggled && toggled.VendorNumber > 0)
            {
                if (e.NewValue == CheckState.Checked)
                    effectiveChecked.Add(toggled.VendorNumber);
                else
                    effectiveChecked.Remove(toggled.VendorNumber);
            }

            // Pass all selected vendors as comma-separated string
            string? vendorList = effectiveChecked.Count > 0 
                ? string.Join(",", effectiveChecked.OrderBy(v => v)) 
                : null;

            await LoadJobsAsync(company, vendorGroup, vendorList);
        }

        private void grpParameters_Enter(object sender, EventArgs e)
        {

        }

        private void InitializeDateTypeDropdown()
        {
            // Load predefined date types
            var dateTypes = DateType.GetDateTypes();

            cboDateType.DataSource = dateTypes;
            cboDateType.DisplayMember = "DisplayName";
            cboDateType.ValueMember = "Code";

            // Set default selection to first item (Financial Period)
            if (dateTypes.Count > 0)
            {
                cboDateType.SelectedIndex = 0;
            }

            Log.Information("Date Type dropdown initialized with {Count} options", dateTypes.Count);
        }

        private async void chkSelectAllVendors_CheckedChanged(object? sender, EventArgs e)
        {
            if (clbVendor.Items.Count == 0)
                return;

            bool isChecked = chkSelectAllVendors.Checked;

            // Temporarily remove the ItemCheck handler to prevent multiple job reloads
            clbVendor.ItemCheck -= clbVendor_ItemCheck;

            try
            {
                for (int i = 0; i < clbVendor.Items.Count; i++)
                {
                    clbVendor.SetItemChecked(i, isChecked);
                }

                Log.Information($"{(isChecked ? "Selected" : "Deselected")} all {clbVendor.Items.Count} vendors");

                // Now trigger the job reload once
                await TriggerVendorJobReload();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in chkSelectAllVendors_CheckedChanged");
                MessageBox.Show($"Error loading jobs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-attach the handler
                clbVendor.ItemCheck += clbVendor_ItemCheck;
            }
        }

        private void chkSelectAllJobs_CheckedChanged(object? sender, EventArgs e)
        {
            if (clbJob.Items.Count == 0)
                return;

            bool isChecked = chkSelectAllJobs.Checked;

            for (int i = 0; i < clbJob.Items.Count; i++)
            {
                clbJob.SetItemChecked(i, isChecked);
            }

            Log.Information($"{(isChecked ? "Selected" : "Deselected")} all {clbJob.Items.Count} jobs");
        }

        private async Task TriggerVendorJobReload()
        {
            if (cboCompany.SelectedValue is not int company)
                return;

            string? vendorGroup = null;
            if (cboVendorGroup.SelectedValue is string vg && !string.IsNullOrWhiteSpace(vg))
                vendorGroup = vg;

            var checkedVendors = clbVendor.CheckedItems
                .Cast<Vendor>()
                .Where(v => v.VendorNumber > 0)
                .Select(v => v.VendorNumber.ToString())
                .OrderBy(v => v)
                .ToList();

            string? vendorList = checkedVendors.Count > 0 
                ? string.Join(",", checkedVendors) 
                : null;

            await LoadJobsAsync(company, vendorGroup, vendorList);
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            if (_records == null || _records.Count == 0)
            {
                MessageBox.Show("No records to export", "Export to Excel",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                SetCursor(Cursors.WaitCursor);
                SetStatus("Exporting to Excel...", true);

                // Show save file dialog
                using var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Export to Excel",
                    FileName = $"AttachmentsExcel_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    DefaultExt = "xlsx",
                    AddExtension = true,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    ExportToExcel(saveFileDialog.FileName);

                    SetStatus("Excel export completed successfully", false);

                    var result = MessageBox.Show(
                        $"Excel file exported successfully!\n\n" +
                        $"Location: {saveFileDialog.FileName}\n\n" +
                        $"Do you want to open the file now?",
                        "Export Complete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error exporting to Excel");
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Error exporting to Excel", false);
            }
            finally
            {
                SetCursor(Cursors.Default);
            }
        }

        private void ExportToExcel(string filePath)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Unallocated PDF Records");

            // Add headers from visible DataGridView columns
            int colIndex = 1;
            var columnMapping = new Dictionary<int, int>(); // Maps Excel column to DataGridView column

            foreach (DataGridViewColumn dgvCol in dgvRecords.Columns)
            {
                // Skip button columns
                if (dgvCol is DataGridViewButtonColumn)
                    continue;

                var headerCell = worksheet.Cell(1, colIndex);
                headerCell.Value = dgvCol.HeaderText;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                headerCell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                columnMapping[colIndex] = dgvCol.Index;
                colIndex++;
            }

            // Add data rows
            int rowIndex = 2;
            foreach (DataGridViewRow dgvRow in dgvRecords.Rows)
            {
                if (dgvRow.IsNewRow)
                    continue;

                colIndex = 1;
                foreach (var kvp in columnMapping)
                {
                    var excelCol = kvp.Key;
                    var dgvColIndex = kvp.Value;

                    var cellValue = dgvRow.Cells[dgvColIndex].Value;
                    var cell = worksheet.Cell(rowIndex, excelCol);

                    if (cellValue != null)
                    {
                        // Handle different data types
                        if (cellValue is DateTime dateTime)
                        {
                            cell.Value = dateTime;
                            cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                        }
                        else if (cellValue is decimal || cellValue is double || cellValue is float)
                        {
                            cell.Value = Convert.ToDouble(cellValue);

                            // Apply currency format if the column header contains "Amount"
                            if (dgvRecords.Columns[dgvColIndex].HeaderText.Contains("Amount"))
                            {
                                cell.Style.NumberFormat.Format = "$#,##0.00";
                            }
                        }
                        else if (cellValue is int || cellValue is long)
                        {
                            cell.Value = Convert.ToInt64(cellValue);
                        }
                        else
                        {
                            cell.Value = cellValue.ToString();
                        }
                    }

                    cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    colIndex++;
                }

                rowIndex++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Freeze header row
            worksheet.SheetView.FreezeRows(1);

            // Save the workbook
            workbook.SaveAs(filePath);

            Log.Information($"Exported {_records.Count} records to Excel file: {filePath}");
        }
    }
}
