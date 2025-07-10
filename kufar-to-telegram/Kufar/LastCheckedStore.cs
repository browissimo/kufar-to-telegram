using System;
using System.IO;
using System.Threading.Tasks;

namespace KufarParserApp.Storage
{
    public class LastCheckedStore
    {
        private readonly string _filePath;

        public LastCheckedStore(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<DateTime?> LoadLastCheckedAsync()
        {
            if (!File.Exists(_filePath))
                return new DateTime(2025, 7, 10);

            var content = await File.ReadAllTextAsync(_filePath);
            if (DateTime.TryParse(content, out var dt))
                return dt;

            return new DateTime(2025, 6, 30); ;
        }

        public async Task SaveLastCheckedAsync(DateTime dateTime)
        {
            await File.WriteAllTextAsync(_filePath, dateTime.ToString("O"));
        }
    }
}
