namespace SourceOne.PdfCombine.Utility.Configuration;

/// <summary>
/// Configuration settings for file storage
/// </summary>
public class FileStorageSettings
{
    public string TemporaryFilePath { get; set; } = "Temp/PDFs";
    public string OutputFilePath { get; set; } = "Output/Combined";
    public List<string> AllowedFileTypes { get; set; } = new() { "pdf" };
    public bool CreateDirectoryIfNotExists { get; set; } = true;

    /// <summary>
    /// Gets the full path for temporary file storage
    /// </summary>
    public string GetFullPath()
    {
        if (Path.IsPathRooted(TemporaryFilePath))
        {
            return TemporaryFilePath;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), TemporaryFilePath);
    }

    /// <summary>
    /// Gets the full path for combined PDF output
    /// </summary>
    public string GetOutputPath()
    {
        if (Path.IsPathRooted(OutputFilePath))
        {
            return OutputFilePath;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), OutputFilePath);
    }

    /// <summary>
    /// Checks if a file type is allowed
    /// </summary>
    public bool IsFileTypeAllowed(string? fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType))
            return false;

        var normalizedFileType = fileType.TrimStart('.').ToLowerInvariant();
        return AllowedFileTypes.Any(allowed => 
            allowed.TrimStart('.').Equals(normalizedFileType, StringComparison.OrdinalIgnoreCase));
    }
}
