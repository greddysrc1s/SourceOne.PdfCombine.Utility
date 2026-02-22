namespace SourceOne.PdfCombine.Utility.Models;

/// <summary>
/// Represents attachment data retrieved from the brptGetAttachmentData_S1S stored procedure
/// </summary>
public class AttachmentData
{
    public int HQCo { get; set; }
    public string? FormName { get; set; }
    public string? KeyField { get; set; }
    public string? Description { get; set; }
    public string? AddedBy { get; set; }
    public DateTime? AddDate { get; set; }
    public string? DocName { get; set; }
    public int AttachmentID { get; set; }
    public string? TableName { get; set; }
    public Guid UniqueAttchID { get; set; }
    public string? OrigFileName { get; set; }
    public byte[]? FileBytes { get; set; }
    public string? AttachmentFileType { get; set; }

    /// <summary>
    /// Gets the file size in a human-readable format
    /// </summary>
    public string GetFileSizeString()
    {
        if (FileBytes == null || FileBytes.Length == 0)
            return "0 bytes";

        long bytes = FileBytes.Length;
        string[] sizes = { "bytes", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    public override string ToString()
    {
        return $"File: {OrigFileName}, Size: {GetFileSizeString()}, Type: {AttachmentFileType}";
    }
}
