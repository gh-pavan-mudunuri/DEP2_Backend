using EventSphere.Application.Dtos;

namespace backend.Interfaces
{
    public interface IHomeService
    {

        Task<IEnumerable<EventCardDto>> GetUpcomingEventsAsync();

        Task<IEnumerable<EventCardDto>> GetTrendingEventsAsync();
        
        Task<IEnumerable<EventCardDto>> FilterEventsAsync(EventFilterDto filter);

        
    }
}