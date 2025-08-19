using EventSphere.Domain.Entities;
using backend.Dtos;
using System.Threading.Tasks;
using EventSphere.Domain.Enums;

namespace backend.Services
{
    public interface IUserService
    {
        Task<User> RegisterAsync(UserRegisterDto dto, UserRole role );
        Task<User?> LoginAsync(UserLoginDto dto);
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();

        // Preferences
        Task<object> GetPreferencesAsync(int userId);
        Task<bool> SetPreferencesAsync(int userId, object preferences);

        // Notification settings
        Task<object> GetNotificationSettingsAsync(int userId);
        Task<bool> UpdateNotificationSettingsAsync(int userId, object settings);

        // Email verification
        Task<bool> VerifyEmailAsync(string token);
        // Forgot password
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<EventSphere.Application.Dtos.Auth.UserDetailsDto?> GetUserDetailsByIdAsync(int id);
        Task<bool> UpdateUserDetailsWithImageAsync(int id, string? name, string? email, string? phoneNumber, string? profileImagePath);
        Task<bool> DeleteUserAsync(int id);
    }
}