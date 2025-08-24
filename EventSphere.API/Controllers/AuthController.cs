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
            try
            {
                var result = await _userService.RegisterAsync(dto, UserRole.User);
                return Ok(new { success = true, user = result });
            }
            catch (ArgumentException ex)
            {
                // Invalid name/email/password
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already registered"))
                    return Conflict(new { success = false, message = ex.Message });
                if (ex.Message.Contains("verification email has been resent"))
                    return Ok(new { success = false, message = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
 
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            try
            {
                var result = await _userService.LoginAsync(dto);
                string? token = result?.EmailVerificationToken;
                if (result != null)
                    result.EmailVerificationToken = null; // Remove token from user object
                return Ok(new { success = true, token, user = result });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("incorrect") || ex.Message.Contains("not found"))
                    return Unauthorized(new { success = false, message = "Email or password is incorrect." });
                if (ex.Message.Contains("not verified"))
                    return Unauthorized(new { success = false, message = "Email not verified. Please check your inbox for the verification link." });
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
 
 
 
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var success = await _userService.VerifyEmailAsync(token);
            if (success)
                return Ok(new { success = true });
            return BadRequest(new { success = false, message = "Expired or invalid token." });
        }
 
 
        public class ForgotPasswordDto
{
    public required string Email { get; set; }
}
 
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                var success = await _userService.RequestPasswordResetAsync(dto.Email);
                if (success)
                    return Ok(new { success = true, message = "Password reset email sent." });
                return NotFound(new { success = false, message = "Invalid email or user not verified." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
 
 
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { success = false, message = "Token and new password are required." });
            try
            {
                var success = await _userService.ResetPasswordAsync(dto.Token, dto.NewPassword);
                if (success)
                    return Ok(new { success = true, message = "Password reset successful." });
                return NotFound(new { success = false, message = "Invalid or expired token." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}