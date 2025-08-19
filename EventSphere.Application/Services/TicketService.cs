using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Domain.Entities;
using EventSphere.Application.Services;
using EventSphere.Infrastructure.Repositories; // Adjust if needed

namespace EventSphere.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _ticketRepository.GetAllAsync();
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _ticketRepository.GetByIdAsync(id);
        }

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            return await _ticketRepository.AddAsync(ticket);
        }

        public async Task<bool> UpdateAsync(int id, Ticket ticket)
        {
            return await _ticketRepository.UpdateAsync(id, ticket);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _ticketRepository.DeleteAsync(id);
        }
    }
}
