using SGC_PORTFOLIO.Models;
using System.IO;

namespace SGC_PORTFOLIO.Services
{
    public class BlogService
    {
        private readonly string _blogTextFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BlogTexts");

        public List<BlogEntry> GetBlogEntries()
        {
            var blogEntries = new List<BlogEntry>();

            foreach (var filePath in Directory.GetFiles(_blogTextFolder, "*.txt"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var fileContent = File.ReadAllText(filePath);

                var summary = ExtractSection(fileContent, "Summary", "SummaryEnd");
                var content = ExtractSection(fileContent, "Content", "ContentEnd");

                var blogEntry = new BlogEntry
                {
                    Title = fileName,
                    Summary = summary,
                    Content = content,
                    CreatedAt = File.GetCreationTime(filePath)
                };

                blogEntries.Add(blogEntry);
            }

            return blogEntries.OrderByDescending(b => b.CreatedAt).ToList();
        }

        private string ExtractSection(string content, string sectionName, string sectionEndName)
        {
            // Normalize line endings to \n
            //content = content.Replace("\r\n", "\n");
            // Normalize line endings to \n
            content = content.Replace("\r\n", "\n").Replace("\r", "\n");

            var sectionStart = content.IndexOf(sectionName + "\n");
            if (sectionStart == -1)
            {
                Console.WriteLine($"Section '{sectionName}' not found.");
                return string.Empty;
            }

            sectionStart += sectionName.Length + 1;
            var sectionEnd = content.IndexOf(sectionEndName, sectionStart);
            if (sectionEnd == -1)
            {
                Console.WriteLine($"Section end '{sectionEndName}' not found.");
                return string.Empty;
            }

            var sectionContent = content.Substring(sectionStart, sectionEnd - sectionStart).Trim();
            Console.WriteLine($"Extracted content for '{sectionName}': {sectionContent}");
            return sectionContent;
        }
    }
}
