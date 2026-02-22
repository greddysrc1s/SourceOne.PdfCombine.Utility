using Serilog;
using SourceOne.PdfCombine.Utility.Configuration;
using SourceOne.PdfCombine.Utility.Models;

namespace SourceOne.PdfCombine.Utility.Services;

/// <summary>
/// Service for handling file storage operations
/// </summary>
public class FileStorageService
{
    private readonly FileStorageSettings _settings;

    public FileStorageService(FileStorageSettings settings, bool clearOnStartup = false)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        
        if (_settings.CreateDirectoryIfNotExists)
        {
            EnsureDirectoryExists();
        }

        // Clear temporary directory on startup if requested
        if (clearOnStartup)
        {
            CleanupTemporaryDirectory();
        }

        Log.Information("FileStorageService initialized with path: {TemporaryPath}", _settings.GetFullPath());
    }

    /// <summary>
    /// Ensures the temporary directory exists
    /// </summary>
    private void EnsureDirectoryExists()
    {
        var fullPath = _settings.GetFullPath();
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            Log.Information("Created temporary directory: {DirectoryPath}", fullPath);
        }
    }

    /// <summary>
    /// Cleans up the temporary directory by deleting all files
    /// </summary>
    public void CleanupTemporaryDirectory()
    {
        var fullPath = _settings.GetFullPath();
        
        if (!Directory.Exists(fullPath))
        {
            Log.Information("Temporary directory does not exist, skipping cleanup");
            return;
        }

        Log.Information("Cleaning up temporary directory: {DirectoryPath}", fullPath);

        try
        {
            var allFiles = Directory.GetFiles(fullPath);
            int deletedCount = 0;
            int failedCount = 0;

            foreach (var file in allFiles)
            {
                try
                {
                    File.Delete(file);
                    Log.Debug("Deleted file: {FileName}", Path.GetFileName(file));
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to delete file: {FilePath}", file);
                    failedCount++;
                }
            }

            if (deletedCount > 0 || failedCount > 0)
            {
                Log.Information("Temporary directory cleanup completed: {DeletedCount} deleted, {FailedCount} failed", 
                    deletedCount, failedCount);
            }
            else
            {
                Log.Information("Temporary directory was already empty");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during temporary directory cleanup: {DirectoryPath}", fullPath);
        }
    }

    /// <summary>
    /// Saves attachment data to disk if it's a PDF file
    /// </summary>
    /// <param name="attachmentData">The attachment data to save</param>
    /// <param name="sequenceNumber">Optional sequence number to prefix the filename</param>
    /// <returns>Full path to saved file, or null if not saved</returns>
    public async Task<string?> SaveAttachmentAsync(AttachmentData attachmentData, int? sequenceNumber = null)
    {
        if (attachmentData == null)
        {
            Log.Warning("Attempted to save null attachment data");
            return null;
        }

        // Check if file type is allowed (only PDF)
        if (!_settings.IsFileTypeAllowed(attachmentData.AttachmentFileType))
        {
            Log.Information("Skipping non-PDF file: {FileName} (Type: {FileType})", 
                attachmentData.OrigFileName, attachmentData.AttachmentFileType);
            return null;
        }

        // Validate file bytes
        if (attachmentData.FileBytes == null || attachmentData.FileBytes.Length == 0)
        {
            Log.Warning("No file bytes found for attachment: {FileName}", attachmentData.OrigFileName);
            return null;
        }

        // Sanitize filename
        var fileName = SanitizeFileName(attachmentData.OrigFileName);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"Attachment_{attachmentData.AttachmentID}.pdf";
            Log.Warning("Invalid filename, using generated name: {FileName}", fileName);
        }

        // Ensure .pdf extension
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".pdf";
        }

        // Add sequence number prefix if provided
        if (sequenceNumber.HasValue)
        {
            fileName = $"{sequenceNumber.Value}_{fileName}";
        }

        var fullPath = Path.Combine(_settings.GetFullPath(), fileName);

        try
        {
            // Handle duplicate filenames
            fullPath = GetUniqueFilePath(fullPath);

            await File.WriteAllBytesAsync(fullPath, attachmentData.FileBytes);
            
            Log.Information("Successfully saved PDF file: {FileName} ({FileSize}) to {FilePath}", 
                fileName, attachmentData.GetFileSizeString(), fullPath);

            return fullPath;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving attachment: {FileName} to {FilePath}", fileName, fullPath);
            throw;
        }
    }

    /// <summary>
    /// Saves multiple attachments and returns the saved file paths
    /// </summary>
    public async Task<List<string>> SaveAttachmentsAsync(IEnumerable<AttachmentData> attachments)
    {
        var savedFiles = new List<string>();
        int sequenceNumber = 1;

        foreach (var attachment in attachments)
        {
            var savedPath = await SaveAttachmentAsync(attachment, sequenceNumber);
            if (!string.IsNullOrWhiteSpace(savedPath))
            {
                savedFiles.Add(savedPath);
                sequenceNumber++;
            }
        }

        Log.Information("Saved {SavedCount} PDF files out of {TotalCount} attachments", 
            savedFiles.Count, attachments.Count());

        return savedFiles;
    }

    /// <summary>
    /// Sanitizes a filename to remove invalid characters
    /// </summary>
    private string SanitizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        return sanitized.Trim();
    }

    /// <summary>
    /// Gets a unique file path by appending a number if the file already exists
    /// </summary>
    private string GetUniqueFilePath(string filePath)
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
            newPath = Path.Combine(directory, $"{fileNameWithoutExtension}_{counter}{extension}");
            counter++;
        }
        while (File.Exists(newPath));

        Log.Debug("File already exists, using unique name: {NewPath}", newPath);
        return newPath;
    }

    /// <summary>
    /// Gets the count of PDF files in the temporary directory
    /// </summary>
    public int GetPdfFileCount()
    {
        var fullPath = _settings.GetFullPath();
        if (!Directory.Exists(fullPath))
            return 0;

        return Directory.GetFiles(fullPath, "*.pdf").Length;
    }

    /// <summary>
    /// Clears all PDF files from the temporary directory
    /// </summary>
    public void ClearTemporaryFiles()
    {
        var fullPath = _settings.GetFullPath();
        if (!Directory.Exists(fullPath))
            return;

        var pdfFiles = Directory.GetFiles(fullPath, "*.pdf");
        foreach (var file in pdfFiles)
        {
            try
            {
                File.Delete(file);
                Log.Debug("Deleted temporary file: {FilePath}", file);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to delete temporary file: {FilePath}", file);
            }
        }

        Log.Information("Cleared {FileCount} temporary PDF files", pdfFiles.Length);
    }

    /// <summary>
    /// Gets all PDF file paths in the temporary directory
    /// </summary>
    public List<string> GetAllPdfFiles()
    {
        var fullPath = _settings.GetFullPath();
        if (!Directory.Exists(fullPath))
            return new List<string>();

        return Directory.GetFiles(fullPath, "*.pdf").ToList();
    }
}
