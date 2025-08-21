namespace IntelliInspect.API.Models
{
    public class PeriodSummary
    {
        public int Records { get; set; }
        public int Days { get; set; }
        public string Range { get; set; } = string.Empty;
    }

    public class MonthlyCount
    {
        public string Month { get; set; } = string.Empty;
        public int Records { get; set; }
    }

    public class DailyCount
    {
        public string Date { get; set; } = string.Empty;
        public int Records { get; set; }
    }

    public class DateRangeResponse
    {
        public string Status { get; set; } = "Invalid";
        public PeriodSummary Training { get; set; } = new();
        public PeriodSummary Testing { get; set; } = new();
        public PeriodSummary Simulation { get; set; } = new();
        public List<MonthlyCount> MonthlyCounts { get; set; } = new();
        public List<DailyCount> DailyCounts { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
