using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Application.Dtos;

namespace EventSphere.Application.Interfaces
{
    public interface IDashboardService
    {


    Task<IEnumerable<EventCardDto>> GetCurrentOrganizedEventsAsync(int organizerId);
    Task<IEnumerable<EventCardDto>> GetPastOrganizedEventsAsync(int organizerId);

    Task<IEnumerable<EventCardDto>> GetCurrentAttendingEventsAsync(int userId);
    Task<IEnumerable<EventCardDto>> GetPastAttendedEventsAsync(int userId);

    Task<IEnumerable<EventSphere.Application.Dtos.Events.OrganizedEventPaymentsDto>> GetOrganizedEventsPaymentsAsync(int organizerId);



    
    }
}
