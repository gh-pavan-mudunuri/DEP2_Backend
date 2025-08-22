
namespace EventSphere.Application.Dtos
{
    public class EventDto
    {
        public int EventId { get; set; }
        public int OrganizerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public string? VibeVideoUrl { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? EventType { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrenceType { get; set; }
        public string? RecurrenceRule { get; set; }
        public string? CustomFields { get; set; }
        public string? Description { get; set; }
        public bool IsPaidEvent { get; set; }
        public decimal? Price { get; set; }
        public string? EventLink { get; set; }
        public string? OrganizerName { get; set; }
        public string? OrganizerEmail { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("maxAttendees")]
        public int? MaxAttendees { get; set; }

        public int EditEventCount { get; set; }
        public List<EventOccurrenceDto>? Occurrences { get; set; }
        public List<EventSpeakerDto>? Speakers { get; set; }
        public List<EventFaqDto>? Faqs { get; set; }
    public List<EventMediaDto>? Media { get; set; }

    // Added: total tickets booked for this event
    public int RegistrationCount { get; set; }
        public bool IsVerifiedByAdmin { get; set; }
        public DateTime AdminVerifiedAt { get; internal set; }
    }
}
