using IntelliInspect.API.Models;
using IntelliInspect.API.Storage;
using System.Linq;

namespace IntelliInspect.API.Services
{
    public class DateRangeService
    {
        public DateRangeResponse ValidateRanges(DateRangeRequest request)
        {
            var fileName = DatasetStorage.GetLastUploadedFile();
            if (fileName == null)
            {
                return new DateRangeResponse
                {
                    Status = "Invalid",
                    Message = "No dataset uploaded yet."
                };
            }

            var dataset = DatasetStorage.LoadDataset(fileName);
            if (dataset == null || !dataset.Any())
            {
                return new DateRangeResponse
                {
                    Status = "Invalid",
                    Message = "Failed to load dataset."
                };
            }

            // ✅ dataset bounds
            var minDate = dataset.Min(r => r.SyntheticTimestamp);
            var maxDate = dataset.Max(r => r.SyntheticTimestamp);

            // ✅ validations
            if (request.TrainStart > request.TrainEnd ||
                request.TestStart > request.TestEnd ||
                request.SimStart > request.SimEnd)
            {
                return new DateRangeResponse
                {
                    Status = "Invalid",
                    Message = "One or more ranges have start date after end date."
                };
            }

            if (!(request.TrainEnd < request.TestStart &&
                  request.TestEnd < request.SimStart))
            {
                return new DateRangeResponse
                {
                    Status = "Invalid",
                    Message = "Ranges must be sequential and non-overlapping."
                };
            }

            if (request.TrainStart < minDate || request.SimEnd > maxDate)
            {
                return new DateRangeResponse
                {
                    Status = "Invalid",
                    Message = "Selected ranges fall outside dataset bounds."
                };
            }

            // ✅ counts
            var trainRecords = dataset.Where(r => r.SyntheticTimestamp >= request.TrainStart &&
                                                  r.SyntheticTimestamp <= request.TrainEnd).ToList();
            var testRecords = dataset.Where(r => r.SyntheticTimestamp >= request.TestStart &&
                                                 r.SyntheticTimestamp <= request.TestEnd).ToList();
            var simRecords = dataset.Where(r => r.SyntheticTimestamp >= request.SimStart &&
                                                r.SyntheticTimestamp <= request.SimEnd).ToList();

            // ✅ monthly breakdown
            var monthlyCounts = dataset
                .GroupBy(r => r.SyntheticTimestamp.ToString("yyyy-MM"))
                .Select(g => new MonthlyCount { Month = g.Key, Records = g.Count() })
                .OrderBy(m => m.Month)
                .ToList();

            // ✅ daily breakdown (for timeline bars)
            var dailyCounts = dataset
                .GroupBy(r => r.SyntheticTimestamp.ToString("yyyy-MM-dd"))
                .Select(g => new DailyCount { Date = g.Key, Records = g.Count() })
                .OrderBy(d => d.Date)
                .ToList();

            return new DateRangeResponse
            {
                Status = "Valid",
                Message = "Date ranges validated successfully.",
                Training = new PeriodSummary
                {
                    Records = trainRecords.Count,
                    Days = (request.TrainEnd - request.TrainStart).Days + 1,
                    Range = $"{request.TrainStart:yyyy-MM-dd} to {request.TrainEnd:yyyy-MM-dd}"
                },
                Testing = new PeriodSummary
                {
                    Records = testRecords.Count,
                    Days = (request.TestEnd - request.TestStart).Days + 1,
                    Range = $"{request.TestStart:yyyy-MM-dd} to {request.TestEnd:yyyy-MM-dd}"
                },
                Simulation = new PeriodSummary
                {
                    Records = simRecords.Count,
                    Days = (request.SimEnd - request.SimStart).Days + 1,
                    Range = $"{request.SimStart:yyyy-MM-dd} to {request.SimEnd:yyyy-MM-dd}"
                },
                MonthlyCounts = monthlyCounts,
                DailyCounts = dailyCounts
            };
        }
    }
}
