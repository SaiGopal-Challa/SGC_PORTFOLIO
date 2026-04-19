using CsvHelper.Configuration.Attributes;

namespace SGC_PORTFOLIO.Models.Entities
{
    public class ClusterInfoDetail
    {
        [Index(0)]
        public int ClusterId { get; set; }

        [Index(1)]
        public DateTime ClusterCreatedAtUtc { get; set; }

        /// <summary>
        /// Semicolon‐delimited “representative tags” for this cluster.
        /// </summary>
        [Index(2)]
        public string RepresentativeTags { get; set; }

        [Index(3)]
        public string AgeTag { get; set; } // Most common age tag in cluster
        [Index(4)]
        public string FieldOfWork { get; set; } // Most common field of work in cluster
        [Index(5)]
        public string ExperienceTag { get; set; } // Most common experience tag in cluster

        public List<string> GetRepresentativeTagList()
        {
            if (string.IsNullOrWhiteSpace(RepresentativeTags))
                return new List<string>();
            return new List<string>(RepresentativeTags.Split(';', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
