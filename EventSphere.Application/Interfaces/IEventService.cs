using EventSphere.Application.Dtos;
using EventSphere.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
 
namespace backend.Interfaces
{
    public interface IEventService
    {
        /// <summary>
        /// Handles all file extraction, media file saving, and description placeholder replacement logic for event creation.
        /// </summary>
        Task<Event> MapAndCreateEventWithFilesAsync(EventSphere.Application.Dtos.Events.CreateEventDto dto, int organizerId, Microsoft.AspNetCore.Http.HttpRequest request);

        Task<Event> MapAndCreateEventAsync(EventSphere.Application.Dtos.Events.CreateEventDto dto, int organizerId, Microsoft.AspNetCore.Http.IFormFile? coverImage, Microsoft.AspNetCore.Http.IFormFile? vibeVideo, List<EventSphere.Application.Dtos.Events.MediaDto>? mediaDtos, List<Microsoft.AspNetCore.Http.IFormFile>? speakerPhotos, Microsoft.AspNetCore.Http.HttpRequest? request);
        Task<Event> CreateEventAsync(Event evt, Microsoft.AspNetCore.Http.IFormFile? coverImage, Microsoft.AspNetCore.Http.IFormFile? vibeVideo, List<EventSphere.Application.Dtos.Events.MediaDto>? mediaDtos, List<Microsoft.AspNetCore.Http.IFormFile>? speakerPhotos);
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event?> GetEventByIdAsync(int id);
        Task<EventDto?> GetEventByIdNewAsync(int id);
    Task<(IEnumerable<Event> Events, int TotalCount)> GetEventsPagedAsync(int page, int pageSize);

    Task<Event?> UpdateEventAsync(int id, EventSphere.Application.Dtos.Events.UpdateEventDto dto, List<Microsoft.AspNetCore.Http.IFormFile>? speakerPhotos = null, bool decrementEditCount = true);
        Task<bool> DeleteEventAsync(int id);

        Task<int?> GetEventEditCountAsync(int eventId);

        Task<int> GetRegistrationCountForEventAsync(int eventId);

    // Optimized method for admin page: get unapproved events paged
    Task<(IEnumerable<EventCardDto> Events, int TotalCount)> GetUnapprovedEventsPagedAsync(int page, int pageSize);


    }
}