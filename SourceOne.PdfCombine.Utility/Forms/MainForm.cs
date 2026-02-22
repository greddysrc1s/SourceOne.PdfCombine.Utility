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

            // Set default dates
            dtpBeginDate.Value = DateTime.Now.AddMonths(-1);
            dtpEndDate.Value = DateTime.Now;

            LogMessage("Application initialized successfully");
            LogMessage($"Temporary file path: {_fileStorageSettings.GetFullPath()}");
            LogMessage($"Output file path: {_fileStorageSettings.GetOutputPath()}");
        }

        private void LogMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(() => LogMessage(message));
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            Log.Information(message);
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
        }

        private async void btnRetrieveRecords_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (!int.TryParse(txtCompany.Text, out int company))
            {
                MessageBox.Show("Please enter a valid company ID", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCompany.Focus();
                return;
            }

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
                // Disable controls
                btnRetrieveRecords.Enabled = false;
                btnCombinePdfs.Enabled = false;
                SetStatus("Retrieving records...", true);

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

                LogMessage("===========================================");
                LogMessage($"Querying database for Company {company} from {beginDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                LogMessage("===========================================");

                // Query records
                _records = await _pdfQueryService.GetUnallocatedPdfRecordsAsync(company, beginDate, endDate);

                LogMessage($"Found {_records.Count} record(s)");

                if (_records.Count > 0)
                {
                    // Show the display attachment form
                    var displayForm = new DisplayAttachmentForm(_records);
                    displayForm.ShowDialog(this);

                    // Process attachments
                    await ProcessAttachmentsAsync();
                }
                else
                {
                    MessageBox.Show("No records found for the specified criteria", "No Records", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                SetStatus($"Retrieved {_records.Count} records", false);
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
                MessageBox.Show($"Error retrieving records: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex, "Error retrieving records");
                SetStatus("Error occurred", false);
            }
            finally
            {
                btnRetrieveRecords.Enabled = true;
            }
        }

        private async Task ProcessAttachmentsAsync()
        {
            if (_records == null || _fileStorageService == null || _pdfQueryService == null)
                return;

            SetStatus("Processing attachments...", true);
            LogMessage("===========================================");
            LogMessage("Retrieving and saving attachment data...");
            LogMessage("===========================================");

            // Collect all unique attachment IDs
            var attachmentIdsToProcess = new HashSet<Guid>();
            foreach (var record in _records)
            {
                if (record.UniqueAttchID.HasValue)
                {
                    attachmentIdsToProcess.Add(record.UniqueAttchID.Value);
                }
            }

            LogMessage($"Found {attachmentIdsToProcess.Count} unique attachment(s) to retrieve");

            int successCount = 0;
            int failCount = 0;
            int skippedCount = 0;
            int sequenceNumber = 1;

            foreach (var attachmentId in attachmentIdsToProcess)
            {
                try
                {
                    LogMessage($"Processing Attachment ID: {attachmentId}");

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
                                LogMessage($"  ? Saved: {Path.GetFileName(savedPath)}");
                            }
                            else
                            {
                                failCount++;
                            }
                        }
                        else
                        {
                            skippedCount++;
                            LogMessage($"  ? Skipped non-PDF file: {attachmentData.AttachmentFileType}");
                        }
                    }
                    else
                    {
                        failCount++;
                        LogMessage($"  ? No attachment data found");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    LogMessage($"  ? Error: {ex.Message}");
                    Log.Error(ex, "Error processing attachment");
                }
            }

            _savedFileCount = successCount;

            LogMessage("===========================================");
            LogMessage("Attachment Processing Summary:");
            LogMessage($"  Total Attachments: {attachmentIdsToProcess.Count}");
            LogMessage($"  PDF Files Saved: {successCount}");
            LogMessage($"  Non-PDF Files Skipped: {skippedCount}");
            LogMessage($"  Failed: {failCount}");
            LogMessage("===========================================");

            if (successCount > 0)
            {
                btnCombinePdfs.Enabled = true;
                SetStatus($"Ready to combine {successCount} PDF files", false);
            }
            else
            {
                SetStatus("No PDF files to combine", false);
            }
        }

        private async void btnCombinePdfs_Click(object sender, EventArgs e)
        {
            if (_pdfCombineService == null || _fileStorageService == null)
                return;

            try
            {
                btnCombinePdfs.Enabled = false;
                SetStatus("Combining PDFs...", true);

                LogMessage("");
                LogMessage("===========================================");
                LogMessage("Combining PDF Files...");
                LogMessage("===========================================");

                var allPdfFiles = _fileStorageService.GetAllPdfFiles();
                LogMessage($"Total PDF files in temporary directory: {allPdfFiles.Count}");

                var combinedPdfPath = await _pdfCombineService.CombinePdfsByCreationOrderAsync(_fileStorageSettings.GetFullPath());

                LogMessage("? PDF combining completed successfully!");
                LogMessage($"Combined PDF location: {combinedPdfPath}");

                var pdfInfo = _pdfCombineService.GetCombinedPdfInfo(combinedPdfPath);
                if (pdfInfo != null)
                {
                    LogMessage("Combined PDF Details:");
                    LogMessage($"  File Name: {pdfInfo.FileName}");
                    LogMessage($"  File Size: {pdfInfo.GetFileSizeString()}");
                    LogMessage($"  Total Pages: {pdfInfo.PageCount}");
                    LogMessage($"  Created: {pdfInfo.CreationDate:yyyy-MM-dd HH:mm:ss}");
                }

                LogMessage("===========================================");

                SetStatus("PDF combining completed successfully", false);

                var result = MessageBox.Show($"PDF combining completed successfully!\n\nLocation: {combinedPdfPath}\n\nDo you want to open the file?", 
                    "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = combinedPdfPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error combining PDF files: {ex.Message}");
                MessageBox.Show($"Error combining PDF files: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex, "Error combining PDF files");
                SetStatus("Error occurred", false);
            }
        }
    }
}
