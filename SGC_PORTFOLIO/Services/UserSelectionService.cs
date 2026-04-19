using System.Globalization;
using SGC_PORTFOLIO.Models;
using SGC_PORTFOLIO.Models.Entities;
using SGC_PORTFOLIO.Services.CsvHelpers;
using System.IO;

namespace SGC_PORTFOLIO.Services
{
    public class UserSelectionService : IDisposable
    {
        private readonly string _userNewsletterCsvPath;
        private readonly string _pdfClusterMappingCsvPath;
        private readonly string _intermediateFolder;
        private readonly string _pdfMetadataCsvPath;
        private readonly string _projectRoot;

        public UserSelectionService()
        {
            _projectRoot = GetProjectRoot();
            _userNewsletterCsvPath = Path.Combine(_projectRoot, "App_Data", "CSV", "UserNewsletter.csv");
            _pdfClusterMappingCsvPath = Path.Combine(_projectRoot, "App_Data", "CSV", "PdfPdfClusterMapping.csv");
            _intermediateFolder = Path.Combine(_projectRoot, "App_Data", "CSV", "Intermediate");
            _pdfMetadataCsvPath = Path.Combine(_projectRoot, "App_Data", "CSV", "PdfMetadata.csv");

            Directory.CreateDirectory(_intermediateFolder);
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

        /// <summary>
        /// Reads PdfPdfClusterMapping.csv and PdfMetadata.csv → for each pdfId in the given list,
        /// produces {pdfId}_Intermediate.csv under Intermediate/.
        /// </summary>
        public void GenerateIntermediateCsvsForPdfIds(IEnumerable<string> pdfIds)
        {
            var pdfIdSet = new HashSet<string>(pdfIds, StringComparer.OrdinalIgnoreCase);
            var users = CsvReaderUtility.ReadAll<UserNewsletterDetail>(_userNewsletterCsvPath);
            var mappings = CsvReaderUtility
                .ReadAll<PdfClusterMappingDetail>(_pdfClusterMappingCsvPath)
                .GroupBy(m => m.PdfId)
                .ToDictionary(g => g.Key, g => g.Select(m => m.ClusterId).ToList(), StringComparer.OrdinalIgnoreCase);
            var allPdfMeta = CsvReaderUtility.ReadAll<PdfMetadataDetail>(_pdfMetadataCsvPath)
                .ToDictionary(p => p.PdfId, p => p, StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in mappings)
            {
                var pdfId = kvp.Key;
                if (!pdfIdSet.Contains(pdfId)) continue;
                var clusterIds = kvp.Value;
                if (!allPdfMeta.TryGetValue(pdfId, out var meta))
                    continue;
                var pdfAgeTags = meta.GetAgeTagList();
                var pdfExpTags = meta.GetExpTagList();
                var pdfTopicTags = meta.GetTopicTagList();
                int minAgeRequirement = 0;
                foreach (var t in pdfAgeTags)
                {
                    if (t.EndsWith("+", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(t.TrimEnd('+'), out var num))
                            minAgeRequirement = Math.Max(minAgeRequirement, num);
                    }
                }
                var clusterSet = new HashSet<int>(clusterIds);
                var lines = new List<string> { "UserId,Email,ClusterId,Score" };
                foreach (var u in users)
                {
                    if (!clusterSet.Contains(u.ClusterId))
                        continue;
                    bool ageMatch = true;
                    if (minAgeRequirement > 0)
                    {
                        if (int.TryParse(u.AgeTag, NumberStyles.Integer, CultureInfo.InvariantCulture, out var userAge))
                            ageMatch = userAge >= minAgeRequirement;
                        else
                            ageMatch = false;
                    }
                    bool expMatch = !string.IsNullOrWhiteSpace(u.ExperienceTag)
                                    && pdfExpTags.Contains(u.ExperienceTag, StringComparer.OrdinalIgnoreCase);
                    var userTopicSet = new HashSet<string>(u.GetTopicTagList(), StringComparer.OrdinalIgnoreCase);
                    int topicMatches = pdfTopicTags.Count(t => userTopicSet.Contains(t));
                    int score = 2 * topicMatches + (ageMatch ? 1 : 0) + (expMatch ? 1 : 0);
                    if (score >= 2)
                        lines.Add($"{u.UserId},{u.Email},{u.ClusterId},{score}");
                }
                var intermediatePath = Path.Combine(_intermediateFolder, $"{pdfId}_Intermediate.csv");
                File.WriteAllLines(intermediatePath, lines);
            }
        }

        public void Dispose()
        {
            // nothing to dispose right now
        }
    }
}
