        
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using EventSphere.Domain.Entities;
using EventSphere.Application.Dtos.Events;
using EventSphere.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace EventSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class EventsController : ControllerBase
    {
        private readonly backend.Interfaces.IEventService _eventService;


        public EventsController(backend.Interfaces.IEventService eventService)
        {
            _eventService = eventService;
        }
            [HttpGet("paged")]
            public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            {
                var (events, totalCount) = await _eventService.GetEventsPagedAsync(page, pageSize);
                var eventDtos = events.Select(e => EventSphere.Application.Mappers.EventMapper.ToDto(e)).ToList();
                return Ok(new { success = true, data = eventDtos, totalCount });
            }



        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _eventService.GetAllEventsAsync();
            var eventDtos = events.Select(e => EventSphere.Application.Mappers.EventMapper.ToDto(e)).ToList();
            return Ok(new { success = true, data = eventDtos });
        }




    [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var evt = await _eventService.GetEventByIdNewAsync(id);
            if (evt == null)
                return NotFound(new { success = false, message = "Event not found" });
            return Ok(new { success = true, data = evt });
        }



        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateEventDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid event data" });

            // Use AuthHelper for organizerId extraction
            var organizerId = EventSphere.Application.Helpers.AuthHelper.GetOrganizerIdFromClaims(HttpContext);
            if (organizerId == null)
                return Unauthorized(new { success = false, message = "User not authenticated or invalid user id in token" });

            // Pass all files and DTO to the service for processing (service will handle form reading and DTO population)
            var evt = await _eventService.MapAndCreateEventWithFilesAsync(dto, organizerId.Value, Request);

            // Return all relevant event details for frontend validation and display
            return Ok(new
            {
                coverImageUrl = evt.CoverImage,
                vibeVideoUrl = evt.VibeVideoUrl,
                eventId = evt.EventId,
                title = evt.Title,
                category = evt.Category,
                description = evt.Description,
                organizerName = evt.OrganizerName,
                eventLink = evt.EventLink,
                recurrenceType = evt.RecurrenceType,
                location = evt.Location,
                occurrences = evt.Occurrences?.Select(o => new
                {
                    o.OccurrenceId,
                    o.StartTime,
                    o.EndTime,
                    o.EventTitle,
                    o.IsCancelled
                }).ToList()
            });
        }



        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]

        public async Task<IActionResult> Update(int id, [FromForm] EventSphere.API.Dtos.UpdateEventApiDto apiDto)
        {
            if (apiDto == null)
                return BadRequest(new { success = false, message = "Invalid event data" });
            // Map API DTO to Application DTO
            var dto = new EventSphere.Application.Dtos.Events.UpdateEventDto
            {
                Title = apiDto.Title,
                Description = apiDto.Description,
                Category = apiDto.Category,
                EventType = apiDto.EventType,
                Location = apiDto.Location,
                RegistrationDeadline = apiDto.RegistrationDeadline,
                EventStart = apiDto.EventStart,
                EventEnd = apiDto.EventEnd,
                IsPaidEvent = apiDto.IsPaidEvent,
                Price = apiDto.Price,
                MaxAttendees = apiDto.MaxAttendees,
                OrganizerName = apiDto.OrganizerName,
                OrganizerEmail = apiDto.OrganizerEmail,
                EventLink = apiDto.EventLink,
                RecurrenceType = apiDto.RecurrenceType,
                RecurrenceRule = apiDto.RecurrenceRule,
                CustomFields = apiDto.CustomFields,
                Occurrences = apiDto.Occurrences,
                Speakers = apiDto.Speakers,
                Faqs = apiDto.Faqs,
                Media = apiDto.Media,
                CoverImage = apiDto.CoverImage,
                VibeVideo = apiDto.VibeVideo
            };
            // Extract speaker photo files from the request
            var speakerPhotos = new List<IFormFile>();
            foreach (var file in Request.Form.Files)
            {
                if (file.Name.StartsWith("speakers["))
                {
                    speakerPhotos.Add(file);
                }
            }
            try
            {
                var updated = await _eventService.UpdateEventAsync(id, dto, speakerPhotos);
                if (updated == null)
                    return NotFound(new { success = false, message = "Event not found" });
                return Ok(new { success = true, data = updated });
            }
            catch (Exception ex) 
            {
                return StatusCode(400, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Check registration count before deleting
            var registrationCount = await _eventService.GetRegistrationCountForEventAsync(id);
            if (registrationCount > 0)
                return BadRequest(new { success = false, message = "Cannot delete event with existing registrations." });

            var deleted = await _eventService.DeleteEventAsync(id);
            if (!deleted)
                return NotFound(new { success = false, message = "Event not found" });
            return Ok(new { success = true, message = "Event deleted successfully" });
        }

        [HttpGet("editeventcount/{id}")]
        public async Task<IActionResult> GetEditEventCount(int id)
        {
            var count = await _eventService.GetEventEditCountAsync(id);
            if (count == null)
                return NotFound(new { success = false, message = "Event not found" });
            return Ok(new { success = true, eventId = id, editEventCount = count });
        }

        



    }
}
