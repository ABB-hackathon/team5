using System.Collections.Generic;

namespace IntelliInspect.API.Models
{
    public class TrainPoint
    {
        public int Epoch { get; set; }
        public double Training { get; set; }
        public double Validation { get; set; }
    }

    public class TrainModelResponse
    {
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public List<TrainPoint>? TrainingAccuracy { get; set; }
        public List<TrainPoint>? TrainingLoss { get; set; }
        public Dictionary<string, int>? ConfusionMatrix { get; set; }

        // Optional charts (Base64 strings so frontend can render images)
        public string? TrainingChartBase64 { get; set; }
        public string? ConfusionMatrixBase64 { get; set; }

        public string Status { get; set; } = "Success";
        public string Message { get; set; } = "Model trained successfully.";
    }
}
