using SGC_PORTFOLIO.Models.Entities;
using SGC_PORTFOLIO.Services.CsvHelpers;
using System.IO;

namespace SGC_PORTFOLIO.Services
{
    public class TagSummaryService
    {
        private readonly string _incomingFolder;
        private readonly string _processedFolder;
        private readonly string _pdfMetadataCsvPath;
        private readonly GenAiClient _genAiClient;

        public TagSummaryService(GenAiClient genAiClient)
        {
            var basePath = GetProjectRoot();
            _incomingFolder = Path.Combine(basePath, "App_Data", "Newsletters", "Incoming");
            _processedFolder = Path.Combine(basePath, "App_Data", "Newsletters", "Processed");
            _pdfMetadataCsvPath = Path.Combine(basePath, "App_Data", "CSV", "PdfMetadata.csv");
            _genAiClient = genAiClient;

            Directory.CreateDirectory(_incomingFolder);
            Directory.CreateDirectory(_processedFolder);

            if (!File.Exists(_pdfMetadataCsvPath))
            {
                var header = "PdfId,FileName,Summary,AgeTags,ExpTags,TopicTags";
                File.WriteAllText(_pdfMetadataCsvPath, header + Environment.NewLine);
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

        public async Task<(string pdfId, List<string> ageTags, List<string> expTags, List<string> topicTags)> ProcessPdfAsync(string incomingFilePath)
        {
            // 1. New GUID
            var pdfId = Guid.NewGuid().ToString("N");
            var originalFileName = Path.GetFileName(incomingFilePath);

            // 2. Move to Processed (keep original name)
            var destPath = Path.Combine(_processedFolder, originalFileName);
            if (File.Exists(destPath))
                File.Delete(destPath);
            File.Move(incomingFilePath, destPath);

            // 3. Append placeholder to PdfMetadata.csv (store originalFileName)
            var placeholderLine = $"{pdfId},{originalFileName},\"\",\"\",\"\",\"\"";
            CsvWriterUtility.AppendLine(_pdfMetadataCsvPath, placeholderLine);

            // 4. Extract text from pdf as string
            var rawText = PdfTextExtractor.Extract(destPath);

            // 5. Call GenAI
            var (summary, ageTags, expTags, topicTags) = await _genAiClient.AnalyzePdfTextAsync(rawText);

            // 6. Update that record in PdfMetadata.csv 
            var allMeta = CsvReaderUtility.ReadAll<PdfMetadataDetail>(_pdfMetadataCsvPath);
            var entry = allMeta.FirstOrDefault(p => p.PdfId == pdfId);
            if (entry != null)
            {
                entry.FileName = originalFileName;
                entry.Summary = summary.Replace("\n", " ").Replace("\r", "");
                entry.AgeTags = string.Join(';', ageTags);
                entry.ExpTags = string.Join(';', expTags);
                entry.TopicTags = string.Join(';', topicTags);
            }
            CsvWriterUtility.WriteAll(_pdfMetadataCsvPath, allMeta);

            return (pdfId, ageTags, expTags, topicTags);
        }
    }
}
