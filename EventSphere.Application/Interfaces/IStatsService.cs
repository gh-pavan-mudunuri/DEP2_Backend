using System.Threading.Tasks;

namespace EventSphere.Application.Interfaces
{
    public interface IStatsService
    {
        Task<int> GetUserCountAsync();
        Task<int> GetEventCountAsync();
        Task<int> GetRegistrationCountAsync();
        Task<int> GetLocationCountAsync();
        Task<int> GetReviewCountAsync();
        Task<double> GetAvgRatingAsync();
    }
}
