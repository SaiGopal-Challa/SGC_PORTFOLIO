using System;

namespace SGC_PORTFOLIO.Models.Entities
{
    public class UserDetail
    {
        /// <summary>
        /// Primary user identifier (GUID or string).
        /// </summary>
        public string UserId { get; set; } // Auto-generated GUID

        /// <summary>
        /// Unique username chosen by the user.
        /// </summary>
        public string UserName { get; set; } // Unique

        /// <summary>
        /// Hashed password (never store plain text).
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string Name { get; set; } // Full name

        /// <summary>
        /// The user’s email address (must be unique and validated).
        /// </summary>
        public string Email { get; set; } // Unique, validated

        /// <summary>
        /// Optional phone number (e.g. "+1-555-123-4567").
        /// </summary>
        public string? Phone { get; set; } // Optional, with country code

        /// <summary>
        /// Optional date of birth (UTC).
        /// </summary>
        public DateTime? DateOfBirthUtc { get; set; } // Optional

        /// <summary>
        /// Optional role of the user (e.g. "admin", "user").
        /// </summary>
        public string? Role { get; set; } // Optional, set by FE

        /// <summary>
        /// Indicates if the user is a pilot (true = is pilot).
        /// </summary>
        public bool IsPilot { get; set; } // Optional, default false

        /// <summary>
        /// Whether the user has unsubscribed (true = unsubscribed).
        /// </summary>
        public bool IsUnsubscribed { get; set; } // For newsletter opt-out
    }
}

// Dummy data for Users.csv
// UserId,UserName,PasswordHash,Name,Email,Phone,DateOfBirthUtc,Role,IsPilot,IsUnsubscribed
// 1234567890abcdef,alice,hash1,Alice Wonderland,alice@example.com,1234567890,1990-01-01T00:00:00Z,admin,false,false
// 2345678901abcdef,bob,hash2,Bob Builder,bob@example.com,2345678901,1992-02-02T00:00:00Z,user,true,false
// 3456789012abcdef,charlie,hash3,Charlie Chocolate,charlie@example.com,3456789012,1994-03-03T00:00:00Z,user,false,true
