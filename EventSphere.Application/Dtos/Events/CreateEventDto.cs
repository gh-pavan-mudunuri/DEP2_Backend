using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EventSphere.Application.Dtos.Events
{
    public class CreateEventDto
    {
        /// <summary>
        /// Used when Category is 'Other' to capture the custom category name.
        /// </summary>
        public string? EventType { get; set; }
        public string? OtherCategory { get; set; }
        public string? RecurrenceType { get; set; }
        public string? RecurrenceRule { get; set; } // iCal RRULE string for rule-based recurrence
        public string? Location { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Category { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public DateTime? RegistrationDeadline { get; set; }
        [Required]
        public DateTime? EventStart { get; set; }
        [Required]
        public DateTime? EventEnd { get; set; }
        [Required]
        public int OrganizerId { get; set; }
        public string? OrganizerName { get; set; }
        public bool IsPaidEvent { get; set; }
        public decimal? Price { get; set; }
        public int? MaxAttendees { get; set; }
        public string? OrganizerEmail { get; set; }
        public IFormFile? CoverImage { get; set; }
        public IFormFile? VibeVideo { get; set; }
        public List<SpeakerDto>? Speakers { get; set; }
        public List<FaqDto>? Faqs { get; set; }
        public List<MediaDto>? Media { get; set; }
        public List<OccurrenceDto>? Occurrences { get; set; }
        public string? EventLink { get; set; }
        // Add other fields as needed
    }

    public class OccurrenceDto
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? EventTitle { get; set; }
    }

    public class SpeakerDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }
        public string? SocialLinks { get; set; }
    }

    public class FaqDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }
    public class MediaDto
    {
        public string MediaUrl { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty; // "Image" or "Video"
        public string? Description { get; set; }
        public IFormFile? MediaFile { get; set; } // For image/video upload
    }
}
