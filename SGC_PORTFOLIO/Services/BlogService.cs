using SGC_PORTFOLIO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SGC_PORTFOLIO.Services
{
    public class BlogService
    {
        private readonly string _blogTextFolder;

        public BlogService()
        {
            _blogTextFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BlogTexts");
        }

        public List<BlogEntry> GetBlogEntries()
        {
            var blogEntries = new List<BlogEntry>();

            if (!Directory.Exists(_blogTextFolder))
            {
                Console.WriteLine($"Blog text folder not found at {_blogTextFolder}");
                return blogEntries;
            }

            foreach (var filePath in Directory.GetFiles(_blogTextFolder, "*.txt"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var fileContent = File.ReadAllText(filePath);

                var summaries = ExtractAllSections(fileContent, "Summary", "SummaryEnd");
                var contentSections = ExtractAllContentSections(fileContent);

                var blogEntry = new BlogEntry
                {
                    Title = fileName,
                    Summary = string.Join("\n\n", summaries.Select(s => s.Text)), // Combine summaries as single text
                    Content = contentSections,
                    CreatedAt = File.GetCreationTime(filePath)
                };

                blogEntries.Add(blogEntry);
            }

            return blogEntries.OrderByDescending(b => b.CreatedAt).ToList();
        }

        private List<ContentSection> ExtractAllSections(string content, string sectionName, string sectionEndName)
        {
            var sections = new List<ContentSection>();
            content = NormalizeLineEndings(content);

            int currentIndex = 0;
            while (currentIndex < content.Length)
            {
                var sectionStart = content.IndexOf($"{sectionName}\n", currentIndex, StringComparison.Ordinal);
                if (sectionStart == -1) break;

                sectionStart += $"{sectionName}\n".Length;
                var sectionEnd = content.IndexOf(sectionEndName, sectionStart, StringComparison.Ordinal);
                if (sectionEnd == -1) break;

                var sectionText = content.Substring(sectionStart, sectionEnd - sectionStart).Trim();
                sections.Add(new ContentSection { Text = sectionText });

                currentIndex = sectionEnd + sectionEndName.Length;
            }

            return sections;
        }

        private List<ContentSection> ExtractAllContentSections(string content)
        {
            var contentSections = new List<ContentSection>();
            content = NormalizeLineEndings(content);

            int currentIndex = 0;
            while (currentIndex < content.Length)
            {
                // Find "Content" section
                var contentStart = content.IndexOf("Content\n", currentIndex, StringComparison.Ordinal);
                if (contentStart == -1) break;

                contentStart += "Content\n".Length;
                var contentEnd = content.IndexOf("ContentEnd", contentStart, StringComparison.Ordinal);
                if (contentEnd == -1) break;

                var sectionText = content.Substring(contentStart, contentEnd - contentStart).Trim();
                currentIndex = contentEnd + "ContentEnd".Length;

                // Check for multiple "Picture" sections after "Content"
                var pictures = new List<string>();
                while (true)
                {
                    var pictureStart = content.IndexOf("Picture\n", currentIndex, StringComparison.Ordinal);
                    if (pictureStart == -1 || pictureStart > content.IndexOf("Content\n", currentIndex, StringComparison.Ordinal))
                        break;

                    pictureStart += "Picture\n".Length;
                    var pictureEnd = content.IndexOf("PictureEnd", pictureStart, StringComparison.Ordinal);
                    if (pictureEnd == -1) break;

                    var imagePath = content.Substring(pictureStart, pictureEnd - pictureStart).Trim();
                    pictures.Add(imagePath);
                    currentIndex = pictureEnd + "PictureEnd".Length;
                }

                contentSections.Add(new ContentSection
                {
                    Text = sectionText,
                    ImagePaths = pictures
                });
            }

            return contentSections;
        }

        private string NormalizeLineEndings(string content)
        {
            return content.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}




















/*
namespace SGC_PORTFOLIO.Services
{
    public class BlogService
    {
        private readonly string _blogTextFolder;

        public BlogService()
        {
            _blogTextFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BlogTexts");
        }

        public List<BlogEntry> GetBlogEntries()
        {
            var blogEntries = new List<BlogEntry>();

            if (!Directory.Exists(_blogTextFolder))
            {
                Console.WriteLine($"Blog text folder not found at {_blogTextFolder}");
                return blogEntries;
            }

            foreach (var filePath in Directory.GetFiles(_blogTextFolder, "*.txt"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var fileContent = File.ReadAllText(filePath);

                var summary = ExtractSection(fileContent, "Summary", "SummaryEnd");
                var contentSections = ExtractAllSections(fileContent);

                var blogEntry = new BlogEntry
                {
                    Title = fileName,
                    Summary = summary,
                    Content = contentSections,
                    CreatedAt = File.GetCreationTime(filePath)
                };

                blogEntries.Add(blogEntry);
            }

            return blogEntries.OrderByDescending(b => b.CreatedAt).ToList();
        }

        private string ExtractSection(string content, string sectionName, string sectionEndName)
        {
            content = NormalizeLineEndings(content);

            var sectionStart = content.IndexOf($"{sectionName}\n", StringComparison.Ordinal);
            if (sectionStart == -1)
            {
                Console.WriteLine($"Section '{sectionName}' not found.");
                return string.Empty;
            }

            sectionStart += $"{sectionName}\n".Length;
            var sectionEnd = content.IndexOf(sectionEndName, sectionStart, StringComparison.Ordinal);
            if (sectionEnd == -1)
            {
                Console.WriteLine($"Section end '{sectionEndName}' not found.");
                return string.Empty;
            }

            return content.Substring(sectionStart, sectionEnd - sectionStart).Trim();
        }

        private List<ContentSection> ExtractAllSections(string content)
        {
            var contentSections = new List<ContentSection>();
            content = NormalizeLineEndings(content);

            var currentIndex = 0;
            while (currentIndex < content.Length)
            {
                var contentStart = content.IndexOf("Content\n", currentIndex, StringComparison.Ordinal);
                if (contentStart == -1)
                    break;

                contentStart += "Content\n".Length;
                var contentEnd = content.IndexOf("ContentEnd", contentStart, StringComparison.Ordinal);
                if (contentEnd == -1)
                    break;

                var sectionText = content.Substring(contentStart, contentEnd - contentStart).Trim();
                currentIndex = contentEnd + "ContentEnd".Length;

                var pictureStart = content.IndexOf("Picture\n", currentIndex, StringComparison.Ordinal);
                if (pictureStart != -1 && pictureStart < content.IndexOf("Content\n", currentIndex, StringComparison.Ordinal))
                {
                    pictureStart += "Picture\n".Length;
                    var pictureEnd = content.IndexOf("PictureEnd", pictureStart, StringComparison.Ordinal);
                    if (pictureEnd != -1)
                    {
                        var imagePath = content.Substring(pictureStart, pictureEnd - pictureStart).Trim();
                        currentIndex = pictureEnd + "PictureEnd".Length;

                        contentSections.Add(new ContentSection { Text = sectionText, ImagePath = imagePath });
                        continue;
                    }
                }

                contentSections.Add(new ContentSection { Text = sectionText });
            }

            return contentSections;
        }

        private string NormalizeLineEndings(string content)
        {
            return content.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}*/
