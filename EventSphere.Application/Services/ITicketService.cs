using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Domain.Entities;

namespace EventSphere.Application.Services
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket> CreateAsync(Ticket ticket);
        Task<bool> UpdateAsync(int id, Ticket ticket);
        Task<bool> DeleteAsync(int id);
        // Add more methods as needed (e.g., create tickets for registration)
    }
}
