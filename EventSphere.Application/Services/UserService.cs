// ...existing code...
using EventSphere.Domain.Enums;
using EventSphere.Domain.Entities;
using backend.Dtos;
using backend.Data;
using backend.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using backend.Config;

namespace backend.Services
{
    public class UserService : IUserService
    {
    private readonly EventSphere.Application.Repositories.IAuthRepository _authRepository;
    private readonly IConfiguration _config;
    private readonly IFileService _fileService;
    private readonly ILogger<UserService> _logger;
        public UserService(EventSphere.Application.Repositories.IAuthRepository authRepository, IConfiguration config, IFileService fileService, ILogger<UserService> logger)
        {
            _authRepository = authRepository;
            _config = config;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<bool> UpdateUserDetailsWithImageAsync(int id, string? name, string? email, string? phoneNumber, string? profileImagePath)
        {
            var user = await _authRepository.GetUserByIdAsync(id);
            if (user == null) return false;
            if (!string.IsNullOrWhiteSpace(name)) user.Name = name;
            if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
            if (!string.IsNullOrWhiteSpace(phoneNumber)) user.Phone = phoneNumber;
            if (!string.IsNullOrWhiteSpace(profileImagePath)) user.ProfileImage = profileImagePath;
            await _authRepository.UpdateUserAsync(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _authRepository.DeleteUserAsync(id);
        }

        public async Task<EventSphere.Application.Dtos.Auth.UserDetailsDto?> GetUserDetailsByIdAsync(int id)
        {
            var user = await _authRepository.GetUserByIdAsync(id);
            if (user == null) return null;
            return new EventSphere.Application.Dtos.Auth.UserDetailsDto
            {
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.Phone,
                ProfileImage = user.ProfileImage
            };
        }
 
        public async Task<bool> VerifyEmailAsync(string token)
        {
            // Use repository for user lookup (add method if needed)
            var user = await _authRepository.GetUserByVerificationTokenAsync(token);
            if (user == null || user.IsEmailVerified)
                return false;
            if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                return false;
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            await _authRepository.UpdateUserAsync(user);
            return true;
        }
 
        public async Task<User> RegisterAsync(UserRegisterDto dto, UserRole role = UserRole.User)
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 3)
            {
                try
                {
                    throw new ArgumentException("Invalid name. Must be at least 3 characters.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invalid name. Must be at least 3 characters.");
                    throw;
                }
            }
            // Validate email
            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
            {
                try
                {
                    throw new ArgumentException("Invalid email address.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invalid email address.");
                    throw;
                }
            }
            // Check if user already exists
            var existingUser = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                if (existingUser.IsEmailVerified)
                {
                    try
                    {
                        throw new Exception("Email already registered. Please Login.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Email already registered. Please Login.");
                        throw;
                    }
                }
                // Resend verification email for unverified user
                existingUser.EmailVerificationToken = Guid.NewGuid().ToString();
                existingUser.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
                await _authRepository.UpdateUserAsync(existingUser);
                SendVerificationEmail(existingUser.Email, existingUser.Name, existingUser.EmailVerificationToken);
                try
                {
                    throw new Exception("A verification email has been resent. Please check your inbox.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "A verification email has been resent. Please check your inbox.");
                    throw;
                }
            }

            ValidatePassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password), // Store hashed password
                Role = role,
                IsEmailVerified = false,
                EmailVerificationToken = Guid.NewGuid().ToString(),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            await _authRepository.AddUserAsync(user);

            SendVerificationEmail(user.Email, user.Name, user.EmailVerificationToken);

            // Registration successful
            return user;
        }
 
