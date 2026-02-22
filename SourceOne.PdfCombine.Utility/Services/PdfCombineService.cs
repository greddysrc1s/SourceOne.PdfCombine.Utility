using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Serilog;
using SourceOne.PdfCombine.Utility.Configuration;

namespace SourceOne.PdfCombine.Utility.Services;

/// <summary>
/// Service for combining multiple PDF files into a single PDF
/// </summary>
public class PdfCombineService
{
    private readonly FileStorageSettings _settings;

    public PdfCombineService(FileStorageSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        
        if (_settings.CreateDirectoryIfNotExists)
        {
            EnsureOutputDirectoryExists();
        }

        Log.Information("PdfCombineService initialized with output path: {OutputPath}", _settings.GetOutputPath());
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
                // Create WriterProperties and explicitly disable smart mode
                var writerProperties = new WriterProperties();
                writerProperties.SetFullCompressionMode(false);
                
                using var outputPdfWriter = new PdfWriter(outputPath, writerProperties);
                using var outputPdfDocument = new PdfDocument(outputPdfWriter);
                
                int pageCount = 0;
                int successfulMerges = 0;
                
                foreach (var sourceFile in sourceFiles)
                {
                    PdfReader? sourcePdfReader = null;
                    PdfDocument? sourcePdfDocument = null;
                    
                    try
                    {
                        if (!File.Exists(sourceFile))
                        {
                            Log.Warning("Source file not found, skipping: {SourceFile}", sourceFile);
                            continue;
                        }

                        Log.Debug("Adding PDF: {SourceFile}", Path.GetFileName(sourceFile));

                        // Create reader with properties
                        var readerProperties = new ReaderProperties();
                        sourcePdfReader = new PdfReader(sourceFile, readerProperties);
                        sourcePdfDocument = new PdfDocument(sourcePdfReader);
                        
                        int sourcePagesCount = sourcePdfDocument.GetNumberOfPages();
                        
                        // Copy pages manually instead of using PdfMerger
                        for (int i = 1; i <= sourcePagesCount; i++)
                        {
                            var page = sourcePdfDocument.GetPage(i);
                            var copiedPage = page.CopyTo(outputPdfDocument);
                            outputPdfDocument.AddPage(copiedPage);
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
                    finally
                    {
                        // Explicitly dispose of resources
                        sourcePdfDocument?.Close();
                        sourcePdfReader?.Close();
                    }
                }
                
                Log.Information("Successfully combined {SuccessfulFiles}/{TotalFiles} files with {TotalPages} total pages", 
                    successfulMerges, sourceFiles.Count, pageCount);
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

        // Get all PDF files ordered by creation time
        var pdfFiles = Directory.GetFiles(sourceDirectory, "*.pdf")
            .Select(f => new FileInfo(f))
            .OrderBy(fi => fi.CreationTime)
            .Select(fi => fi.FullName)
            .ToList();

        if (pdfFiles.Count == 0)
        {
            Log.Warning("No PDF files found in directory: {SourceDirectory}", sourceDirectory);
            throw new InvalidOperationException("No PDF files found to combine");
        }

        Log.Information("Found {FileCount} PDF files in directory, ordered by creation time", pdfFiles.Count);
        
        foreach (var file in pdfFiles)
        {
            var fileInfo = new FileInfo(file);
            Log.Debug("  {FileName} - Created: {CreationTime:yyyy-MM-dd HH:mm:ss}", 
                Path.GetFileName(file), fileInfo.CreationTime);
        }

        return await CombinePdfsAsync(pdfFiles);
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
            using var pdfReader = new PdfReader(filePath);
            using var pdfDocument = new PdfDocument(pdfReader);
            
            return new PdfFileInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileSize = fileInfo.Length,
                PageCount = pdfDocument.GetNumberOfPages(),
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

/// <summary>
/// Information about a PDF file
/// </summary>
public class PdfFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int PageCount { get; set; }
    public DateTime CreationDate { get; set; }

    public string GetFileSizeString()
    {
        if (FileSize == 0)
            return "0 bytes";

        string[] sizes = { "bytes", "KB", "MB", "GB" };
        int order = 0;
        double size = FileSize;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}
