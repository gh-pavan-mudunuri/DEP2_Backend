using System.Threading.Tasks;
using EventSphere.Application.Interfaces;
using EventSphere.Infrastructure.Repositories;

namespace EventSphere.Application.Services
{
    public class StatsService : IStatsService
    {
        private readonly StatsRepository _repo;
        public StatsService(StatsRepository repo)
        {
            _repo = repo;
        }

        public Task<int> GetUserCountAsync() => _repo.GetUserCountAsync();
        public Task<int> GetEventCountAsync() => _repo.GetEventCountAsync();
        public Task<int> GetRegistrationCountAsync() => _repo.GetRegistrationCountAsync();
        public Task<int> GetLocationCountAsync() => _repo.GetLocationCountAsync();
        public Task<int> GetReviewCountAsync() => _repo.GetReviewCountAsync();
        public Task<double> GetAvgRatingAsync() => _repo.GetAvgRatingAsync();
    }
}
