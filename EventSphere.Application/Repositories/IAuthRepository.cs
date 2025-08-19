using System.Threading.Tasks;
using EventSphere.Domain.Entities;

namespace EventSphere.Application.Repositories
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByVerificationTokenAsync(string token);
        Task<User?> GetUserByPasswordResetTokenAsync(string token);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }
}
