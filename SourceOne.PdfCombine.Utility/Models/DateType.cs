namespace SourceOne.PdfCombine.Utility.Models
{
    /// <summary>
    /// Represents a date type option for filtering
    /// </summary>
    public class DateType
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary>
        /// Gets the predefined list of date types
        /// </summary>
        public static List<DateType> GetDateTypes()
        {
            return new List<DateType>
            {
                new DateType { Code = "Financial Period", DisplayName = "Financial Period" },
                new DateType { Code = "Invoice Date",     DisplayName = "Invoice Date" },
                new DateType { Code = "Attach Date",      DisplayName = "Attach Date" }
            };
        }
    }
}
