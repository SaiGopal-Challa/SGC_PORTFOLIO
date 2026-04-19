namespace SGC_PORTFOLIO.Models.Dtos
{
    public class UpdateUserDto
    {
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? DateOfBirthUtc { get; set; }
        public string? Role { get; set; }
        public bool? IsPilot { get; set; }
    }
}
