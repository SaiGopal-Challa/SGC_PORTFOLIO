using SGC_PORTFOLIO.Models.Entities;
using SGC_PORTFOLIO.Services.CsvHelpers;
using System.IO;

namespace SGC_PORTFOLIO.Services
{
    public class PdfClusterMappingService : IDisposable
    {
        private readonly string _clusterInfoCsvPath;
        private readonly string _mappingCsvPath;

        public PdfClusterMappingService()
        {
            var basePath = GetProjectRoot();
            _clusterInfoCsvPath = Path.Combine(basePath, "App_Data", "CSV", "ClusterInfo.csv");
            _mappingCsvPath = Path.Combine(basePath, "App_Data", "CSV", "PdfPdfClusterMapping.csv");

            // Ensure the CSV exists with a header
            if (!File.Exists(_mappingCsvPath))
            {
                var header = "PdfId,ClusterId";
                File.WriteAllText(_mappingCsvPath, header + Environment.NewLine);
            }
        }

        private string GetProjectRoot()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = new DirectoryInfo(baseDir);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SGC_PORTFOLIO.csproj")))
            {
                dir = dir.Parent;
            }
            return dir?.FullName ?? baseDir;
        }

        public void MapPdfToClusters(string pdfId, List<string> pdfAgeTags, List<string> pdfExpTags, List<string> pdfTopicTags)
        {
            // 1) Read all clusters
            var clusters = CsvReaderUtility.ReadAll<ClusterInfoDetail>(_clusterInfoCsvPath);

            var clusterRepDict = clusters.ToDictionary(
                c => c.ClusterId,
                c => new HashSet<string>(c.GetRepresentativeTagList(), StringComparer.OrdinalIgnoreCase));

            // 2) Build a single set of all PDF tags
            var pdfAllTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in pdfAgeTags) pdfAllTags.Add(t);
            foreach (var t in pdfExpTags) pdfAllTags.Add(t);
            foreach (var t in pdfTopicTags) pdfAllTags.Add(t);

            var matchingClusters = new List<int>();
            foreach (var kvp in clusterRepDict)
            {
                if (kvp.Value.Overlaps(pdfAllTags))
                    matchingClusters.Add(kvp.Key);
            }

            // 3) Read existing mappings and remove any lines for this pdfId
            var existingLines = File.ReadAllLines(_mappingCsvPath).Skip(1).ToList();
            var newLines = existingLines
                .Where(line => !line.StartsWith(pdfId + ",", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 4) Append new lines for each matching cluster
            foreach (var cid in matchingClusters)
                newLines.Add($"{pdfId},{cid}");

            // 5) Overwrite mapping CSV
            using var writer = new StreamWriter(_mappingCsvPath, false);
            writer.WriteLine("PdfId,ClusterId");
            foreach (var line in newLines)
                writer.WriteLine(line);
        }

        public void Dispose()
        {
            // nothing to dispose for now
        }
    }
}
