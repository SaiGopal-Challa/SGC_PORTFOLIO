namespace SGC_PORTFOLIO.Models
{
    public class BlogEntry
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public List<ContentSection> Content { get; set; } = new List<ContentSection>();
        public DateTime CreatedAt { get; set; }
    }

    public class ContentSection_old
    {
        public string Text { get; set; }
        public string ImagePath { get; set; }
    }

    public class ContentSection
    {
        public string Text { get; set; }
        public List<string> ImagePaths { get; set; } = new List<string>(); // Now supports multiple images per section
    }
}
