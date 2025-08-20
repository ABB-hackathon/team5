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

            // Prepare payload
            var payload = new
            {
            trainData,
            testData
            };

            // Decide base URL (local dev vs docker)
            var mlServiceUrl = Environment.GetEnvironmentVariable("ML_SERVICE_URL") 
                               ?? "http://localhost:8000"; // fallback for dev

            // Call ML service
            var response = await _httpClient.PostAsJsonAsync($"{mlServiceUrl}/train", payload);

            if (!response.IsSuccessStatusCode)
            {
                return new TrainModelResponse
                {
                    Status = "Error",
                    Message = $"Failed to call ML service: {response.StatusCode}"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<TrainModelResponse>();
            return result ?? new TrainModelResponse
            {
                Status = "Error",
                Message = "Invalid response from ML service."
            };

        }
    }
}
