using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace EventSphere.Application.Dtos.Events
{
    public class UpdateEventDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? EventType { get; set; }
        public string? Location { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public DateTime? EventStart { get; set; }
        public DateTime? EventEnd { get; set; }
        public bool? IsPaidEvent { get; set; }
        public decimal? Price { get; set; }
        public int? MaxAttendees { get; set; }
        public int IsVerifiedByAdmin { get; set; } = 0;
        public string? OrganizerName { get; set; }
        public string? OrganizerEmail { get; set; }
        public string? EventLink { get; set; }
        public string? RecurrenceType { get; set; }
        public string? RecurrenceRule { get; set; }
        public string? CustomFields { get; set; }
        public List<OccurrenceDto>? Occurrences { get; set; }
        public List<SpeakerDto>? Speakers { get; set; }
        public List<FaqDto>? Faqs { get; set; }
        public List<MediaDto>? Media { get; set; }
        public IFormFile? CoverImage { get; set; }
        public IFormFile? VibeVideo { get; set; }
    }
}
