namespace IntelliInspect.API.Models
{
    public class DatasetMetadata
    {
        public string FileName { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int TotalColumns { get; set; }
        public double PassRate { get; set; }
        public DateTime StartTimestamp { get; set; }
        public DateTime EndTimestamp { get; set; }
    }
}
