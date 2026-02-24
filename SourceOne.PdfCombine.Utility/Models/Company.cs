namespace SourceOne.PdfCombine.Utility.Models
{
    /// <summary>
    /// Represents a company from the JCCO table
    /// </summary>
    public class Company
    {
        public int JCCo { get; set; }
        public string? Name { get; set; }
        public string? Label { get; set; }

        public override string ToString()
        {
            return Label ?? $"{JCCo} - {Name}";
        }
    }
}
