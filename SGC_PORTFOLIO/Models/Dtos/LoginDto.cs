namespace SGC_PORTFOLIO.Models.Dtos
{
    public class LoginDto
    {
        /// <summary>
        /// Either username or email
        /// </summary>
        public string UserNameOrEmail { get; set; }

        public string Password { get; set; }
    }
}
