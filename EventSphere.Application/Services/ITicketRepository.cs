using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Domain.Entities;

namespace EventSphere.Application.Services
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket> AddAsync(Ticket ticket);
        Task<bool> UpdateAsync(int id, Ticket ticket);
        Task<bool> DeleteAsync(int id);
    }
}
