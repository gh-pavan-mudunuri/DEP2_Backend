using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventSphere.Domain.Enums;
namespace EventSphere.Domain.Entities
{
    // Enums moved to Enums/EventEnums.cs
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [ForeignKey("Organizer")]
        public int OrganizerId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string CoverImage { get; set; } = string.Empty;

        public int IsCompleted { get; set; } = 0;


        public string? VibeVideoUrl { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public EventType EventType { get; set; }

        public string? Location { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        [Required]
        public DateTime EventStart { get; set; }

        [Required]
        public DateTime EventEnd { get; set; }

        // Recurrence fields
        public bool IsRecurring { get; set; } = false;
        public string? RecurrenceType { get; set; } // 'rule', 'custom', or null
        public string? RecurrenceRule { get; set; } // iCal-style rule or null
        public string? CustomFields { get; set; } // JSON metadata

        // For category 'Others', store the custom category value

        // Direct update method for recurrence and organizer fields
        public void UpdateRecurrence(string? recurrenceType, string? recurrenceRule, string? customFields)
        {
            IsRecurring = !string.IsNullOrEmpty(recurrenceType);
            RecurrenceType = recurrenceType;
            RecurrenceRule = recurrenceRule;
            CustomFields = customFields;
        }

        public void UpdateOrganizer(string? organizerName, string? eventLink)
        {
            OrganizerName = organizerName!;
            EventLink = eventLink;
        }

        [Required]
        public string Description { get; set; } = string.Empty;

        public bool IsPaidEvent { get; set; } = false;
        public decimal? Price { get; set; }
        public bool IsVerifiedByAdmin { get; set; } = false;
        public DateTime? AdminVerifiedAt { get; set; }
        public EventStatus Status { get; set; } = EventStatus.Draft;
        public bool PlatformFeePaid { get; set; } = false;
        public int? MaxAttendees { get; set; }

        public int EditEventCount { get; set; } = 3;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        // New fields
        public string OrganizerName { get; set; } = string.Empty;
        public string? EventLink { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // Navigation and workflow fields
        public string? OrganizerEmail { get; set; } // For notifications/emails
        public string? AdminComments { get; set; } // For admin workflow
        public virtual User? Organizer { get; set; }
        public virtual ICollection<Ticket>? Tickets { get; set; }
        public virtual ICollection<Registration>? Registrations { get; set; }
        public virtual ICollection<EventOccurrence>? Occurrences { get; set; }
        public virtual ICollection<EventMedia>? Media { get; set; }
        public virtual ICollection<EventFaq>? Faqs { get; set; }
        public virtual ICollection<EventSpeaker>? Speakers { get; set; }
        public virtual ICollection<Bookmark>? Bookmarks { get; set; }
        public virtual ICollection<Notification>? Notifications { get; set; }
    }
}
