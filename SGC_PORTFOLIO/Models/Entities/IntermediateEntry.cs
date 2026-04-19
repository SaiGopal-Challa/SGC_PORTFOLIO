using CsvHelper.Configuration.Attributes;

namespace SGC_PORTFOLIO.Models.Entities
{
    public class IntermediateEntry
    {
        [Index(0)]
        public string UserId { get; set; }

        [Index(1)]
        public string Email { get; set; }

        [Index(2)]
        public int ClusterId { get; set; }

        [Index(3)]
        public int Score { get; set; }
    }
}
