using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SGC_PORTFOLIO.Models.Dtos;
using SGC_PORTFOLIO.Models.Entities;
using SGC_PORTFOLIO.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SGC_PORTFOLIO.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly IConfiguration _config;
        private readonly string _blacklistPath;

        public AccountController(AuthenticationService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            _blacklistPath = Path.Combine(basePath, "App_Data", "CSV", "BlackList.csv");
            if (!System.IO.File.Exists(_blacklistPath))
                System.IO.File.WriteAllText(_blacklistPath, "Token\n");
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                _authService.CreateUser(dto.UserName, dto.Password, dto.Email, dto.Phone, dto.DateOfBirthUtc, dto.Name, dto.Role, dto.IsPilot ?? false);
                return Ok(new { message = "User created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("check-username")]
        public IActionResult CheckUsername([FromQuery] string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return BadRequest("Username required.");
            var exists = _authService.UserNameExists(userName);
            return Ok(new { isUnique = !exists });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserNameOrEmail) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username/email and password are required.");
            if (_authService.ValidateCredentials(dto.UserNameOrEmail, dto.Password, out var user, out var error))
            {
                if (user.IsPilot)
                {
                    _authService.SendOtpToEmail(user.Email);
                    return Ok(new { requiresOtp = true, userId = user.UserId });
                }
                var token = GenerateJwtToken(user);
                var refreshToken = Guid.NewGuid().ToString("N");
                return Ok(new { token, refreshToken, userId = user.UserId });
            }
            else
            {
                return Unauthorized(error ?? "Invalid credentials.");
            }
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] OtpVerifyDto dto)
        {
            if (_authService.VerifyOtp(dto.UserId, dto.Otp, out var user))
            {
                var token = GenerateJwtToken(user);
                var refreshToken = Guid.NewGuid().ToString("N");
                return Ok(new { token, refreshToken, userId = user.UserId });
            }
            return Unauthorized("Invalid OTP.");
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var user = _authService.GetUserById(dto.UserId);
            if (user == null)
                return Unauthorized();
            var token = GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid().ToString("N");
            return Ok(new { token, refreshToken });
        }

        [HttpPost("update-details")]
        public IActionResult UpdateUserDetails([FromBody] UpdateUserDto dto)
        {
            var updated = _authService.UpdateUserDetails(dto, out var user, out var error);
            if (!updated)
                return BadRequest(error);
            if (_authService.IsUserSubscribedToNewsletter(user.UserId))
            {
                _authService.UpdateUserDetailsInNewsletter(user);
            }
            return Ok(new { message = "User details updated." });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest("Token required.");
            System.IO.File.AppendAllText(_blacklistPath, dto.Token + "\n");
            return Ok(new { message = "Logged out." });
        }

        [HttpPost("reset-password-request")]
        public IActionResult ResetPasswordRequest([FromBody] ResetPasswordRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email required.");
            _authService.SendResetPasswordEmail(dto.Email);
            return Ok(new { message = "Reset link sent to email (valid for 5 min)." });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (_authService.ResetPasswordWithToken(dto.Email, dto.Token, dto.NewPassword))
                return Ok(new { message = "Password reset." });
            return BadRequest("Invalid or expired token.");
        }

        private string GenerateJwtToken(UserDetail user)
        {
            var jwtKey = _config["Jwt:Key"];
            var jwtIssuer = _config["Jwt:Issuer"];
            var jwtAudience = _config["Jwt:Audience"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.Role ?? ""),
            };
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