        public async Task<User?> LoginAsync(UserLoginDto dto)
        {
            var user = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            {
                try
                {
                    throw new Exception("Email or Password is incorrect.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email or Password is incorrect.");
                    throw;
                }
            }
            if (!user.IsEmailVerified)
            {
                try
                {
                    throw new Exception("Email not verified. Please check your inbox for the verification link.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email not verified. Please check your inbox for the verification link.");
                    throw;
                }
            }
 
            // Generate JWT token
            var jwtKey = _config["Jwt:Key"] ?? "supersecretkey";
            var jwtHelper = new backend.Helpers.JwtHelper(jwtKey);
            var token = jwtHelper.GenerateToken(user);
 
            // Return token and user
            return new User
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                ProfileImage = user.ProfileImage,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                EmailVerificationToken = token // Use this property to pass token for now
            };
        }
 
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _authRepository.GetUserByIdAsync(id);
        }
 
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _authRepository.GetAllUsersAsync();
        }
 
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
 
        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
 
        // Demo: Preferences and notification settings are stored as JSON in ProfileImage and Phone fields, respectively.
        // In production, use dedicated fields or related tables.
 
        public async Task<object> GetPreferencesAsync(int userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                try
                {
                    throw new Exception("User not found");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "User not found for UserId: {UserId}", userId);
                    throw;
                }
            }
            if (string.IsNullOrEmpty(user.ProfileImage)) return new { };
            return JsonSerializer.Deserialize<object>(user.ProfileImage)!;
        }
 
        public async Task<bool> SetPreferencesAsync(int userId, object preferences)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                try
                {
                    throw new Exception("User not found");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "User not found for UserId: {UserId}", userId);
                    throw;
                }
            }
            user.ProfileImage = JsonSerializer.Serialize(preferences);
            await _authRepository.UpdateUserAsync(user);
            return true;
        }
 
        public async Task<object> GetNotificationSettingsAsync(int userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                try
                {
                    throw new Exception("User not found");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "User not found for UserId: {UserId}", userId);
                    throw;
                }
            }
            if (string.IsNullOrEmpty(user.Phone)) return new { };
            return JsonSerializer.Deserialize<object>(user.Phone)!;
        }
 
        public async Task<bool> UpdateNotificationSettingsAsync(int userId, object settings)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                try
                {
                    throw new Exception("User not found");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "User not found for UserId: {UserId}", userId);
                    throw;
                }
            }
            user.Phone = JsonSerializer.Serialize(settings);
            await _authRepository.UpdateUserAsync(user);
            return true;
        }
 
        // Forgot Password: Request password reset
        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);
            if (user == null || !user.IsEmailVerified)
                return false;
            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hour expiry
            await _authRepository.UpdateUserAsync(user);
            var frontendUrl = _config["Frontend:Url"] ?? throw new InvalidOperationException("Frontend URL not configured.");
            var resetUrl = $"{frontendUrl}/reset-password?token={user.PasswordResetToken}";
            var subject = "Reset your password";
            var body = $@"
            <div style='font-family:Arial,sans-serif;'>
            <h2 style='color:#0070f3;'>EventSphere Password Reset</h2>
            <p>Hi {user.Name},</p>
            <p>We received a request to reset your password. Click the button below to set a new password:</p>
            <p style='margin:20px 0;'>
                <a href='{resetUrl}' style='background:#0070f3;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px;'>Reset Password</a>
            </p>
            <p>If you did not request this, you can ignore this email.</p>
            <br>
            <p style='font-size:12px;color:#888;'>Best regards,<br>EventSphere Team</p>
            </div>
            ";
            var emailSender = new SmtpEmailSender(_config);
            await emailSender.SendEmailAsync(user.Email, subject, body);
            return true;
        }
 
        // Forgot Password: Reset password
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _authRepository.GetUserByPasswordResetTokenAsync(token);
            if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
                return false;
            ValidatePassword(newPassword);
            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _authRepository.UpdateUserAsync(user);
            return true;
        }
        // --- Helper methods to reduce duplication ---
        private void ValidatePassword(string password)
        {
            if (password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.");
            if (!password.Any(char.IsUpper))
                throw new ArgumentException("Password must contain at least one uppercase letter.");
            if (!password.Any(char.IsLower))
                throw new ArgumentException("Password must contain at least one lowercase letter.");
            if (!password.Any(char.IsDigit))
                throw new ArgumentException("Password must contain at least one digit.");
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                throw new ArgumentException("Password must contain at least one special character.");
        }

        private async void SendVerificationEmail(string email, string name, string token)
        {
            string frontendUrl;
            try
            {
                frontendUrl = _config["Frontend:Url"] ?? throw new InvalidOperationException("Frontend URL not configured.");
            }
            catch (InvalidOperationException ex)
            {
                // Optionally log the error here
                throw new Exception("Registration failed: Frontend URL is not configured on the server. Please contact support.", ex);
            }
            var verificationUrl = $"{frontendUrl}/verify-email?token={token}";
            var subject = "Verify your email address";
            var body = $@"
            <div style='font-family:Arial,sans-serif;'>
            <h2 style='color:#0070f3;'>Greetings from EventSphere!</h2>
            <p>Hi {name},</p>
            <p>Thank you for registering with <strong>EventSphere</strong>.</p>
            <p>To complete your registration, please verify your email address by clicking the button below:</p>
            <p style='margin:20px 0;'>
                <a href='{verificationUrl}' style='background:#0070f3;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px;'>Verify Email</a>
            </p>
            <p>If you did not sign up for EventSphere, please ignore this email.</p>
            <br>
            <p style='font-size:12px;color:#888;'>Best regards,<br>EventSphere Team</p>
            </div>
            ";
            var emailSender = new SmtpEmailSender(_config);
            await emailSender.SendEmailAsync(email, subject, body);
        }
    }
}
