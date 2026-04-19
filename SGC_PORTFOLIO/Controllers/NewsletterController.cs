using Microsoft.AspNetCore.Mvc;
using SGC_PORTFOLIO.Models.Dtos;
using SGC_PORTFOLIO.Models.Entities;
using SGC_PORTFOLIO.Services.CsvHelpers;
using SGC_PORTFOLIO.Services;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGC_PORTFOLIO.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsletterController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly string _usersCsvPath;
        private readonly string _userNewsletterCsvPath;
        private readonly string _pdfMetadataCsvPath;
        private readonly object _newsletterFileLock = new object();

        public NewsletterController(AuthenticationService authService)
        {
            _authService = authService;
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            _usersCsvPath = Path.Combine(basePath, "App_Data", "CSV", "Users.csv");
            _userNewsletterCsvPath = Path.Combine(basePath, "App_Data", "CSV", "UserNewsletter.csv");
            _pdfMetadataCsvPath = Path.Combine(basePath, "App_Data", "CSV", "PdfMetadata.csv");
        }

        /// <summary>
        /// POST api/Newsletter/register
        /// Registers an existing user for the newsletter (or updates their details).
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register([FromBody] NewsletterRegistrationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("UserId and Email are required.");

            // 1) Verify user exists in Users.csv and email matches
            var users = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath);
            var user = users.FirstOrDefault(u => (u.UserId == dto.UserId || u.Email.Equals(dto.Email, System.StringComparison.OrdinalIgnoreCase))
                                                  && !u.IsUnsubscribed);
            if (user == null)
                return BadRequest("User not found or unsubscribed.");

            if (!user.Email.Equals(dto.Email, System.StringComparison.OrdinalIgnoreCase))
                return BadRequest("Provided email does not match the registered user’s email.");

            // 2) Upsert this user into UserNewsletter.csv
            lock (_newsletterFileLock)
            {
                // Ensure the CSV exists
                if (!System.IO.File.Exists(_userNewsletterCsvPath))
                {
                    var header = "UserId,Email,ClusterId,AgeTag,FieldOfWork,ExperienceTag,TopicTags,IsActive";
                    System.IO.File.WriteAllText(_userNewsletterCsvPath, header + System.Environment.NewLine);
                }

                var allEntries = CsvReaderUtility.ReadAll<UserNewsletterDetail>(_userNewsletterCsvPath).ToList();
                var existing = allEntries.FirstOrDefault(e => e.UserId == dto.UserId);

                // Build the semicolon‐delimited TopicTags string
                var topicTagsJoined = dto.TopicTags == null || dto.TopicTags.Count == 0
                    ? ""
                    : string.Join(';', dto.TopicTags);

                if (existing != null)
                {
                    // Update existing
                    existing.Email = dto.Email;
                    existing.AgeTag = dto.AgeTag ?? "";
                    existing.FieldOfWork = dto.FieldOfWork ?? "";
                    existing.ExperienceTag = dto.ExperienceTag ?? "";
                    existing.TopicTags = topicTagsJoined;
                    existing.IsActive = true;
                }
                else
                {
                    // Append new user; default ClusterId = 0 (will be set next nightly clustering)
                    allEntries.Add(new UserNewsletterDetail
                    {
                        UserId = dto.UserId,
                        Email = dto.Email,
                        ClusterId = 0,
                        AgeTag = dto.AgeTag ?? "",
                        FieldOfWork = dto.FieldOfWork ?? "",
                        ExperienceTag = dto.ExperienceTag ?? "",
                        TopicTags = topicTagsJoined,
                        IsActive = true
                    });
                }

                CsvWriterUtility.WriteAll(_userNewsletterCsvPath, allEntries);
            }

            return Ok(new { message = "Registered for newsletter successfully." });
        }

        /// <summary>
        /// PUT api/Newsletter/update-details
        /// Updates any of the user details in the newsletter subscription.
        /// </summary>
        [HttpPut("update-details")]
        public IActionResult UpdateUserDetailsInNewsletter([FromBody] NewsletterRegistrationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("UserId and Email are required.");

            lock (_newsletterFileLock)
            {
                if (!System.IO.File.Exists(_userNewsletterCsvPath))
                    return BadRequest("No newsletter registrations exist.");

                var allEntries = CsvReaderUtility.ReadAll<UserNewsletterDetail>(_userNewsletterCsvPath).ToList();
                var existing = allEntries.FirstOrDefault(e => e.UserId == dto.UserId);

                if (existing == null)
                    return BadRequest("User is not registered for the newsletter.");

                existing.Email = dto.Email;
                existing.AgeTag = dto.AgeTag ?? "";
                existing.FieldOfWork = dto.FieldOfWork ?? "";
                existing.ExperienceTag = dto.ExperienceTag ?? "";
                existing.TopicTags = dto.TopicTags == null || dto.TopicTags.Count == 0 ? "" : string.Join(';', dto.TopicTags);
                existing.IsActive = true;
                CsvWriterUtility.WriteAll(_userNewsletterCsvPath, allEntries);
            }

            return Ok(new { message = "Newsletter user details updated successfully." });
        }

        /// <summary>
        /// POST api/Newsletter/unsubscribe
        /// Sets IsActive to false in UserNewsletter.csv.
        /// </summary>
        [HttpPost("unsubscribe")]
        public IActionResult Unsubscribe([FromBody] UnsubscribeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserIdOrEmail))
                return BadRequest("UserIdOrEmail is required.");

            // 1) Mark user as unsubscribed in Users.csv
            var users = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath).ToList();
            var user = users.FirstOrDefault(u =>
                u.UserId.Equals(dto.UserIdOrEmail, System.StringComparison.OrdinalIgnoreCase) ||
                u.Email.Equals(dto.UserIdOrEmail, System.StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return BadRequest("User not found.");

            user.IsUnsubscribed = true;
            CsvWriterUtility.WriteAll(_usersCsvPath, users);

            // 2) Set IsActive = false in UserNewsletter.csv
            lock (_newsletterFileLock)
            {
                if (System.IO.File.Exists(_userNewsletterCsvPath))
                {
                    var allEntries = CsvReaderUtility.ReadAll<UserNewsletterDetail>(_userNewsletterCsvPath).ToList();
                    var entry = allEntries.FirstOrDefault(e => e.UserId.Equals(user.UserId, System.StringComparison.OrdinalIgnoreCase));
                    if (entry != null)
                    {
                        entry.IsActive = false;
                        CsvWriterUtility.WriteAll(_userNewsletterCsvPath, allEntries);
                    }
                }
            }

            return Ok(new { message = "Successfully unsubscribed from newsletter." });
        }

        /// <summary>
        /// GET api/Newsletter/available-pdfs
        /// Returns a list of all processed PDFs and their GenAI‐generated metadata.
        /// </summary>
        [HttpGet("available-pdfs")]
        public IActionResult GetAvailablePdfs()
        {
            if (!System.IO.File.Exists(_pdfMetadataCsvPath))
                return Ok(System.Array.Empty<AvailablePdfDto>());

            var allMeta = CsvReaderUtility.ReadAll<PdfMetadataDetail>(_pdfMetadataCsvPath);
            var dtos = allMeta.Select(p => new AvailablePdfDto
            {
                PdfId = p.PdfId,
                FileName = p.FileName,
                Summary = p.Summary,
                AgeTags = p.GetAgeTagList(),
                ExpTags = p.GetExpTagList(),
                TopicTags = p.GetTopicTagList()
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// POST api/Newsletter/upload-pdf
        /// Upload a PDF to the Incoming folder for newsletter processing.
        /// </summary>
        [HttpPost("upload-pdf")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPdf()
        {
            var pdf = Request.Form?.Files?.FirstOrDefault();
            if (pdf == null || pdf.Length == 0)
                return BadRequest("No PDF file uploaded.");

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var incomingFolder = Path.Combine(basePath, "App_Data", "Newsletters", "Incoming");
            System.IO.Directory.CreateDirectory(incomingFolder);

            var fileName = System.IO.Path.GetFileName(pdf.FileName);
            var filePath = System.IO.Path.Combine(incomingFolder, fileName);

            await using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await pdf.CopyToAsync(stream);
            }

            return Ok(new { message = $"PDF '{fileName}' uploaded successfully and will be processed shortly." });
        }
        #region old method - working, but changed to fix swagger 
        /*[HttpPost("upload-pdf")]
        public IActionResult UploadPdf([FromForm] IFormFile pdf)
        {
            if (pdf == null || pdf.Length == 0)
                return BadRequest("No PDF file uploaded.");

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var incomingFolder = Path.Combine(basePath, "App_Data", "Newsletters", "Incoming");
            System.IO.Directory.CreateDirectory(incomingFolder);

            var fileName = System.IO.Path.GetFileName(pdf.FileName);
            var filePath = System.IO.Path.Combine(incomingFolder, fileName);

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                pdf.CopyTo(stream);
            }

            return Ok(new { message = $"PDF '{fileName}' uploaded successfully and will be processed shortly." });
        }*/
        #endregion
    }
}

