namespace IntelliInspect.API.Models
{
    public class TrainModelResponse
    {
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }

        // Optional charts (Base64 strings so frontend can render images)
        public string? TrainingChartBase64 { get; set; }
        public string? ConfusionMatrixBase64 { get; set; }

        public string Status { get; set; } = "Success";
        public string Message { get; set; } = "Model trained successfully.";
    }
}
