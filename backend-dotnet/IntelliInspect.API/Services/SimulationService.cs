using IntelliInspect.API.Models;
using IntelliInspect.API.Storage;
using System.Net.Http.Json;

namespace IntelliInspect.API.Services
{
    public class SimulationService
    {
        private readonly HttpClient _httpClient;

        public SimulationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SimulationResponse> RunAsync(SimulationRequest request)
        {
            var fileName = DatasetStorage.GetLastUploadedFile();
            if (fileName == null)
            {
                return new SimulationResponse { Status = "Invalid", Message = "No dataset uploaded yet." };
            }

            var dataset = DatasetStorage.LoadDataset(fileName);
            if (dataset == null || !dataset.Any())
            {
                return new SimulationResponse { Status = "Invalid", Message = "Failed to load dataset." };
            }

            var rows = dataset
                .Where(r => r.SyntheticTimestamp >= request.Start && r.SyntheticTimestamp <= request.End)
                .OrderBy(r => r.SyntheticTimestamp)
                .ToList();

            if (!rows.Any())
            {
                return new SimulationResponse { Status = "Invalid", Message = "No rows in selected simulation period." };
            }

            // Build row dictionaries compatible with ML /predict
            var payload = new
            {
                rows = rows.Select((r, idx) => new Dictionary<string, object?>
                {
                    ["synthetic_timestamp"] = r.SyntheticTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["Id"] = idx + 1,
                    ["Sensor_A"] = r.Sensor_A,
                    ["Sensor_B"] = r.Sensor_B,
                    ["Sensor_C"] = r.Sensor_C,
                    // Lower-case keys so ML response echoes them into fields
                    ["temperature"] = r.Temperature,
                    ["pressure"] = r.Pressure,
                    ["humidity"] = r.Humidity,
                    // Include Response even though ML drops it
                    ["Response"] = r.Response
                }).ToList()
            };

            var mlServiceUrl = Environment.GetEnvironmentVariable("ML_SERVICE_URL")
                               ?? "http://localhost:8000";

            var response = await _httpClient.PostAsJsonAsync($"{mlServiceUrl}/predict", payload);
            if (!response.IsSuccessStatusCode)
            {
                return new SimulationResponse { Status = "Error", Message = $"ML service error: {response.StatusCode}" };
            }

            var predictions = await response.Content.ReadFromJsonAsync<List<SimulationRow>>()
                               ?? new List<SimulationRow>();

            var stats = new SimulationStats
            {
                Total = predictions.Count,
                Pass = predictions.Count(p => p.Prediction == "Pass"),
                Fail = predictions.Count(p => p.Prediction == "Fail"),
                AverageConfidence = predictions.Any() ? predictions.Average(p => p.Confidence) : 0
            };

            return new SimulationResponse
            {
                Rows = predictions,
                Stats = stats,
                Status = "Success",
                Message = $"Simulated {stats.Total} rows"
            };
        }
    }
}



