using System.Collections.Concurrent;
using IntelliInspect.API.Models;
using System.Globalization;

namespace IntelliInspect.API.Storage
{
    public static class DatasetStorage
    {
        private static readonly ConcurrentDictionary<string, string> datasets = new();
        private static string? lastUploadedFile; // <-- track most recent upload

        public static void Save(string fileName, string filePath)
        {
            datasets[fileName] = filePath;
            lastUploadedFile = fileName; // remember it
        }

        public static string? GetPath(string fileName)
        {
            return datasets.TryGetValue(fileName, out var path) ? path : null;
        }

        public static string? GetLastUploadedFile()
        {
            return lastUploadedFile;
        }

        public static IReadOnlyDictionary<string, string> GetAll()
        {
            return datasets;
        }

        // ✅ Load dataset rows from stored file path
        public static List<DatasetRow>? LoadDataset(string fileName)
        {
            if (!datasets.TryGetValue(fileName, out var path) || !File.Exists(path))
                return null;

            var rows = new List<DatasetRow>();
            using var reader = new StreamReader(path);
            var header = reader.ReadLine(); // skip header
            if (header == null) return null;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');

                try
                {
                    var row = new DatasetRow
                    {
                        SyntheticTimestamp = DateTime.Parse(parts[0], CultureInfo.InvariantCulture),
                        Sensor_A = double.Parse(parts[1], CultureInfo.InvariantCulture),
                        Sensor_B = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        Sensor_C = double.Parse(parts[3], CultureInfo.InvariantCulture),
                        Temperature = double.Parse(parts[4], CultureInfo.InvariantCulture),
                        Pressure = int.Parse(parts[5], CultureInfo.InvariantCulture),
                        Humidity = int.Parse(parts[6], CultureInfo.InvariantCulture),
                        Response = int.Parse(parts[7], CultureInfo.InvariantCulture)
                    };
                    rows.Add(row);
                }
                catch
                {
                    // TODO: log/handle bad row
                }
            }

            return rows;
        }
    }
}
