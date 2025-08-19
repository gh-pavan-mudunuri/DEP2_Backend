using backend.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EventSphere.Infrastructure.Repositories
{
    public class StatsRepository
    {
        private readonly AppDbContext _context;
        public StatsRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<int> GetUserCountAsync() => Task.FromResult(_context.Users.Count());
        public Task<int> GetEventCountAsync() => Task.FromResult(_context.Events.Count());
        public Task<int> GetRegistrationCountAsync() => Task.FromResult(_context.Registrations.Count());
        public Task<int> GetLocationCountAsync() => Task.FromResult(_context.Events.Select(e => e.Location).Distinct().Count());
        public Task<int> GetReviewCountAsync() => Task.FromResult(_context.WebsiteReviews.Count());
        public Task<double> GetAvgRatingAsync() => Task.FromResult(_context.WebsiteReviews.Count() > 0 ? _context.WebsiteReviews.Average(r => r.Rating) : 0);
    }
}
