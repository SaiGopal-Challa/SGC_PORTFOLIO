using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SGC_PORTFOLIO.Models.Entities;
using SGC_PORTFOLIO.Services.CsvHelpers;
using System.IO;

namespace SGC_PORTFOLIO.Services
{
    public class EmailDispatchService : IDisposable
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _pdfMetadataCsvPath;
        private readonly string _sentLogPath;
        private readonly string _processedPdfFolder;

        public EmailDispatchService(IConfiguration configuration)
        {
            _smtpHost = configuration["Smtp:Host"];
            _smtpPort = int.Parse(configuration["Smtp:Port"] ?? "587");
            _smtpUser = configuration["Smtp:User"];
            _smtpPass = configuration["Smtp:Pass"];

            var basePath = GetProjectRoot();
            _pdfMetadataCsvPath = Path.Combine(basePath, "App_Data", "CSV", "PdfMetadata.csv");
            _sentLogPath = Path.Combine(basePath, "App_Data", "CSV", "SentLog.txt");
            _processedPdfFolder = Path.Combine(basePath, "App_Data", "Newsletters", "Processed");

            if (!File.Exists(_sentLogPath))
                File.WriteAllText(_sentLogPath, "");
        }

        private string GetProjectRoot()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = new DirectoryInfo(baseDir);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SGC_PORTFOLIO.csproj")))
            {
                dir = dir.Parent;
            }
            return dir?.FullName ?? baseDir;
        }

        public async Task SendEmailsAsync(string intermediateCsvPath, string pdfId)
        {
            // 1) Load PDF metadata
            var allMeta = CsvReaderUtility.ReadAll<PdfMetadataDetail>(_pdfMetadataCsvPath);
            var pdfEntry = allMeta.FirstOrDefault(p => p.PdfId == pdfId);
            if (pdfEntry == null) return;

            var pdfFileName = pdfEntry.FileName;
            var summary = pdfEntry.Summary;

            // 2) Prepare a fresh SmtpClient per call
            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(_smtpUser, _smtpPass);

            // 3) For each line in intermediate CSV
            var lines = File.ReadAllLines(intermediateCsvPath).Skip(1);
            foreach (var line in lines)
            {
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) continue;

                var userId = parts[0];
                var email = parts[1];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Newsletter Service", _smtpUser));
                message.To.Add(new MailboxAddress(userId, email));
                message.Subject = $"New Newsletter: {pdfFileName}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <h2>New Newsletter: {pdfFileName}</h2>
                    <p>{summary}</p>
                    <p>We've attached the newsletter PDF to this email.</p>
                    <p>If the attachment isn't working or doesn't display properly, you can <a href=""https://samudbhav.tech/newsletters/{pdfId}.pdf"">download it here</a>.</p>
                    <p>Thanks for subscribing!</p>
                    "
                };

                // Build full path to the PDF file
                var pdfPath = Path.Combine(_processedPdfFolder, pdfFileName);

                if (File.Exists(pdfPath))
                {
                    bodyBuilder.Attachments.Add(pdfPath);
                }
                else
                {
                    Console.WriteLine($"[EmailDispatchService] PDF not found: {pdfPath}");
                    //continue; // Skip sending if the attachment is missing
                }

                message.Body = bodyBuilder.ToMessageBody();

                try
                {
                    await smtpClient.SendAsync(message);

                    // 4) Log to SentLog.txt
                    var logLine = $"{userId}, {email}, {pdfFileName}, {pdfId}, {DateTime.Now:O}";
                    File.AppendAllText(_sentLogPath, logLine + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EmailDispatchService] Failed sending to {email}: {ex}");
                }
            }

            await smtpClient.DisconnectAsync(true);
        }

        public void Dispose()
        {
            // Nothing else to dispose
        }
    }
}
