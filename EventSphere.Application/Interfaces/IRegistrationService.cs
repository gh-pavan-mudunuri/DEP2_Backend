using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Application.Dtos.Registrations;

namespace EventSphere.Application.Interfaces
{
    public interface IRegistrationService
    {
        Task<IEnumerable<RegistrationDto>> GetAllAsync();
        Task<RegistrationDto?> GetByIdAsync(int id);
        Task<RegistrationDto> CreateAsync(RegistrationRequestDto registrationDto);
        Task<bool> UpdateAsync(int id, RegistrationRequestDto registrationDto);
        Task<bool> DeleteAsync(int id);
    }
}
