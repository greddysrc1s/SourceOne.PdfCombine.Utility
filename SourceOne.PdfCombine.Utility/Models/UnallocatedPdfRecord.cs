namespace SourceOne.PdfCombine.Utility.Models;

/// <summary>
/// Represents a record from the urptJFKS_Unallocated_PDF_Query_S1S stored procedure
/// </summary>
public class UnallocatedPdfRecord
{
    public Int32 Vendor { get; set; }
    public string? Name { get; set; }
    public string? Invoice { get; set; }
    public string? Job { get; set; }
    public string? Expense_Account { get; set; }
    public decimal Amount { get; set; }
    public DateTime? Invoice_Date { get; set; }
    public string? Item { get; set; }
    public Guid? UniqueAttchID { get; set; }
    public DateTime? AddDate { get; set; }
    public string? OrigFileName { get; set; }
    public Guid? UniqueAttchID_Line { get; set; }
    public DateTime? AddDate_Line { get; set; }
    public string? OrigFileName_Line { get; set; }
    public DateTime? DateEntered { get; set; }

    public override string ToString()
    {
        return $"Vendor: {Vendor}, Name: {Name}, Invoice: {Invoice}, Amount: {Amount:C}, AttachmentID: {UniqueAttchID}";
    }
}
