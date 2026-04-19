namespace SGC_PORTFOLIO.Models.Dtos
{
    public class ResetPasswordDto
    {
        /// <summary>
        /// The user’s email for lookup.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The token for password reset.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The new plain‐text password to set.
        /// </summary>
        public string NewPassword { get; set; }
    }
}
