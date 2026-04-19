using SGC_PORTFOLIO.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SGC_PORTFOLIO.Background
{
    public class NewsletterHostedService : IHostedService, IDisposable
    {
        private Timer _nightlyClusterTimer;
        private Timer _pdfScanTimer;
        private readonly IServiceProvider _services;
        private bool _clusteredToday;
        private readonly SemaphoreSlim _scanLock = new SemaphoreSlim(1, 1);
        private readonly string _projectRoot;

        public NewsletterHostedService(IServiceProvider services)
        {
            _services = services;
            _clusteredToday = false;
            _projectRoot = GetProjectRoot();
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("[NewsletterHostedService] Starting background service...");
            // Run clustering once on startup if not yet run today
            Task.Run(async () =>
            {
                Console.WriteLine("[NewsletterHostedService] Running clustering on startup...");
                await RunClusteringIfNeeded();
            }, cancellationToken);

            // Immediately process any incoming PDFs once on startup
            Task.Run(async () =>
            {
                Console.WriteLine("[NewsletterHostedService] Processing PDFs on startup...");
                await ProcessNewPdfsBatchAsync();
            }, cancellationToken);

            // Schedule nightly clustering at 01:00 UTC
            var next1amUtc = GetNextOccurrenceUtc(1, 0, 0);
            var timeTo1am = next1amUtc - DateTime.UtcNow;
            if (timeTo1am < TimeSpan.Zero)
                timeTo1am = TimeSpan.Zero;

            _nightlyClusterTimer = new Timer(
                async _ => {
                    Console.WriteLine("[NewsletterHostedService] Nightly clustering timer fired.");
                    await RunClusteringCallback();
                },
                null,
                dueTime: timeTo1am,
                //period: TimeSpan.FromHours(24));
                period: TimeSpan.FromDays(30));

            // Schedule PDF scanning every 5 minutes
            _pdfScanTimer = new Timer(
                async _ => {
                    Console.WriteLine("[NewsletterHostedService] PDF scan timer fired.");
                    await ProcessNewPdfsBatchCallback();
                },
                null,
                //dueTime: TimeSpan.FromMinutes(5),
                dueTime: TimeSpan.FromDays(5),
                //period: TimeSpan.FromMinutes(5));
                period: TimeSpan.FromDays(5));

            return Task.CompletedTask;
        }

        private async Task RunClusteringIfNeeded()
        {
            if (_clusteredToday) return;
            Console.WriteLine("[NewsletterHostedService] Running clustering (once per day)...");
            using var scope = _services.CreateScope();
            var clusterService = scope.ServiceProvider.GetRequiredService<ClusterService>();
            await clusterService.RunClusteringAsync();
            _clusteredToday = true;
            Console.WriteLine("[NewsletterHostedService] Clustering complete.");
        }

        private async Task RunClusteringCallback()
        {
            Console.WriteLine("[NewsletterHostedService] Running nightly clustering callback...");
            using var scope = _services.CreateScope();
            var clusterService = scope.ServiceProvider.GetRequiredService<ClusterService>();
            await clusterService.RunClusteringAsync();
            Console.WriteLine("[NewsletterHostedService] Nightly clustering complete.");
        }

        private async Task ProcessNewPdfsBatchCallback()
        {
            if (!await _scanLock.WaitAsync(0)) return;
            try
            {
                Console.WriteLine("[NewsletterHostedService] Entered PDF scan lock, processing batch...");
                await ProcessNewPdfsBatchAsync();
            }
            finally
            {
                _scanLock.Release();
            }
        }

        private async Task ProcessNewPdfsBatchAsync()
        {
            Console.WriteLine("[NewsletterHostedService] Starting PDF batch processing...");
            using var scope = _services.CreateScope();
            var tagService = scope.ServiceProvider.GetRequiredService<TagSummaryService>();
            var mappingService = scope.ServiceProvider.GetRequiredService<PdfClusterMappingService>();
            var userSelectionService = scope.ServiceProvider.GetRequiredService<UserSelectionService>();
            var emailDispatchService = scope.ServiceProvider.GetRequiredService<EmailDispatchService>();

            var incomingFolder = Path.Combine(_projectRoot, "App_Data", "Newsletters", "Incoming");
            Directory.CreateDirectory(incomingFolder);

            var pdfFiles = Directory.GetFiles(incomingFolder, "*.pdf").ToList();
            Console.WriteLine($"[NewsletterHostedService] Found {pdfFiles.Count} PDF(s) in Incoming folder.");
            if (!pdfFiles.Any()) return;

            var processedPdfIds = new List<string>();
            foreach (var pdfPath in pdfFiles)
            {
                try
                {
                    Console.WriteLine($"[NewsletterHostedService] Processing PDF: {pdfPath}");
                    var (pdfId, ageTags, expTags, topicTags) = await tagService.ProcessPdfAsync(pdfPath);
                    Console.WriteLine($"[NewsletterHostedService] PDF processed. pdfId={pdfId}, ageTags={string.Join(';', ageTags)}, expTags={string.Join(';', expTags)}, topicTags={string.Join(';', topicTags)}");
                    mappingService.MapPdfToClusters(pdfId, ageTags, expTags, topicTags);
                    processedPdfIds.Add(pdfId);
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine($"[NewsletterHostedService] I/O exception for {pdfPath}: {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NewsletterHostedService] Error processing {pdfPath}: {ex}");
                }
            }

            // Only generate intermediate CSVs for the current batch
            Console.WriteLine("[NewsletterHostedService] Generating intermediate CSVs for current batch only...");
            userSelectionService.GenerateIntermediateCsvsForPdfIds(processedPdfIds);
            Console.WriteLine("[NewsletterHostedService] Intermediate CSV generation complete.");

            // Then send emails for each processed pdf
            foreach (var pdfId in processedPdfIds)
            {
                var intermediatePath = Path.Combine(_projectRoot, "App_Data", "CSV", "Intermediate", $"{pdfId}_Intermediate.csv");

                if (File.Exists(intermediatePath))
                {
                    try
                    {
                        Console.WriteLine($"[NewsletterHostedService] Sending emails for PDF: {pdfId}");
                        await emailDispatchService.SendEmailsAsync(intermediatePath, pdfId);
                        File.Delete(intermediatePath);
                        Console.WriteLine($"[NewsletterHostedService] Emails sent and intermediate CSV deleted for PDF: {pdfId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NewsletterHostedService] Error sending emails for {pdfId}: {ex}");
                    }
                }
                else
                {
                    Console.WriteLine($"[NewsletterHostedService] Intermediate CSV not found for PDF: {pdfId}");
                }
            }
            Console.WriteLine("[NewsletterHostedService] PDF batch processing complete.");
        }

        private static DateTime GetNextOccurrenceUtc(int hour, int minute, int second)
        {
            var now = DateTime.UtcNow;
            var target = new DateTime(now.Year, now.Month, now.Day, hour, minute, second, DateTimeKind.Utc);
            return now < target ? target : target.AddDays(1);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("[NewsletterHostedService] Stopping background service...");
            _nightlyClusterTimer?.Change(Timeout.Infinite, 0);
            _pdfScanTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _nightlyClusterTimer?.Dispose();
            _pdfScanTimer?.Dispose();
        }
    }
}
