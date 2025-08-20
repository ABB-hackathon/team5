using System.Collections.Concurrent;

namespace IntelliInspect.API.Storage
{
    public static class DatasetStorage
    {
        // In-memory storage for now (could be swapped with database/file system later)
        private static readonly ConcurrentDictionary<string, string> datasets = new();

        // Save processed file path for reuse
        public static void Save(string fileName, string filePath)
        {
            datasets[fileName] = filePath;
        }

        // Retrieve stored dataset path
        public static string? Get(string fileName)
        {
            return datasets.TryGetValue(fileName, out var path) ? path : null;
        }

        // Get all stored datasets (for admin/history features later)
        public static IReadOnlyDictionary<string, string> GetAll()
        {
            return datasets;
        }
    }
}
