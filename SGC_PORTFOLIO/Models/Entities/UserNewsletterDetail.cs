using System;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;

namespace SGC_PORTFOLIO.Models.Entities
{
    public class UserNewsletterDetail
    {
        [Index(0)]
        public string UserId { get; set; }

        [Index(1)]
        public string Email { get; set; }

        [Index(2)]
        public int ClusterId { get; set; }

        [Index(3)]
        public string AgeTag { get; set; }

        [Index(4)]
        public string FieldOfWork { get; set; }

        [Index(5)]
        public string ExperienceTag { get; set; }

        [Index(6)]
        public string TopicTags { get; set; } // Semicolon-delimited, always 12

        [Index(7)]
        public bool IsActive { get; set; } // true=active, false=unsubscribed

        /// <summary>
        /// Helper to return TopicTags as a List&lt;string&gt; (always 12 items, pad empty if less).
        /// </summary>
        public List<string> GetTopicTagList(int fixedCount = 12)
        {
            var tags = string.IsNullOrWhiteSpace(TopicTags)
                ? new string[0]
                : TopicTags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var result = new List<string>(tags);
            while (result.Count < fixedCount)
                result.Add("");
            if (result.Count > fixedCount)
                result = result.GetRange(0, fixedCount);
            return result;
        }
    }
}

// CSV: UserId,Email,ClusterId,AgeTag,FieldOfWork,ExperienceTag,TopicTags,IsActive
