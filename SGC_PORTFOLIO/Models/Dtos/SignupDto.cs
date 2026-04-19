using System.ComponentModel.DataAnnotations;

namespace SGC_PORTFOLIO.Models.Dtos
{
    public class SignupDto
    {
        /// The user’s desired username (must be unique).
        [Required]
        public string UserName { get; set; }

        /// Plain‐text password (will be hashed by your auth logic).
        [Required]
        public string Password { get; set; }

        /// User’s email address (must be unique).
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// Optional phone number (e.g. "+1-555-123-4567").
        public string? Phone { get; set; }

        /// Optional date of birth (UTC). 
        /// Use this to generate an AgeTag later if needed.
        public DateTime? DateOfBirthUtc { get; set; }

        /// Full name
        public string? Name { get; set; } 

        /// Optional, set by FE
        public string? Role { get; set; } 

        /// Optional, default false
        public bool? IsPilot { get; set; } 
    }
}
