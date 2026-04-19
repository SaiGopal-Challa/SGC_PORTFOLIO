using System.Security.Cryptography;
using System.Text;
using SGC_PORTFOLIO.Models.Entities;
using SGC_PORTFOLIO.Services.CsvHelpers;
using System.Net.Mail;
using System.Net;
using SGC_PORTFOLIO.Models.Dtos;

namespace SGC_PORTFOLIO.Services
{
    public class AuthenticationService
    {
        private readonly string _usersCsvPath;
        private readonly string _newsletterCsvPath;
        private readonly string _otpCsvPath;
        private readonly string _resetTokenCsvPath;
        private readonly IConfiguration _config;
        private readonly object _userFileLock = new object();
        public AuthenticationService(IConfiguration config)
        {
            _config = config;
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            _usersCsvPath = Path.Combine(basePath, "App_Data", "CSV", "Users.csv");
            _newsletterCsvPath = Path.Combine(basePath, "App_Data", "CSV", "UserNewsletter.csv");
            _otpCsvPath = Path.Combine(basePath, "App_Data", "CSV", "UserOtp.csv");
            _resetTokenCsvPath = Path.Combine(basePath, "App_Data", "CSV", "ResetTokens.csv");
            if (!File.Exists(_usersCsvPath))
                File.WriteAllText(_usersCsvPath, "UserId,UserName,PasswordHash,Name,Email,Phone,DateOfBirthUtc,Role,IsPilot,IsUnsubscribed\n");
            if (!File.Exists(_newsletterCsvPath))
                File.WriteAllText(_newsletterCsvPath, "UserId,Email,ClusterId,AgeTag,FieldOfWork,ExperienceTag,TopicTags,IsActive\n");
            if (!File.Exists(_otpCsvPath))
                File.WriteAllText(_otpCsvPath, "UserId,Otp,ExpiryUtc\n");
            if (!File.Exists(_resetTokenCsvPath))
                File.WriteAllText(_resetTokenCsvPath, "Email,Token,ExpiryUtc\n");
        }
        public void CreateUser(string userName, string plainPwd, string email, string phone, DateTime? dobUtc, string? name, string? role, bool isPilot)
        {
            lock (_userFileLock)
            {
                var allUsers = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath).ToList();
                if (allUsers.Any(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)))
                    throw new Exception("Username already exists.");
                if (allUsers.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                    throw new Exception("Email already registered.");
                var userId = Guid.NewGuid().ToString("N");
                var hashedPwd = HashPassword(plainPwd);
                var newUser = new UserDetail
                {
                    UserId = userId,
                    UserName = userName,
                    PasswordHash = hashedPwd,
                    Name = name ?? "",
                    Email = email,
                    Phone = phone ?? "",
                    DateOfBirthUtc = dobUtc,
                    Role = role,
                    IsPilot = isPilot,
                    IsUnsubscribed = false
                };
                allUsers.Add(newUser);
                CsvWriterUtility.WriteAll(_usersCsvPath, allUsers);
            }
        }
        public bool UserNameExists(string userName)
        {
            var allUsers = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath);
            return allUsers.Any(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        }
        public bool ValidateCredentials(string userNameOrEmail, string plainPwd, out UserDetail user, out string error)
        {
            user = null;
            error = null;
            var allUsers = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath);
            var hashToCheck = HashPassword(plainPwd);
            user = allUsers.FirstOrDefault(u =>
                !u.IsUnsubscribed &&
                ((u.UserName.Equals(userNameOrEmail, StringComparison.OrdinalIgnoreCase)) ||
                 (u.Email.Equals(userNameOrEmail, StringComparison.OrdinalIgnoreCase))) &&
                u.PasswordHash.Equals(hashToCheck, StringComparison.Ordinal));
            if (user != null)
                return true;
            error = "Invalid credentials or user unsubscribed.";
            return false;
        }
        public void SendOtpToEmail(string email)
        {
            var allUsers = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath);
            var user = allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (user == null) throw new Exception("User not found.");
            var otp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);
            var allOtps = CsvReaderUtility.ReadAll<UserOtp>(_otpCsvPath).ToList();
            allOtps.RemoveAll(o => o.UserId == user.UserId);
            allOtps.Add(new UserOtp { UserId = user.UserId, Otp = otp, ExpiryUtc = expiry });
            CsvWriterUtility.WriteAll(_otpCsvPath, allOtps);
            SendEmail(email, "Your OTP Code", $"Your OTP is: {otp}");
        }
        public bool VerifyOtp(string userId, string otp, out UserDetail user)
        {
            user = null;
            var allOtps = CsvReaderUtility.ReadAll<UserOtp>(_otpCsvPath).ToList();
            var entry = allOtps.FirstOrDefault(o => o.UserId == userId && o.Otp == otp && o.ExpiryUtc > DateTime.UtcNow);
            if (entry == null) return false;
            var allUsers = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath);
            user = allUsers.FirstOrDefault(u => u.UserId == userId);
            allOtps.RemoveAll(o => o.UserId == userId);
            CsvWriterUtility.WriteAll(_otpCsvPath, allOtps);
            return user != null;
        }
        public UserDetail GetUserById(string userId)
        {
            var allUsers = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath);
            return allUsers.FirstOrDefault(u => u.UserId == userId);
        }
        public bool UpdateUserDetails(UpdateUserDto dto, out UserDetail user, out string error)
        {
            user = null;
            error = null;
            lock (_userFileLock)
            {
                var allUsers = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath).ToList();
                user = allUsers.FirstOrDefault(u => u.UserId == dto.UserId);
                if (user == null) { error = "User not found."; return false; }
                if (!string.IsNullOrWhiteSpace(dto.UserName)) user.UserName = dto.UserName;
                if (!string.IsNullOrWhiteSpace(dto.Name)) user.Name = dto.Name;
                if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;
                if (!string.IsNullOrWhiteSpace(dto.Phone)) user.Phone = dto.Phone;
                if (dto.DateOfBirthUtc.HasValue) user.DateOfBirthUtc = dto.DateOfBirthUtc;
                if (!string.IsNullOrWhiteSpace(dto.Role)) user.Role = dto.Role;
                if (dto.IsPilot.HasValue) user.IsPilot = dto.IsPilot.Value;
                CsvWriterUtility.WriteAll(_usersCsvPath, allUsers);
                return true;
            }
        }
        public bool IsUserSubscribedToNewsletter(string userId)
        {
            var all = CsvReaderUtility.ReadAll<UserNewsletterDetail>(_newsletterCsvPath);
            return all.Any(e => e.UserId == userId && e.IsActive);
        }
        public void UpdateUserDetailsInNewsletter(UserDetail user)
        {
            var all = CsvReaderUtility.ReadAll<UserNewsletterDetail>(_newsletterCsvPath).ToList();
            var entry = all.FirstOrDefault(e => e.UserId == user.UserId);
            if (entry != null)
            {
                entry.Email = user.Email;
                CsvWriterUtility.WriteAll(_newsletterCsvPath, all);
            }
        }
        public void SendResetPasswordEmail(string email)
        {
            var allUsers = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath);
            var user = allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (user == null) throw new Exception("User not found.");
            var token = Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.AddMinutes(5);
            var allTokens = CsvReaderUtility.ReadAll<ResetToken>(_resetTokenCsvPath).ToList();
            allTokens.RemoveAll(t => t.Email == email);
            allTokens.Add(new ResetToken { Email = email, Token = token, ExpiryUtc = expiry });
            CsvWriterUtility.WriteAll(_resetTokenCsvPath, allTokens);
            SendEmail(email, "Password Reset", $"Your reset token is: {token}");
        }
        public bool ResetPasswordWithToken(string email, string token, string newPassword)
        {
            var allTokens = CsvReaderUtility.ReadAll<ResetToken>(_resetTokenCsvPath).ToList();
            var entry = allTokens.FirstOrDefault(t => t.Email == email && t.Token == token && t.ExpiryUtc > DateTime.UtcNow);
            if (entry == null) return false;
            var allUsers = CsvReaderUtility.ReadAll<UserDetail>(_usersCsvPath).ToList();
            var user = allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (user == null) return false;
            user.PasswordHash = HashPassword(newPassword);
            CsvWriterUtility.WriteAll(_usersCsvPath, allUsers);
            allTokens.RemoveAll(t => t.Email == email);
            CsvWriterUtility.WriteAll(_resetTokenCsvPath, allTokens);
            return true;
        }
        private void SendEmail(string to, string subject, string body)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"]);
            var smtpUser = _config["Smtp:User"];
            var smtpPass = _config["Smtp:Pass"];
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };
            var mail = new MailMessage(smtpUser, to, subject, body);
            client.Send(mail);
        }
        private static string HashPassword(string plain)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(plain);
            var hashBytes = sha.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
        public class UserOtp
        {
            public string UserId { get; set; }
            public string Otp { get; set; }
            public DateTime ExpiryUtc { get; set; }
        }
        public class ResetToken
        {
            public string Email { get; set; }
            public string Token { get; set; }
            public DateTime ExpiryUtc { get; set; }
        }
    }
}
