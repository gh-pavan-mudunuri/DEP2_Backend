using backend.Interfaces;
using EventSphere.Application.Dtos.Events;
using EventSphere.Application.Repositories;
using EventSphere.Domain.Entities;
using EventSphere.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services
{
    public class AdminService : IAdminService
    {
        private readonly IEventRepository _eventRepository;

        public AdminService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<IActionResult> AproveEventEditAsync(int id)
{
    var existing = await _eventRepository.GetEventByIdNewAsync(id);
    if (existing == null)
        throw new KeyNotFoundException("Event not found");

    // Only apply if there are pending edits stored in CustomFields
    if (!string.IsNullOrWhiteSpace(existing.CustomFields))
    {
        try
        {
            var dto = System.Text.Json.JsonSerializer.Deserialize<UpdateEventDto>(existing.CustomFields);

            // Apply changes like in UpdateEventAsync
            if (dto.Title != null) existing.Title = dto.Title;
            if (dto.Description != null) existing.Description = dto.Description;
            if (dto.Category != null) existing.Category = dto.Category;
            if (dto.EventType != null)
                existing.EventType = dto.EventType;
            if (dto.Location != null) existing.Location = dto.Location;
            if (dto.RegistrationDeadline.HasValue) existing.RegistrationDeadline = dto.RegistrationDeadline.Value;
            if (dto.EventStart.HasValue) existing.EventStart = dto.EventStart.Value;
            if (dto.EventEnd.HasValue) existing.EventEnd = dto.EventEnd.Value;
            if (dto.IsPaidEvent.HasValue) existing.IsPaidEvent = dto.IsPaidEvent.Value;
            if (dto.Price.HasValue) existing.Price = dto.Price.Value;
            if (dto.MaxAttendees.HasValue) existing.MaxAttendees = dto.MaxAttendees.Value;
            if (dto.OrganizerName != null) existing.OrganizerName = dto.OrganizerName;
            if (dto.OrganizerEmail != null) existing.OrganizerEmail = dto.OrganizerEmail;
            if (dto.EventLink != null) existing.EventLink = dto.EventLink;
            if (dto.RecurrenceType != null) existing.RecurrenceType = dto.RecurrenceType;
            if (dto.RecurrenceRule != null) existing.RecurrenceRule = dto.RecurrenceRule;
            if (!string.IsNullOrWhiteSpace(dto.CustomFields)) existing.CustomFields = dto.CustomFields;

            // Clear pending changes after applying
            existing.CustomFields = null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse pending event changes: {ex.Message}");
        }
    }

    // Mark as verified and reset count
    existing.IsVerifiedByAdmin = true;
    existing.AdminVerifiedAt = DateTime.UtcNow;
    existing.EditEventCount = 0;

    var eventEntity = new Event
{
    EventId = existing.EventId,
    OrganizerId = existing.OrganizerId,
    Title = existing.Title,
    CoverImage = existing.CoverImage,
    VibeVideoUrl = existing.VibeVideoUrl,
    Category = existing.Category,
    Location = existing.Location,
    EventType = Enum.TryParse<EventType>(existing.EventType, out var eventTypeEnum) ? eventTypeEnum : EventType.TBA,
    RegistrationDeadline = existing.RegistrationDeadline,
    EventStart = existing.EventStart,
    EventEnd = existing.EventEnd,
    IsRecurring = existing.IsRecurring,
    RecurrenceType = existing.RecurrenceType,
    RecurrenceRule = existing.RecurrenceRule,
    CustomFields = existing.CustomFields,
    Description = existing.Description,
    IsPaidEvent = existing.IsPaidEvent,
    Price = existing.Price,
    EventLink = existing.EventLink,
    OrganizerName = existing.OrganizerName,
    OrganizerEmail = existing.OrganizerEmail,
    MaxAttendees = existing.MaxAttendees,
    EditEventCount = existing.EditEventCount,
    IsVerifiedByAdmin = existing.IsVerifiedByAdmin,
    AdminVerifiedAt = existing.AdminVerifiedAt
    // Add mapping for child collections if needed
};

    await _eventRepository.UpdateEventAsync(eventEntity);

    return new OkObjectResult(new { success = true, message = "Event edit approved" });
}

        
    }
}