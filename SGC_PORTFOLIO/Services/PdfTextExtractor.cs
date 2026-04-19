using System.Text;
using UglyToad.PdfPig;

namespace SGC_PORTFOLIO.Services
{
    public static class PdfTextExtractor
    {
        public static string Extract(string pdfFilePath)
        {
            var sb = new StringBuilder();
            using var document = PdfDocument.Open(pdfFilePath);
            foreach (var page in document.GetPages())
                sb.AppendLine(page.Text);
            return sb.ToString();
        }
    }
}
