using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Domain.Entities;
using EventSphere.Application.Dtos;

namespace EventSphere.Application.Repositories
{
    public interface IEventRepository
    {
        Task<EventDto?> GetEventByIdNewAsync(int id);

        Task<Event?> GetEventByIdAsync(int id);

        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<(IEnumerable<Event> Events, int TotalCount)> GetEventsPagedAsync(int page, int pageSize);

        Task<List<EventCardDto>> GetCurrentOrganizedEventsAsync(int organizerId);

        Task AddEventAsync(Event ev);
        Task UpdateEventAsync(Event ev);
        Task<List<Event>> GetUpcomingEventsAsync();

        Task<List<Event>> GetTrendingEventsAsync();


        Task<bool> DeleteEventAsync(int id);
        // Add more event-related methods as needed

        Task<int> GetRegistrationCountForEventAsync(int eventId);
        Task<IEnumerable<EventCardDto>> FilterEventsAsync(EventFilterDto filter);

        Task<Dictionary<int, int>> GetRegistrationCountsForEventsAsync(List<int> eventIds);

        Task<int?> GetEventEditCountAsync(int eventId);

        Task<List<int>> GetRegisteredEventIdsByUserIdAsync(int userId);

        // Get all registrations for an event (for notifications)
        Task<List<Registration>> GetRegistrationsByEventIdAsync(int eventId);

        // Optimized method for admin page: get unapproved events paged
        Task<(IEnumerable<EventCardDto> Events, int TotalCount)> GetUnapprovedEventsPagedAsync(int page, int pageSize);


        Task<(IEnumerable<EventCardDto> Events, int TotalCount)> FilterEventsPagedAsync(EventFilterDto filter, int page = 1, int pageSize = 20);

    }
}
