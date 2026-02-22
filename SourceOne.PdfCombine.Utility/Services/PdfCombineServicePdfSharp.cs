using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Serilog;
using SourceOne.PdfCombine.Utility.Configuration;

namespace SourceOne.PdfCombine.Utility.Services;

/// <summary>
/// Service for combining multiple PDF files into a single PDF using PdfSharp
/// </summary>
public class PdfCombineServicePdfSharp
{
    private readonly FileStorageSettings _settings;

    public PdfCombineServicePdfSharp(FileStorageSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        
        if (_settings.CreateDirectoryIfNotExists)
        {
            EnsureOutputDirectoryExists();
        }

        Log.Information("PdfCombineServicePdfSharp initialized with output path: {OutputPath}", _settings.GetOutputPath());
    }

    /// <summary>
    /// Ensures the output directory exists
    /// </summary>
    private void EnsureOutputDirectoryExists()
    {
        var outputPath = _settings.GetOutputPath();
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
            Log.Information("Created output directory: {DirectoryPath}", outputPath);
        }
    }

    /// <summary>
    /// Combines multiple PDF files into a single PDF with today's date
    /// </summary>
    /// <param name="sourceFiles">List of PDF file paths to combine (in order)</param>
    /// <returns>Path to the combined PDF file</returns>
    public async Task<string> CombinePdfsAsync(List<string> sourceFiles)
    {
        if (sourceFiles == null || sourceFiles.Count == 0)
        {
            Log.Warning("No source files provided for combining");
            throw new ArgumentException("At least one source file is required", nameof(sourceFiles));
        }

        // Generate output filename with today's date
        var todayDate = DateTime.Now.ToString("yyyy-MM-dd");
        var outputFileName = $"Combined_PDF_{todayDate}.pdf";
        var outputPath = Path.Combine(_settings.GetOutputPath(), outputFileName);

        // Handle duplicate filenames
        outputPath = GetUniqueFilePath(outputPath);

        Log.Information("Combining {FileCount} PDF files into: {OutputPath}", sourceFiles.Count, outputPath);

        try
        {
            await Task.Run(() =>
            {
                // Create a new PDF document for output
                using var outputDocument = new PdfDocument();
                
                int pageCount = 0;
                int successfulMerges = 0;
                
                foreach (var sourceFile in sourceFiles)
                {
                    try
                    {
                        if (!File.Exists(sourceFile))
                        {
                            Log.Warning("Source file not found, skipping: {SourceFile}", sourceFile);
                            continue;
                        }

                        Log.Debug("Adding PDF: {SourceFile}", Path.GetFileName(sourceFile));

                        // Open the source PDF document
                        using var sourceDocument = PdfReader.Open(sourceFile, PdfDocumentOpenMode.Import);
                        
                        int sourcePagesCount = sourceDocument.PageCount;
                        
                        // Copy all pages from source to output
                        for (int i = 0; i < sourcePagesCount; i++)
                        {
                            var page = sourceDocument.Pages[i];
                            outputDocument.AddPage(page);
                        }
                        
                        pageCount += sourcePagesCount;
                        successfulMerges++;
                        Log.Debug("  Added {PageCount} page(s) from {FileName}", sourcePagesCount, Path.GetFileName(sourceFile));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error adding PDF to combined file: {SourceFile}", sourceFile);
                        // Continue with other files
                    }
                }
                
                // Save the combined PDF
                if (outputDocument.PageCount > 0)
                {
                    outputDocument.Save(outputPath);
                    Log.Information("Successfully combined {SuccessfulFiles}/{TotalFiles} files with {TotalPages} total pages", 
                        successfulMerges, sourceFiles.Count, pageCount);
                }
                else
                {
                    Log.Warning("No pages were added to the output document. Combined PDF not created.");
                    throw new InvalidOperationException("No pages were successfully added to the combined PDF");
                }
            });

            var fileInfo = new FileInfo(outputPath);
            Log.Information("Combined PDF created: {FileName} ({FileSize:N0} bytes)", 
                Path.GetFileName(outputPath), fileInfo.Length);

            return outputPath;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error combining PDF files");
            throw;
        }
    }

    /// <summary>
    /// Combines PDF files in the order they were saved (by file creation time)
    /// </summary>
    /// <param name="sourceDirectory">Directory containing PDF files</param>
    /// <returns>Path to the combined PDF file</returns>
    public async Task<string> CombinePdfsByCreationOrderAsync(string sourceDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            Log.Error("Source directory not found: {SourceDirectory}", sourceDirectory);
            throw new DirectoryNotFoundException($"Directory not found: {sourceDirectory}");
        }

        // Get all PDF files
        var pdfFiles = Directory.GetFiles(sourceDirectory, "*.pdf")
            .Select(f => new FileInfo(f))
            .Select(fi => new
            {
                FileInfo = fi,
                FileName = fi.Name,
                FullPath = fi.FullName,
                SequenceNumber = ExtractSequenceNumber(fi.Name)
            })
            .ToList();

        // Sort by sequence number first (if available), then by creation time
        var sortedPdfFiles = pdfFiles
            .OrderBy(f => f.SequenceNumber ?? int.MaxValue)
            .ThenBy(f => f.FileInfo.CreationTime)
            .Select(f => f.FullPath)
            .ToList();

        if (sortedPdfFiles.Count == 0)
        {
            Log.Warning("No PDF files found in directory: {SourceDirectory}", sourceDirectory);
            throw new InvalidOperationException("No PDF files found to combine");
        }

        Log.Information("Found {FileCount} PDF files in directory, ordered by sequence number", sortedPdfFiles.Count);
        
        int displayIndex = 1;
        foreach (var file in sortedPdfFiles)
        {
            var fileInfo = new FileInfo(file);
            var sequenceNumber = ExtractSequenceNumber(fileInfo.Name);
            var sequenceDisplay = sequenceNumber.HasValue ? $"Seq: {sequenceNumber}" : "No sequence";
            
            Log.Debug("  [{DisplayIndex}] {FileName} - {Sequence} - Created: {CreationTime:yyyy-MM-dd HH:mm:ss}", 
                displayIndex, Path.GetFileName(file), sequenceDisplay, fileInfo.CreationTime);
            displayIndex++;
        }

        return await CombinePdfsAsync(sortedPdfFiles);
    }

    /// <summary>
    /// Extracts the sequence number from a filename (e.g., "1_filename.pdf" returns 1)
    /// </summary>
    private int? ExtractSequenceNumber(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        // Check if filename starts with a number followed by underscore
        var parts = fileName.Split('_', 2);
        if (parts.Length >= 2 && int.TryParse(parts[0], out int sequenceNumber))
        {
            return sequenceNumber;
        }

        return null;
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

        Log.Debug("Output file already exists, using unique name: {NewPath}", Path.GetFileName(newPath));
        return newPath;
    }

    /// <summary>
    /// Gets information about the combined PDF
    /// </summary>
    public PdfFileInfo? GetCombinedPdfInfo(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var fileInfo = new FileInfo(filePath);
        
        try
        {
            using var pdfDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.InformationOnly);
            
            return new PdfFileInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileSize = fileInfo.Length,
                PageCount = pdfDocument.PageCount,
                CreationDate = fileInfo.CreationTime
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reading PDF information: {FilePath}", filePath);
            return null;
        }
    }
}
