using IntelliInspect.API.Models;
using IntelliInspect.API.Storage;

namespace IntelliInspect.API.Services
{
    public class DatasetService
    {
        private readonly string storagePath = Path.Combine(Path.GetTempPath(), "intelliinspect");

        public DatasetService()
        {
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);
        }

        public DatasetMetadata ProcessCsv(Stream fileStream, string fileName)
        {
            Console.WriteLine(">>> DEBUG: Running NO-CsvHelper version");

            var records = new List<Dictionary<string, string>>();
            List<string> headers;

            using (var reader = new StreamReader(fileStream))
            {
                // Read headers
                var headerLine = reader.ReadLine();
                if (headerLine == null)
                    throw new Exception("CSV is empty.");

                headers = headerLine.Split(',').Select(h => h.Trim()).ToList();

                if (!headers.Contains("Response"))
                    throw new Exception("CSV must contain 'Response' column.");

                // Read rows
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(',');
                    var record = new Dictionary<string, string>();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        if (i < values.Length)
                            record[headers[i]] = values[i].Trim();
                        else
                            record[headers[i]] = "";
                    }
                    records.Add(record);
                }
            }

            // Add synthetic timestamp if missing
            bool hasTimestamp = headers.Contains("synthetic_timestamp");
            if (!hasTimestamp) headers.Add("synthetic_timestamp");

            DateTime start = new DateTime(2021, 1, 1, 0, 0, 0);
            for (int i = 0; i < records.Count; i++)
            {
                if (!hasTimestamp)
                    records[i]["synthetic_timestamp"] = start.AddSeconds(i).ToString("yyyy-MM-dd HH:mm:ss");
            }

            // Metadata
            var timestamps = records.Select(r => DateTime.Parse(r["synthetic_timestamp"])).ToList();
            int total = records.Count;
            int cols = headers.Count;
            double passRate = records.Count(r => r["Response"] == "1") / (double)total;

            // Save processed dataset
            var savePath = Path.Combine(storagePath, fileName);
            using (var writer = new StreamWriter(savePath))
            {
                writer.WriteLine(string.Join(",", headers));

                foreach (var record in records)
                {
                    var row = headers.Select(h => record.ContainsKey(h) ? record[h] : "").ToArray();
                    writer.WriteLine(string.Join(",", row));
                }
            }

            DatasetStorage.Save(fileName, savePath);

            // Return metadata
            return new DatasetMetadata
            {
                FileName = fileName,
                TotalRecords = total,
                TotalColumns = cols,
                PassRate = passRate,
                StartTimestamp = timestamps.Min(),
                EndTimestamp = timestamps.Max()
            };
        }
    }
}
