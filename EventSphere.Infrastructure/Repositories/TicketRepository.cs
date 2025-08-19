using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Domain.Entities;
using EventSphere.Application.Services;
using backend.Data; // Adjust namespace as needed
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _dbContext;
        public TicketRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _dbContext.Tickets.ToListAsync();
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _dbContext.Tickets.FindAsync(id);
        }

        public async Task<Ticket> AddAsync(Ticket ticket)
        {
            _dbContext.Tickets.Add(ticket);
            await _dbContext.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> UpdateAsync(int id, Ticket ticket)
        {
            var existing = await _dbContext.Tickets.FindAsync(id);
            if (existing == null) return false;
            _dbContext.Entry(existing).CurrentValues.SetValues(ticket);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await _dbContext.Tickets.FindAsync(id);
            if (ticket == null) return false;
            _dbContext.Tickets.Remove(ticket);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
