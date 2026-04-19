using CsvHelper.Configuration.Attributes;

namespace SGC_PORTFOLIO.Models
{
    public class PdfClusterMappingDetail
    {
        [Index(0)]
        public string PdfId { get; set; }

        [Index(1)]
        public int ClusterId { get; set; }
    }
}
