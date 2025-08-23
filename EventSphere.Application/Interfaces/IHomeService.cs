using EventSphere.Application.Dtos;

namespace backend.Interfaces
{
    public interface IHomeService
    {

        Task<IEnumerable<EventCardDto>> GetUpcomingEventsAsync();

        Task<IEnumerable<EventCardDto>> GetTrendingEventsAsync();

        Task<IEnumerable<EventCardDto>> FilterEventsAsync(EventFilterDto filter);

        Task<(IEnumerable<EventCardDto> Events, int TotalCount)> FilterEventsPagedAsync(EventFilterDto filter, int page = 1, int pageSize = 20);


    }
}