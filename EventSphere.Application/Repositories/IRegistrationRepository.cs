using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Domain.Entities;

namespace EventSphere.Application.Repositories
{
    public interface IRegistrationRepository
    {
        Task<Registration> AddAsync(Registration registration);
        Task<IEnumerable<Registration>> GetAllAsync();
        Task<Registration?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, Registration registration);
        Task<bool> DeleteAsync(int id);
    }
}
