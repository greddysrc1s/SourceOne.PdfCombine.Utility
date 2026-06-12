namespace SourceOne.PdfCombine.Utility.Models
{
    /// <summary>
    /// Represents a job from the JCJM table
    /// </summary>
    public class Job
    {
        public string? JobNumber { get; set; }
        public string? Description { get; set; }
        public string? JobName { get; set; }

        public override string ToString()
        {
            return JobName ?? Description ?? string.Empty;
        }
    }
}
