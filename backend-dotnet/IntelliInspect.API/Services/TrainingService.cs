using IntelliInspect.API.Models;
using IntelliInspect.API.Storage;

namespace IntelliInspect.API.Services
{
    public class TrainingService
    {
        private readonly HttpClient _httpClient;

        public TrainingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TrainModelResponse> TrainAsync(TrainModelRequest request)
        {
            var fileName = DatasetStorage.GetLastUploadedFile();
            if (fileName == null)
            {
                return new TrainModelResponse
                {
                    Status = "Invalid",
                    Message = "No dataset uploaded yet."
                };
            }

            var dataset = DatasetStorage.LoadDataset(fileName);
            if (dataset == null || !dataset.Any())
            {
                return new TrainModelResponse
                {
                    Status = "Invalid",
                    Message = "Failed to load dataset."
                };
            }

            // Slice dataset
            var trainData = dataset.Where(r => r.SyntheticTimestamp >= request.TrainStart &&
                                               r.SyntheticTimestamp <= request.TrainEnd).ToList();

            var testData = dataset.Where(r => r.SyntheticTimestamp >= request.TestStart &&
                                              r.SyntheticTimestamp <= request.TestEnd).ToList();

            if (!trainData.Any() || !testData.Any())
            {
                return new TrainModelResponse
                {
                    Status = "Invalid",
                    Message = "No records found in selected ranges."
                };
            }

            // ⚡ Stubbed response for now (replace with Python ML service call later)
            await Task.Delay(500); // simulate training delay
            return new TrainModelResponse
            {
                Accuracy = 0.87,
                Precision = 0.85,
                Recall = 0.83,
                F1Score = 0.84,
                Status = "Success",
                Message = $"Model trained on {trainData.Count} training records and {testData.Count} testing records."
            };
        }
    }
}
