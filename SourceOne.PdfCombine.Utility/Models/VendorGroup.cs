namespace SourceOne.PdfCombine.Utility.Models
{
    /// <summary>
    /// Represents a vendor group from the HQCO table
    /// </summary>
    public class VendorGroup
    {
        public string? VendorGroupCode { get; set; }

        public override string ToString()
        {
            return VendorGroupCode ?? string.Empty;
        }
    }
}
