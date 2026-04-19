using System.Globalization;
using CsvHelper;

namespace SGC_PORTFOLIO.Services.CsvHelpers
{
    public static class CsvReaderUtility
    {
        /// <summary>
        /// Reads all records of type T from the CSV at csvPath. Assumes a header row.
        /// </summary>
        public static List<T> ReadAll<T>(string csvPath) where T : class, new()
        {
            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            try
            {
                var records = new List<T>();
                foreach (var record in csv.GetRecords<T>())
                {
                    // Defensive: skip records with any null property
                    if (record == null) continue;
                    bool hasNull = false;
                    foreach (var prop in typeof(T).GetProperties())
                    {
                        var val = prop.GetValue(record);
                        if (val == null)
                        {
                            hasNull = true;
                            break;
                        }
                    }
                    if (!hasNull)
                        records.Add(record);
                }
                return records;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CsvReaderUtility] Exception: {ex.Message}");
                return new List<T>();
            }
        }
    }
}
