using System.Globalization;
using CsvHelper;

namespace SGC_PORTFOLIO.Services.CsvHelpers
{
    public static class CsvWriterUtility
    {
        /// <summary>
        /// Overwrites the CSV at csvPath with the provided records (including header).
        /// </summary>
        public static void WriteAll<T>(string csvPath, List<T> records)
        {
            using var writer = new StreamWriter(csvPath, false); // overwrite
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteHeader<T>();
            csv.NextRecord();
            csv.WriteRecords(records);
        }

        /// <summary>
        /// Appends a raw line to the CSV (no header). Use only if you know the exact CSV format.
        /// </summary>
        public static void AppendLine(string csvPath, string line)
        {
            using var writer = new StreamWriter(csvPath, true);
            writer.WriteLine(line);
        }
    }
}
