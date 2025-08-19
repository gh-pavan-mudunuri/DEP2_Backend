using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventSphere.Application.Repositories;
using EventSphere.Domain.Entities;
using backend.Data;

namespace EventSphere.Infrastructure.Repositories
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly AppDbContext _context;

        public RegistrationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateAsync(int id, Registration registration)
        {
            var existing = await _context.Registrations.FindAsync(id);
            if (existing == null) return false;
            // Update QR code and PaymentId only, do not overwrite other fields
            existing.QrCode = registration.QrCode;
            existing.PaymentId = registration.PaymentId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Registrations.FindAsync(id);
            if (existing == null) return false;
            _context.Registrations.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Registration> AddAsync(Registration registration)
        {
            try
            {
                await _context.Registrations.AddAsync(registration);
                await _context.SaveChangesAsync();
                return registration;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to add registration: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<Registration>> GetAllAsync()
        {
            return await _context.Registrations.ToListAsync();
        }

        public async Task<Registration?> GetByIdAsync(int id)
        {
            return await _context.Registrations.FindAsync(id);
        }
    }
}