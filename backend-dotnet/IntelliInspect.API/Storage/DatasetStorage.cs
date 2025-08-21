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
            var header = reader.ReadLine();
            if (header == null) return null;

            var headers = header.Split(',').Select(h => h.Trim()).ToList();
            int idxTs = headers.FindIndex(h => string.Equals(h, "synthetic_timestamp", StringComparison.OrdinalIgnoreCase));
            int idxA = headers.FindIndex(h => string.Equals(h, "Sensor_A", StringComparison.OrdinalIgnoreCase));
            int idxB = headers.FindIndex(h => string.Equals(h, "Sensor_B", StringComparison.OrdinalIgnoreCase));
            int idxC = headers.FindIndex(h => string.Equals(h, "Sensor_C", StringComparison.OrdinalIgnoreCase));
            int idxTemp = headers.FindIndex(h => string.Equals(h, "Temperature", StringComparison.OrdinalIgnoreCase));
            int idxPress = headers.FindIndex(h => string.Equals(h, "Pressure", StringComparison.OrdinalIgnoreCase));
            int idxHum = headers.FindIndex(h => string.Equals(h, "Humidity", StringComparison.OrdinalIgnoreCase));
            int idxResp = headers.FindIndex(h => string.Equals(h, "Response", StringComparison.OrdinalIgnoreCase));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                try
                {
                    DateTime ts = DateTime.Parse(parts[idxTs], CultureInfo.InvariantCulture);
                    double sensorA = idxA >= 0 && idxA < parts.Length ? double.Parse(parts[idxA], CultureInfo.InvariantCulture) : 0;
                    double sensorB = idxB >= 0 && idxB < parts.Length ? double.Parse(parts[idxB], CultureInfo.InvariantCulture) : 0;
                    double sensorC = idxC >= 0 && idxC < parts.Length ? double.Parse(parts[idxC], CultureInfo.InvariantCulture) : 0;
                    double temp = idxTemp >= 0 && idxTemp < parts.Length ? double.Parse(parts[idxTemp], CultureInfo.InvariantCulture) : 0;
                    int press = idxPress >= 0 && idxPress < parts.Length ? int.Parse(parts[idxPress], CultureInfo.InvariantCulture) : 0;
                    int hum = idxHum >= 0 && idxHum < parts.Length ? int.Parse(parts[idxHum], CultureInfo.InvariantCulture) : 0;
                    int resp = idxResp >= 0 && idxResp < parts.Length ? int.Parse(parts[idxResp], CultureInfo.InvariantCulture) : 0;

                    rows.Add(new DatasetRow
                    {
                        SyntheticTimestamp = ts,
                        Sensor_A = sensorA,
                        Sensor_B = sensorB,
                        Sensor_C = sensorC,
                        Temperature = temp,
                        Pressure = press,
                        Humidity = hum,
                        Response = resp
                    });
                }
                catch
                {
                    // ignore bad row
                }
            }

            return rows;
        }
    }
}
