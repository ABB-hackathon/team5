namespace IntelliInspect.API.Models
{
    public class SimulationRequest
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class SimulationRow
    {
        public string Timestamp { get; set; } = string.Empty; // maps from timestamp
        public string? SampleId { get; set; } // maps from sample_id
        public string Prediction { get; set; } = string.Empty; // Pass/Fail
        public double Confidence { get; set; }
        public double? Temperature { get; set; }
        public double? Pressure { get; set; }
        public double? Humidity { get; set; }
    }

    public class SimulationStats
    {
        public int Total { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public double AverageConfidence { get; set; }
    }

    public class SimulationResponse
    {
        public List<SimulationRow> Rows { get; set; } = new();
        public SimulationStats Stats { get; set; } = new();
        public string Status { get; set; } = "Success";
        public string Message { get; set; } = string.Empty;
    }
}



