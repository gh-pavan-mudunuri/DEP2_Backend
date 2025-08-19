using System;
using EventSphere.Domain.Enums;

namespace EventSphere.Application.Dtos
{
    public class EventCardDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public EventType EventType { get; set; }
        public string? Location { get; set; }
        public int EditEventCount { get; set; } = 0;
        public DateTime RegistrationDeadline { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public decimal? Price { get; set; }
        public int RegistrationCount { get; set; } = 0;
    public bool IsVerifiedByAdmin { get; set; }
    public string? OrganizerEmail { get; set; }
    }
}
