using CsvHelper.Configuration.Attributes;

namespace SGC_PORTFOLIO.Models.Entities
{
    public class PdfMetadataDetail
    {
        [Index(0)]
        public string PdfId { get; set; }

        [Index(1)]
        public string FileName { get; set; }

        /// <summary>
        /// The one-paragraph summary returned by GenAI.
        /// </summary>
        [Index(2)]
        public string Summary { get; set; }

        /// <summary>
        /// Semicolon‐delimited age tags (e.g., "18+;25+").
        /// </summary>
        [Index(3)]
        public string AgeTags { get; set; }

        /// <summary>
        /// Semicolon‐delimited experience tags (e.g., "Junior;Senior").
        /// </summary>
        [Index(4)]
        public string ExpTags { get; set; }

        /// <summary>
        /// Semicolon‐delimited topic tags returned by GenAI.
        /// </summary>
        [Index(5)]
        public string TopicTags { get; set; }

        public List<string> GetAgeTagList()
        {
            if (string.IsNullOrWhiteSpace(AgeTags))
                return new List<string>();
            return new List<string>(AgeTags.Split(';', StringSplitOptions.RemoveEmptyEntries));
        }

        public List<string> GetExpTagList()
        {
            if (string.IsNullOrWhiteSpace(ExpTags))
                return new List<string>();
            return new List<string>(ExpTags.Split(';', StringSplitOptions.RemoveEmptyEntries));
        }

        public List<string> GetTopicTagList()
        {
            if (string.IsNullOrWhiteSpace(TopicTags))
                return new List<string>();
            return new List<string>(TopicTags.Split(';', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
