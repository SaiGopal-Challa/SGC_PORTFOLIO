using SGC_PORTFOLIO.Models;

namespace SGC_PORTFOLIO.Services
{
    public class BlogServiceOld
    {
        private readonly string _blogTextFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BlogTexts");

        public List<BlogEntry> GetBlogEntries()
        {
            var blogEntries = new List<BlogEntry>();

            foreach (var filePath in Directory.GetFiles(_blogTextFolder, "*.txt"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var fileContent = File.ReadAllText(filePath);
                var sections = fileContent.Split(new[] { "\n\n" }, StringSplitOptions.None);

                var blogEntry = new BlogEntry
                {
                    Title = fileName,
                    Summary = sections.Length > 0 ? sections[0] : string.Empty,
                    //Content = sections.Length > 1 ? sections[1] : string.Empty,
                    CreatedAt = File.GetCreationTime(filePath)
                };

                blogEntries.Add(blogEntry);
            }

            return blogEntries.OrderByDescending(b => b.CreatedAt).ToList();
        }
    }
}
