using Microsoft.AspNetCore.Mvc;
using backend.Dtos;
using backend.Services;
using EventSphere.Domain.Enums;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }
 
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var result = await _userService.RegisterAsync(dto, UserRole.User);
            return Ok(result);
        }
 
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            if (result == null)
            {
                return BadRequest(new { success = false, message = "Login failed. User not found or invalid credentials." });
            }
            var token = result.EmailVerificationToken;
            result.EmailVerificationToken = null; // Remove token from user object
            return Ok(new { success = true, token, user = result });
        }
 
 
 
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var success = await _userService.VerifyEmailAsync(token);
            if (success)
            {
                // Redirect to frontend verify-email page with success message
                return Ok(new { success = true });
            }
            else
            {
                // Redirect to frontend verify-email page with error message
                return BadRequest(new { success = false, message = "Expired or invalid token." });
 
            }
        }
 
 
        public class ForgotPasswordDto
{
    public required string Email { get; set; }
}
 
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
{
    var success = await _userService.RequestPasswordResetAsync(dto.Email);
    if (success)
        return Ok(new { success = true, message = "Password reset email sent." });
    return BadRequest(new { success = false, message = "Invalid email or user not verified." });
}
 
 
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { success = false, message = "Token and new password are required." });
            var success = await _userService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (success)
                return Ok(new { success = true, message = "Password reset successful." });
            return BadRequest(new { success = false, message = "Invalid or expired token." });
        }
    }
}