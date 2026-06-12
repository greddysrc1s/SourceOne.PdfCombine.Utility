namespace SourceOne.PdfCombine.Utility.Models
{
    /// <summary>
    /// Represents a vendor from the APVM table
    /// </summary>
    public class Vendor
    {
        public int VendorNumber { get; set; }
        public string? Name { get; set; }

        public string DisplayName => VendorNumber == 0
            ? (Name ?? string.Empty)
            : $"{VendorNumber} - {Name}";

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
